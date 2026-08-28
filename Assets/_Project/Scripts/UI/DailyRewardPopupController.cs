using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GoldAndGoblins.LiveOps;

namespace GoldAndGoblins.UI
{
    public class DailyRewardPopupController : MonoBehaviour
    {
        [SerializeField] private GameObject popupRoot;
        [SerializeField] private TMP_Text streakText;
        [SerializeField] private Button claimButton;

        private void Awake()
        {
            if (claimButton != null) claimButton.onClick.AddListener(OnClaimClicked);
        }

        private void OnEnable()
        {
            var ready = DailyRewardManager.Instance != null && DailyRewardManager.Instance.HasRewardReadyToday;
            if (popupRoot != null) popupRoot.SetActive(ready);
            if (ready) RefreshStreakText();
        }

        private void RefreshStreakText()
        {
            if (streakText != null)
            {
                streakText.text = $"Day {DailyRewardManager.Instance.CurrentStreak + 1} login reward!";
            }
        }

        private void OnClaimClicked()
        {
            DailyRewardManager.Instance.ClaimDailyReward();
            if (popupRoot != null) popupRoot.SetActive(false);
        }
    }
}
