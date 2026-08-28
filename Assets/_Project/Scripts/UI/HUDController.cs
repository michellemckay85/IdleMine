using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GoldAndGoblins.Core;
using GoldAndGoblins.Economy;
using GoldAndGoblins.Gameplay;

namespace GoldAndGoblins.UI
{
    public class HUDController : MonoBehaviour
    {
        [SerializeField] private TMP_Text goldText;
        [SerializeField] private TMP_Text gemsText;
        [SerializeField] private TMP_Text depthText;
        [SerializeField] private Button prestigeButton;
        [SerializeField] private PrestigePanelController prestigePanel;

        private void OnEnable()
        {
            EventBus.Subscribe<GoldChangedEvent>(OnGoldChanged);
            EventBus.Subscribe<GemsChangedEvent>(OnGemsChanged);
            EventBus.Subscribe<DepthAdvancedEvent>(OnDepthAdvanced);
            EventBus.Subscribe<DepthCappedEvent>(OnDepthCapped);
            EventBus.Subscribe<PrestigeEvent>(OnPrestige);
            EventBus.Subscribe<UpgradePurchasedEvent>(OnUpgradePurchased);
        }

        // Start (not OnEnable) because it's the only lifecycle method Unity
        // guarantees runs after every other object's Awake -- the manager
        // singletons this reads are only guaranteed to exist by then.
        private void Start()
        {
            if (prestigeButton != null) prestigeButton.onClick.AddListener(OpenPrestige);
            RefreshAll();
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<GoldChangedEvent>(OnGoldChanged);
            EventBus.Unsubscribe<GemsChangedEvent>(OnGemsChanged);
            EventBus.Unsubscribe<DepthAdvancedEvent>(OnDepthAdvanced);
            EventBus.Unsubscribe<DepthCappedEvent>(OnDepthCapped);
            EventBus.Unsubscribe<PrestigeEvent>(OnPrestige);
            EventBus.Unsubscribe<UpgradePurchasedEvent>(OnUpgradePurchased);
        }

        public void OpenPrestige()
        {
            var nav = FindObjectOfType<NavBarController>();
            if (nav != null) nav.ShowPrestige();
            else if (prestigePanel != null) prestigePanel.Open();
        }

        private void RefreshAll()
        {
            if (SaveManager.Instance == null || CurrencyManager.Instance == null || MineGrid.Instance == null) return;
            SetGoldText(CurrencyManager.Instance.Gold);
            SetGemsText(CurrencyManager.Instance.Gems);
            RefreshDepthText();
        }

        private void OnGoldChanged(GoldChangedEvent evt) => SetGoldText(evt.NewTotal);
        private void OnGemsChanged(GemsChangedEvent evt) => SetGemsText(evt.NewTotal);
        private void OnDepthAdvanced(DepthAdvancedEvent evt) => RefreshDepthText();
        private void OnDepthCapped(DepthCappedEvent evt) => RefreshDepthText();
        private void OnPrestige(PrestigeEvent evt)
        {
            RefreshDepthText();
            SetGoldText(CurrencyManager.Instance != null ? CurrencyManager.Instance.Gold : 0);
        }
        private void OnUpgradePurchased(UpgradePurchasedEvent evt) => RefreshDepthText();

        private void SetGoldText(double value) { if (goldText != null) goldText.text = FormatNumber(value); }
        private void SetGemsText(long value) { if (gemsText != null) gemsText.text = value.ToString(); }

        private void RefreshDepthText()
        {
            if (depthText == null || MineGrid.Instance == null) return;
            var depth = MineGrid.Instance.CurrentDepth;
            var max = MineGrid.Instance.MaxUnlockedDepth;
            var multiplier = PrestigeManager.Instance != null
                ? PrestigeManager.Instance.CurrentPrestigeMultiplier
                : 1.0;
            var depthPart = max == int.MaxValue ? $"Depth {depth}" : $"Depth {depth}/{max}";
            depthText.text = $"{depthPart}  •  {multiplier:0.##}x";
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
