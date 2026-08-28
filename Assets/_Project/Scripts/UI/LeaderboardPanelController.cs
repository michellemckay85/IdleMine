using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GoldAndGoblins.Social;

namespace GoldAndGoblins.UI
{
    public class LeaderboardPanelController : MonoBehaviour
    {
        [SerializeField] private LeaderboardRowController rowPrefab;
        [SerializeField] private Transform rowContainer;
        [SerializeField] private TMP_Text statusText;

        private readonly List<LeaderboardRowController> spawnedRows = new List<LeaderboardRowController>();

        private async void OnEnable() => await RefreshAsync();

        public async System.Threading.Tasks.Task RefreshAsync()
        {
            if (LeaderboardManager.Instance == null || rowPrefab == null || rowContainer == null) return;

            SetStatus("Loading...");
            foreach (var row in spawnedRows) Destroy(row.gameObject);
            spawnedRows.Clear();

            await LeaderboardManager.Instance.SubmitScoreAsync();
            var rows = await LeaderboardManager.Instance.GetTopScoresAsync();

            if (rows.Count == 0)
            {
                SetStatus(LeaderboardManager.Instance.IsReady
                    ? "No scores yet -- be the first!"
                    : "Leaderboard unavailable (offline, or Unity Gaming Services isn't set up yet).");
                return;
            }

            SetStatus(null);
            foreach (var row in rows)
            {
                var rowGo = Instantiate(rowPrefab, rowContainer);
                rowGo.Bind(row);
                spawnedRows.Add(rowGo);
            }

            if (rowContainer is RectTransform rt) LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
        }

        private void SetStatus(string message)
        {
            if (statusText == null) return;
            statusText.gameObject.SetActive(!string.IsNullOrEmpty(message));
            statusText.text = message;
        }
    }
}
