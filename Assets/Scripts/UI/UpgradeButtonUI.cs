using GoldAndGoblins.Core;
using GoldAndGoblins.Mining;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GoldAndGoblins.UI
{
    /// <summary>One upgrade row: name, level, cost, buy button. Set upgradeId in the Inspector to match an UpgradeCatalog entry.</summary>
    public class UpgradeButtonUI : MonoBehaviour
    {
        [SerializeField] private string upgradeId;
        [SerializeField] private UpgradeSystem upgradeSystem;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text costText;
        [SerializeField] private Button buyButton;

        private void Awake()
        {
            if (UpgradeCatalog.TryGet(upgradeId, out UpgradeDefinition def) && nameText != null)
            {
                nameText.text = def.displayName;
            }

            if (buyButton != null) buyButton.onClick.AddListener(HandleBuyClicked);
        }

        private void OnEnable()
        {
            upgradeSystem.OnUpgradeLevelChanged += HandleUpgradeLevelChanged;
            Refresh();
        }

        private void OnDisable()
        {
            upgradeSystem.OnUpgradeLevelChanged -= HandleUpgradeLevelChanged;
        }

        private void Update()
        {
            // Cost display doesn't change without a purchase, but affordability (button
            // interactable state) depends on live gold, so poll it cheaply each frame.
            if (buyButton != null) buyButton.interactable = upgradeSystem.CanPurchase(upgradeId);
        }

        private void HandleBuyClicked()
        {
            if (upgradeSystem.TryPurchase(upgradeId)) Refresh();
        }

        private void HandleUpgradeLevelChanged(string changedId, int newLevel)
        {
            if (changedId == upgradeId) Refresh();
        }

        private void Refresh()
        {
            int level = upgradeSystem.GetLevel(upgradeId);
            double nextCost = upgradeSystem.GetNextCost(upgradeId);

            if (levelText != null) levelText.text = $"Lv. {level}";
            if (costText != null) costText.text = NumberFormatter.Format(nextCost);
        }
    }
}
