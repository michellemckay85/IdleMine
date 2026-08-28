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

        public void Bind(IAPProductSO product)
        {
            if (nameText != null) nameText.text = product.displayName;
            if (descriptionText != null) descriptionText.text = product.description;

            if (priceText != null)
            {
                var localizedPrice = IAPManager.Instance.GetLocalizedPriceString(product.productId);
                priceText.text = string.IsNullOrEmpty(localizedPrice) ? "..." : localizedPrice;
            }

            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(() => IAPManager.Instance.BuyProduct(product.productId));
        }
    }
}
