using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using GoldAndGoblins.Core;

namespace GoldAndGoblins.Economy
{
    public class IAPManager : GoldAndGoblins.Utils.Singleton<IAPManager>, IDetailedStoreListener
    {
        [SerializeField] private ProductCatalogSO catalog;

        private IStoreController storeController;
        private IExtensionProvider extensionProvider;
        private IReceiptValidator receiptValidator = new TrustClientReceiptValidator();

        private readonly Dictionary<string, IAPProductSO> productsById = new Dictionary<string, IAPProductSO>();

        public bool IsInitialized => storeController != null && extensionProvider != null;

        protected override void Awake()
        {
            base.Awake();
            InitializePurchasing();
        }

        public void SetReceiptValidator(IReceiptValidator validator) => receiptValidator = validator;

        private void InitializePurchasing()
        {
            if (IsInitialized || catalog == null) return;

            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
            foreach (var product in catalog.products)
            {
                productsById[product.productId] = product;
                builder.AddProduct(product.productId, product.productType);
            }

            UnityPurchasing.Initialize(this, builder);
        }

        public string GetLocalizedPriceString(string productId)
        {
            if (!IsInitialized) return "";
            var product = storeController.products.WithID(productId);
            return product != null && product.availableToPurchase ? product.metadata.localizedPriceString : "";
        }

        public bool IsProductOwned(string productId)
        {
            var data = SaveManager.Instance.Current;
            return data.ownedNonConsumableProductIds.Contains(productId);
        }

        public void BuyProduct(string productId)
        {
            if (!IsInitialized)
            {
                Debug.LogWarning("[IAPManager] Store not initialized yet.");
                return;
            }

            var product = storeController.products.WithID(productId);
            if (product == null || !product.availableToPurchase)
            {
                Debug.LogWarning($"[IAPManager] Product not available: {productId}");
                EventBus.Publish(new PurchaseFailedEvent(productId, "unavailable"));
                return;
            }

            storeController.InitiatePurchase(product);
        }

        public void RestorePurchases(System.Action<bool> onComplete = null)
        {
#if UNITY_IOS
            var apple = extensionProvider.GetExtension<IAppleExtensions>();
            apple.RestoreTransactions(onComplete);
#else
            onComplete?.Invoke(true); // Google Play restores automatically via account purchase history.
#endif
        }

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            storeController = controller;
            extensionProvider = extensions;
            Debug.Log("[IAPManager] Store initialized.");
        }

        public void OnInitializeFailed(InitializationFailureReason error) =>
            Debug.LogError($"[IAPManager] Init failed: {error}");

        public void OnInitializeFailed(InitializationFailureReason error, string message) =>
            Debug.LogError($"[IAPManager] Init failed: {error} - {message}");

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            var productId = args.purchasedProduct.definition.id;

            if (!receiptValidator.Validate(args.purchasedProduct))
            {
                Debug.LogWarning($"[IAPManager] Receipt failed validation for {productId}");
                EventBus.Publish(new PurchaseFailedEvent(productId, "receipt_invalid"));
                return PurchaseProcessingResult.Pending;
            }

            if (productsById.TryGetValue(productId, out var product))
            {
                GrantReward(product);
            }

            SaveManager.Instance.Save();
            EventBus.Publish(new PurchaseCompletedEvent(productId));
            return PurchaseProcessingResult.Complete;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            EventBus.Publish(new PurchaseFailedEvent(product.definition.id, failureReason.ToString()));
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
        {
            EventBus.Publish(new PurchaseFailedEvent(product.definition.id, failureDescription.message));
        }

        private void GrantReward(IAPProductSO product)
        {
            var data = SaveManager.Instance.Current;

            switch (product.rewardKind)
            {
                case RewardKind.Gems:
                    CurrencyManager.Instance.AddGems(product.gemAmount);
                    break;
                case RewardKind.Gold:
                    CurrencyManager.Instance.AddGold(product.goldAmount);
                    break;
                case RewardKind.RemoveAds:
                    data.removeAdsPurchased = true;
                    break;
                case RewardKind.VipPass:
                    // VIP = permanent 2x gold + no interstitial ads.
                    data.vipActive = true;
                    data.removeAdsPurchased = true;
                    CurrencyManager.Instance.RefreshPersistentMultipliers(
                        CurrencyManager.Instance.PrestigeGoldMultiplier,
                        vipActive: true);
                    break;
                case RewardKind.StarterBundle:
                    CurrencyManager.Instance.AddGems(product.gemAmount + product.bonusGems);
                    CurrencyManager.Instance.AddGold(product.goldAmount);
                    break;
            }

            if (product.productType == ProductType.NonConsumable && !data.ownedNonConsumableProductIds.Contains(product.productId))
            {
                data.ownedNonConsumableProductIds.Add(product.productId);
            }
        }
    }
}
