using UnityEngine;
using UnityEngine.UI;
using GoldAndGoblins.Core;

namespace GoldAndGoblins.UI
{
    public class GoblinHealthBarController : MonoBehaviour
    {
        [SerializeField] private GameObject barRoot;
        [SerializeField] private Image fillImage;

        private void OnEnable()
        {
            EventBus.Subscribe<GoblinEncounterStartedEvent>(OnEncounterStarted);
            EventBus.Subscribe<GoblinHealthChangedEvent>(OnHealthChanged);
            EventBus.Subscribe<GoblinDefeatedEvent>(OnDefeated);
            if (barRoot != null) barRoot.SetActive(false);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<GoblinEncounterStartedEvent>(OnEncounterStarted);
            EventBus.Unsubscribe<GoblinHealthChangedEvent>(OnHealthChanged);
            EventBus.Unsubscribe<GoblinDefeatedEvent>(OnDefeated);
        }

        private void OnEncounterStarted(GoblinEncounterStartedEvent evt)
        {
            if (barRoot != null) barRoot.SetActive(true);
            if (fillImage != null) fillImage.fillAmount = 1f;
        }

        private void OnHealthChanged(GoblinHealthChangedEvent evt)
        {
            if (fillImage != null && evt.MaxHealth > 0) fillImage.fillAmount = evt.CurrentHealth / evt.MaxHealth;
        }

        private void OnDefeated(GoblinDefeatedEvent evt)
        {
            if (barRoot != null) barRoot.SetActive(false);
        }
    }
}
