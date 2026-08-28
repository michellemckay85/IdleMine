using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GoldAndGoblins.Economy;

namespace GoldAndGoblins.UI
{
    public class ShopRowController : MonoBehaviour
    {
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private TMP_Text priceText;
        [SerializeField] private Button buyButton;

        private IAPProductSO product;

        public void Wire(TMP_Text name, TMP_Text description, TMP_Text price, Button buy)
        {
            nameText = name;
            descriptionText = description;
            priceText = price;
            buyButton = buy;
        }

        private void Awake() => UiRowLayoutFix.FixRow(transform);

        public void Bind(IAPProductSO productData)
        {
            product = productData;
            if (nameText != null) nameText.text = product.displayName;
            if (descriptionText != null) descriptionText.text = product.description;

            if (priceText != null)
            {
                var localizedPrice = IAPManager.Instance != null
                    ? IAPManager.Instance.GetLocalizedPriceString(product.productId)
                    : "";
                priceText.text = string.IsNullOrEmpty(localizedPrice) ? "Buy" : localizedPrice;
            }

            if (buyButton != null)
            {
                buyButton.onClick.RemoveAllListeners();
                buyButton.onClick.AddListener(() =>
                {
                    if (IAPManager.Instance != null && product != null)
                    {
                        IAPManager.Instance.BuyProduct(product.productId);
                    }
                });
            }

            UiRowLayoutFix.FixRow(transform);
        }
    }
}
