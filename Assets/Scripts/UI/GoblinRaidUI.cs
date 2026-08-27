using GoldAndGoblins.Core;
using GoldAndGoblins.Mining;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GoldAndGoblins.UI
{
    /// <summary>Shows/hides the raid warning panel and reports defend-tap progress on a fill bar.</summary>
    public class GoblinRaidUI : MonoBehaviour
    {
        [SerializeField] private GoblinRaidManager goblinRaidManager;
        [SerializeField] private GameObject raidPanel;
        [SerializeField] private Image defendProgressFill;
        [SerializeField] private Button defendButton;
        [SerializeField] private TMP_Text resultText;
        [SerializeField] private float resultTextDisplaySeconds = 2f;

        private float _resultTextTimer;

        private void Awake()
        {
            if (defendButton != null) defendButton.onClick.AddListener(() => goblinRaidManager.RegisterDefendTap());
        }

        private void OnEnable()
        {
            goblinRaidManager.OnRaidStarted += HandleRaidStarted;
            goblinRaidManager.OnDefendProgress += HandleDefendProgress;
            goblinRaidManager.OnRaidDefended += HandleRaidDefended;
            goblinRaidManager.OnRaidFailed += HandleRaidFailed;

            SetRaidPanelVisible(false);
            if (resultText != null) resultText.gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            goblinRaidManager.OnRaidStarted -= HandleRaidStarted;
            goblinRaidManager.OnDefendProgress -= HandleDefendProgress;
            goblinRaidManager.OnRaidDefended -= HandleRaidDefended;
            goblinRaidManager.OnRaidFailed -= HandleRaidFailed;
        }

        private void Update()
        {
            if (_resultTextTimer <= 0f || resultText == null) return;
            _resultTextTimer -= Time.deltaTime;
            if (_resultTextTimer <= 0f) resultText.gameObject.SetActive(false);
        }

        private void HandleRaidStarted()
        {
            SetRaidPanelVisible(true);
            if (defendProgressFill != null) defendProgressFill.fillAmount = 0f;
        }

        private void HandleDefendProgress(int current, int required)
        {
            if (defendProgressFill != null) defendProgressFill.fillAmount = (float)current / required;
        }

        private void HandleRaidDefended(double bonusGold)
        {
            SetRaidPanelVisible(false);
            ShowResult($"Raid repelled! +{NumberFormatter.Format(bonusGold)} gold");
        }

        private void HandleRaidFailed(double stolenGold)
        {
            SetRaidPanelVisible(false);
            ShowResult($"Goblins stole {NumberFormatter.Format(stolenGold)} gold!");
        }

        private void ShowResult(string message)
        {
            if (resultText == null) return;
            resultText.text = message;
            resultText.gameObject.SetActive(true);
            _resultTextTimer = resultTextDisplaySeconds;
        }

        private void SetRaidPanelVisible(bool visible)
        {
            if (raidPanel != null) raidPanel.SetActive(visible);
        }
    }
}
