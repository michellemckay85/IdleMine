using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GoldAndGoblins.Core;
using GoldAndGoblins.LiveOps;

namespace GoldAndGoblins.UI
{
    public class EventBannerController : MonoBehaviour
    {
        [SerializeField] private GameObject bannerRoot;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private Image bannerImage;
        [SerializeField] private EventManager eventManager;

        private void OnEnable()
        {
            EventBus.Subscribe<LiveEventStartedEvent>(OnEventStarted);
            EventBus.Subscribe<LiveEventEndedEvent>(OnEventEnded);
            RefreshFromActiveEvents();
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<LiveEventStartedEvent>(OnEventStarted);
            EventBus.Unsubscribe<LiveEventEndedEvent>(OnEventEnded);
        }

        private void OnEventStarted(LiveEventStartedEvent evt) => ShowEvent(evt.EventId);
        private void OnEventEnded(LiveEventEndedEvent evt) => RefreshFromActiveEvents();

        private void RefreshFromActiveEvents()
        {
            if (eventManager == null || eventManager.ActiveEventIds.Count == 0)
            {
                if (bannerRoot != null) bannerRoot.SetActive(false);
                return;
            }

            foreach (var id in eventManager.ActiveEventIds)
            {
                ShowEvent(id);
                break;
            }
        }

        private void ShowEvent(string eventId)
        {
            var data = eventManager != null ? eventManager.GetEventData(eventId) : null;
            if (data == null) return;

            if (bannerRoot != null) bannerRoot.SetActive(true);
            if (titleText != null) titleText.text = data.displayName;
            if (bannerImage != null && data.bannerArt != null) bannerImage.sprite = data.bannerArt;
        }
    }
}
