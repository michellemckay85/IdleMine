using System;
using GoldAndGoblins.Ads;
using GoldAndGoblins.Core;
using GoldAndGoblins.Mining;
using UnityEngine;
using UnityEngine.Purchasing;

namespace GoldAndGoblins.IAP
{
    /// <summary>
    /// Wraps Unity IAP (com.unity.purchasing) for both the App Store and Google Play using a
    /// single product catalog (IAPProductCatalog). Attach once to the GameManager object.
    ///
    /// Setup required in the Unity Editor (see docs/SETUP_GUIDE.md):
    ///  - Window > Package Manager > In App Purchasing must be installed (already declared in
    ///    Packages/manifest.json).
    ///  - Product IDs below must be created identically in App Store Connect and Play Console.
    ///
    /// Note on subscriptions: this client tracks Goblin Ward's active/expiry state locally from
    /// the purchase receipt for gameplay purposes. For real entitlement enforcement (e.g. to gate
    /// server-side content) validate the receipt server-side against Apple/Google, since a
    /// jailbroken/rooted client can fake local state.
    /// </summary>
    public class IAPManager : MonoBehaviour, IDetailedStoreListener
    {
        public static IAPManager Instance { get; private set; }

        [SerializeField] private CurrencyManager currencyManager;
        [SerializeField] private GoblinRaidManager goblinRaidManager;
        [SerializeField] private AdsGateway adsGateway;

        public event Action<string> OnPurchaseSucceeded;
        public event Action<string, string> OnPurchaseFailedEvent; // (productId, reason)
        public event Action OnStoreInitialized;
        public event Action<string> OnStoreInitializeFailed;

        private IStoreController _storeController;
        private IExtensionProvider _extensionProvider;

        public bool IsInitialized => _storeController != null && _extensionProvider != null;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            InitializePurchasing();
        }

        private void InitializePurchasing()
        {
            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

            foreach (IAPProductDefinition def in IAPProductCatalog.All)
            {
                builder.AddProduct(def.id, def.type);
            }

            UnityPurchasing.Initialize(this, builder);
        }

        public void BuyProduct(string productId)
        {
            if (!IsInitialized)
            {
                Debug.LogWarning("[IAPManager] Store not initialized yet.");
                OnPurchaseFailedEvent?.Invoke(productId, "Store not ready. Please try again shortly.");
                return;
            }

            Product product = _storeController.products.WithID(productId);
            if (product == null || !product.availableToPurchase)
            {
                Debug.LogWarning($"[IAPManager] Product not available: {productId}");
                OnPurchaseFailedEvent?.Invoke(productId, "Product not available.");
                return;
            }

            _storeController.InitiatePurchase(product);
        }

        /// <summary>Required on iOS by App Store guidelines for non-consumables/subscriptions.</summary>
        public void RestorePurchases(Action<bool> onComplete = null)
        {
            if (!IsInitialized)
            {
                onComplete?.Invoke(false);
                return;
            }

#if UNITY_IOS
            var apple = _extensionProvider.GetExtension<IAppleExtensions>();
            apple.RestoreTransactions(onComplete);
#else
            // Google Play restores non-consumable/subscription ownership automatically on
            // initialization; nothing extra to trigger here.
            onComplete?.Invoke(true);
#endif
        }

        public string GetLocalizedPriceString(string productId)
        {
            if (!IsInitialized) return string.Empty;
            Product product = _storeController.products.WithID(productId);
            return product != null ? product.metadata.localizedPriceString : string.Empty;
        }

        // ---------------- IDetailedStoreListener ----------------

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            _storeController = controller;
            _extensionProvider = extensions;
            Debug.Log("[IAPManager] Store initialized.");
            ReapplyOwnedNonConsumables();
            OnStoreInitialized?.Invoke();
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            OnInitializeFailed(error, null);
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            string reason = $"{error} {message}".Trim();
            Debug.LogError($"[IAPManager] Initialize failed: {reason}");
            OnStoreInitializeFailed?.Invoke(reason);
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            string productId = args.purchasedProduct.definition.id;

            if (!IAPProductCatalog.TryGet(productId, out IAPProductDefinition def))
            {
                Debug.LogWarning($"[IAPManager] Unknown product purchased: {productId}");
                return PurchaseProcessingResult.Complete;
            }

            ApplyReward(def);
            OnPurchaseSucceeded?.Invoke(productId);
            return PurchaseProcessingResult.Complete;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            OnPurchaseFailed(product, new PurchaseFailureDescription(product.definition.id, failureReason, failureReason.ToString()));
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
        {
            Debug.LogWarning($"[IAPManager] Purchase failed: {product.definition.id} - {failureDescription.reason}");
            OnPurchaseFailedEvent?.Invoke(product.definition.id, failureDescription.reason.ToString());
        }

        // ---------------- Reward application ----------------

        private void ApplyReward(IAPProductDefinition def)
        {
            switch (def.reward)
            {
                case RewardKind.Gold:
                    currencyManager.AddGoldRaw(def.goldAmount);
                    break;

                case RewardKind.Gems:
                    currencyManager.AddGems(def.gemAmount);
                    break;

                case RewardKind.RemoveAds:
                    currencyManager.MarkRemoveAdsPurchased();
                    if (adsGateway != null) adsGateway.SetAdsRemoved(true);
                    break;

                case RewardKind.VipBundle:
                    currencyManager.MarkVipBundlePurchased();
                    currencyManager.AddGoldRaw(def.goldAmount);
                    currencyManager.AddGems(def.gemAmount);
                    currencyManager.SetPermanentGoldMultiplier(def.permanentGoldMultiplier);
                    break;

                case RewardKind.GoblinWardSubscription:
                    // Client-side heuristic expiry; see class doc comment re: server-side validation.
                    DateTime expiry = DateTime.UtcNow.AddDays(31);
                    if (goblinRaidManager != null) goblinRaidManager.SetGoblinWardActive(true, expiry);
                    break;
            }

            GameManager.Instance?.SaveNow();
        }

        /// <summary>
        /// On store init, Product.hasReceipt tells us which non-consumables/subscriptions this
        /// player already owns (e.g. reinstall, new device). Re-grant entitlements that don't
        /// already have a save flag set so state matches the store.
        /// </summary>
        private void ReapplyOwnedNonConsumables()
        {
            foreach (Product product in _storeController.products.all)
            {
                if (!product.hasReceipt) continue;
                if (!IAPProductCatalog.TryGet(product.definition.id, out IAPProductDefinition def)) continue;

                switch (def.reward)
                {
                    case RewardKind.RemoveAds:
                        if (!currencyManager.RemoveAdsPurchased)
                        {
                            currencyManager.MarkRemoveAdsPurchased();
                            if (adsGateway != null) adsGateway.SetAdsRemoved(true);
                        }
                        break;

                    case RewardKind.VipBundle:
                        if (!currencyManager.VipBundlePurchased)
                        {
                            currencyManager.MarkVipBundlePurchased();
                            currencyManager.SetPermanentGoldMultiplier(def.permanentGoldMultiplier);
                        }
                        break;

                    case RewardKind.GoblinWardSubscription:
                        if (goblinRaidManager != null && !goblinRaidManager.GoblinWardActive)
                        {
                            goblinRaidManager.SetGoblinWardActive(true, DateTime.UtcNow.AddDays(31));
                        }
                        break;
                }
            }
        }
    }
}
