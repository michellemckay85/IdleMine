using GoldAndGoblins.IAP;
using UnityEngine;

namespace GoldAndGoblins.UI
{
    /// <summary>
    /// Refreshes every ShopItemUI's localized price once the store finishes initializing, and
    /// again after any purchase (in case store state such as ownership changes what's shown).
    /// Assign every ShopItemUI in the shop panel here in the Inspector.
    /// </summary>
    public class ShopUI : MonoBehaviour
    {
        [SerializeField] private IAPManager iapManager;
        [SerializeField] private ShopItemUI[] shopItems;

        private void OnEnable()
        {
            iapManager.OnStoreInitialized += HandleStoreInitialized;
            iapManager.OnPurchaseSucceeded += HandlePurchaseSucceeded;

            if (iapManager.IsInitialized) HandleStoreInitialized();
        }

        private void OnDisable()
        {
            iapManager.OnStoreInitialized -= HandleStoreInitialized;
            iapManager.OnPurchaseSucceeded -= HandlePurchaseSucceeded;
        }

        private void HandleStoreInitialized()
        {
            foreach (ShopItemUI item in shopItems)
            {
                item.RefreshPrice();
                item.SetInteractable(true);
            }
        }

        private void HandlePurchaseSucceeded(string productId)
        {
            foreach (ShopItemUI item in shopItems)
            {
                if (item.ProductId == productId) item.RefreshPrice();
            }
        }
    }
}
