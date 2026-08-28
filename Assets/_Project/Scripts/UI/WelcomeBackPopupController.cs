using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GoldAndGoblins.Core;
using GoldAndGoblins.Gameplay;
using GoldAndGoblins.Ads;

namespace GoldAndGoblins.UI
{
    public class WelcomeBackPopupController : MonoBehaviour
    {
        [SerializeField] private GameObject popupRoot;
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private Button watchAdToDoubleButton;
        [SerializeField] private Button closeButton;

        private void Awake()
        {
            if (popupRoot != null) popupRoot.SetActive(false);
            if (watchAdToDoubleButton != null) watchAdToDoubleButton.onClick.AddListener(OnWatchAdClicked);
            if (closeButton != null) closeButton.onClick.AddListener(Close);
        }

        private void OnEnable() => EventBus.Subscribe<WelcomeBackEvent>(OnWelcomeBack);
        private void OnDisable() => EventBus.Unsubscribe<WelcomeBackEvent>(OnWelcomeBack);

        private void OnWelcomeBack(WelcomeBackEvent evt)
        {
            if (popupRoot != null) popupRoot.SetActive(true);
            var hours = evt.OfflineSeconds / 3600.0;
            if (messageText != null)
            {
                messageText.text = $"Welcome back! You earned {evt.GoldEarned:0} gold while away ({hours:0.#}h).";
            }
        }

        private void OnWatchAdClicked()
        {
            AdsManager.Instance.ShowRewardedAd("double_offline_earnings", () =>
            {
                IdleEarningsManager.Instance.ClaimExtendedOfflineBonus();
                Close();
            });
        }

        private void Close()
        {
            if (popupRoot != null) popupRoot.SetActive(false);
        }
    }
}
