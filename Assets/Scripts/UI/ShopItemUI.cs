using GoldAndGoblins.IAP;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GoldAndGoblins.UI
{
    /// <summary>One row/card in the shop: title, price button. Place one per product on a prefab or scene row.</summary>
    public class ShopItemUI : MonoBehaviour
    {
        [SerializeField] private string productId;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text priceText;
        [SerializeField] private Button buyButton;

        public string ProductId => productId;

        private void Awake()
        {
            if (buyButton != null) buyButton.onClick.AddListener(HandleBuyClicked);
        }

        private void HandleBuyClicked()
        {
            IAPManager.Instance?.BuyProduct(productId);
        }

        public void RefreshPrice()
        {
            if (priceText == null || IAPManager.Instance == null) return;
            string price = IAPManager.Instance.GetLocalizedPriceString(productId);
            priceText.text = string.IsNullOrEmpty(price) ? "--" : price;
        }

        public void SetInteractable(bool interactable)
        {
            if (buyButton != null) buyButton.interactable = interactable;
        }
    }
}
