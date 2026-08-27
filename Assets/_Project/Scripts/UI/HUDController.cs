using UnityEngine;
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

        private void OnEnable()
        {
            EventBus.Subscribe<GoldChangedEvent>(OnGoldChanged);
            EventBus.Subscribe<GemsChangedEvent>(OnGemsChanged);
            EventBus.Subscribe<DepthAdvancedEvent>(OnDepthAdvanced);
            RefreshAll();
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<GoldChangedEvent>(OnGoldChanged);
            EventBus.Unsubscribe<GemsChangedEvent>(OnGemsChanged);
            EventBus.Unsubscribe<DepthAdvancedEvent>(OnDepthAdvanced);
        }

        private void RefreshAll()
        {
            if (SaveManager.Instance == null) return;
            SetGoldText(CurrencyManager.Instance.Gold);
            SetGemsText(CurrencyManager.Instance.Gems);
            SetDepthText(MineGrid.Instance.CurrentDepth);
        }

        private void OnGoldChanged(GoldChangedEvent evt) => SetGoldText(evt.NewTotal);
        private void OnGemsChanged(GemsChangedEvent evt) => SetGemsText(evt.NewTotal);
        private void OnDepthAdvanced(DepthAdvancedEvent evt) => SetDepthText(evt.NewDepth);

        private void SetGoldText(double value) { if (goldText != null) goldText.text = FormatNumber(value); }
        private void SetGemsText(long value) { if (gemsText != null) gemsText.text = value.ToString(); }
        private void SetDepthText(int depth) { if (depthText != null) depthText.text = $"Depth {depth}"; }

        private static string FormatNumber(double value)
        {
            if (value >= 1_000_000_000) return $"{value / 1_000_000_000:0.##}B";
            if (value >= 1_000_000) return $"{value / 1_000_000:0.##}M";
            if (value >= 1_000) return $"{value / 1_000:0.##}K";
            return value.ToString("0");
        }
    }
}
