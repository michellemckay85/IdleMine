using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GoldAndGoblins.Core;
using GoldAndGoblins.Gameplay;

namespace GoldAndGoblins.UI
{
    public class PrestigePanelController : MonoBehaviour
    {
        [SerializeField] private GameObject popupRoot;
        [SerializeField] private TMP_Text summaryText;
        [SerializeField] private Button prestigeButton;
        [SerializeField] private Button closeButton;

        private void Awake()
        {
            if (popupRoot != null) popupRoot.SetActive(false);
            if (prestigeButton != null) prestigeButton.onClick.AddListener(OnPrestigeClicked);
            if (closeButton != null) closeButton.onClick.AddListener(Close);
        }

        private void OnEnable()
        {
            EventBus.Subscribe<GoldChangedEvent>(OnGoldChanged);
            EventBus.Subscribe<PrestigeEvent>(OnPrestiged);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<GoldChangedEvent>(OnGoldChanged);
            EventBus.Unsubscribe<PrestigeEvent>(OnPrestiged);
        }

        public void Open()
        {
            if (popupRoot != null) popupRoot.SetActive(true);
            Refresh();
        }

        public void Close()
        {
            if (popupRoot != null) popupRoot.SetActive(false);
        }

        private void OnGoldChanged(GoldChangedEvent evt) => Refresh();
        private void OnPrestiged(PrestigeEvent evt) => Refresh();

        private void Refresh()
        {
            if (PrestigeManager.Instance == null || SaveManager.Instance == null) return;

            var prestige = PrestigeManager.Instance;
            var gain = prestige.PotentialPrestigeGainFromCurrentRun();
            var lifetime = SaveManager.Instance.Current.lifetimeGoldEarned;
            var currentMult = prestige.CurrentPrestigeMultiplier;
            var nextMult = 1.0 + (prestige.PrestigeLevel + gain) * prestige.MultiplierPerPoint;
            if (summaryText != null)
            {
                if (gain > 0)
                {
                    summaryText.text =
                        $"Prestige {prestige.PrestigeLevel}  •  {currentMult:0.##}x gold\n" +
                        $"This run: {FormatNumber(lifetime)} lifetime gold\n\n" +
                        $"Prestige now for +{gain} level(s) → {nextMult:0.##}x gold.\n" +
                        "Resets depth, gold, and upgrades. Keeps gems and prestige.";
                }
                else
                {
                    var remaining = prestige.LifetimeGoldUntilNextPrestigePoint();
                    summaryText.text =
                        $"Prestige {prestige.PrestigeLevel}  •  {currentMult:0.##}x gold\n" +
                        $"This run: {FormatNumber(lifetime)} lifetime gold\n\n" +
                        $"Earn {FormatNumber(remaining)} more this run to prestige.\n" +
                        "Prestige resets depth, gold, and upgrades for a permanent gold multiplier.";
                }
            }

            if (prestigeButton != null) prestigeButton.interactable = prestige.CanPrestige();
        }

        private void OnPrestigeClicked()
        {
            if (PrestigeManager.Instance == null) return;
            PrestigeManager.Instance.DoPrestige(MineGrid.Instance, UpgradeSystem.Instance);
            Close();
        }

        private static string FormatNumber(double value)
        {
            if (value >= 1_000_000_000) return $"{value / 1_000_000_000:0.##}B";
            if (value >= 1_000_000) return $"{value / 1_000_000:0.##}M";
            if (value >= 1_000) return $"{value / 1_000:0.##}K";
            return value.ToString("0");
        }
    }
}
