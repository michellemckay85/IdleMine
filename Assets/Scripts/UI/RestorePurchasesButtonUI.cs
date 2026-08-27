using GoldAndGoblins.IAP;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GoldAndGoblins.UI
{
    /// <summary>
    /// "Restore Purchases" button required by Apple App Store guidelines for apps selling
    /// non-consumables/subscriptions. Place in the shop or settings panel.
    /// </summary>
    public class RestorePurchasesButtonUI : MonoBehaviour
    {
        [SerializeField] private IAPManager iapManager;
        [SerializeField] private Button restoreButton;
        [SerializeField] private TMP_Text statusText;

        private void Awake()
        {
            if (restoreButton != null) restoreButton.onClick.AddListener(HandleRestoreClicked);
        }

        private void HandleRestoreClicked()
        {
            SetStatus("Restoring...");
            iapManager.RestorePurchases(success =>
            {
                SetStatus(success ? "Purchases restored." : "Nothing to restore.");
            });
        }

        private void SetStatus(string message)
        {
            if (statusText != null) statusText.text = message;
        }
    }
}
