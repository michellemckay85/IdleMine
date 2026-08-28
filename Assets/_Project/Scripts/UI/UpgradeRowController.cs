using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GoldAndGoblins.Gameplay;

namespace GoldAndGoblins.UI
{
    // One instance per upgrade row prefab; UpgradePanelController spawns/binds these.
    public class UpgradeRowController : MonoBehaviour
    {
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text costText;
        [SerializeField] private Button buyButton;

        private UpgradeDataSO data;

        private void Awake() => UiRowLayoutFix.FixRow(transform);

        public void Bind(UpgradeDataSO upgradeData)
        {
            data = upgradeData;
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(OnBuyClicked);
            Refresh();
            UiRowLayoutFix.FixRow(transform);
        }

        public void Refresh()
        {
            if (data == null) return;
            var level = UpgradeSystem.Instance.GetLevel(data.upgradeId);
            var cost = UpgradeSystem.Instance.GetNextCost(data.upgradeId);

            if (nameText != null) nameText.text = data.displayName;
            if (levelText != null) levelText.text = $"Lv. {level}";
            if (costText != null) costText.text = cost.ToString("0");
            if (buyButton != null) buyButton.interactable = UpgradeSystem.Instance.CanPurchase(data.upgradeId);
        }

        private void OnBuyClicked()
        {
            if (UpgradeSystem.Instance.Purchase(data.upgradeId))
            {
                Refresh();
            }
        }
    }
}
