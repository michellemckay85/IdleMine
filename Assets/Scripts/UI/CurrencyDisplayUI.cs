using GoldAndGoblins.Core;
using TMPro;
using UnityEngine;

namespace GoldAndGoblins.UI
{
    /// <summary>Binds the top-bar Gold/Gems labels to CurrencyManager's change events.</summary>
    public class CurrencyDisplayUI : MonoBehaviour
    {
        [SerializeField] private CurrencyManager currencyManager;
        [SerializeField] private TMP_Text goldText;
        [SerializeField] private TMP_Text gemsText;

        private void OnEnable()
        {
            currencyManager.OnGoldChanged += HandleGoldChanged;
            currencyManager.OnGemsChanged += HandleGemsChanged;

            HandleGoldChanged(currencyManager.Gold);
            HandleGemsChanged(currencyManager.Gems);
        }

        private void OnDisable()
        {
            currencyManager.OnGoldChanged -= HandleGoldChanged;
            currencyManager.OnGemsChanged -= HandleGemsChanged;
        }

        private void HandleGoldChanged(double gold)
        {
            if (goldText != null) goldText.text = NumberFormatter.Format(gold);
        }

        private void HandleGemsChanged(long gems)
        {
            if (gemsText != null) gemsText.text = NumberFormatter.Format(gems);
        }
    }
}
