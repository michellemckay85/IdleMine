using UnityEngine;
using TMPro;
using GoldAndGoblins.Social;

namespace GoldAndGoblins.UI
{
    public class LeaderboardRowController : MonoBehaviour
    {
        [SerializeField] private TMP_Text rankText;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text scoreText;

        private static readonly Color LocalPlayerColor = new Color(0.85f, 0.55f, 0.05f);
        private static readonly Color DefaultColor = new Color(0.25f, 0.15f, 0.05f);

        public void Bind(LeaderboardRow row)
        {
            var color = row.IsLocalPlayer ? LocalPlayerColor : DefaultColor;

            if (rankText != null) { rankText.text = $"#{row.Rank}"; rankText.color = color; }
            if (nameText != null) { nameText.text = row.PlayerName; nameText.color = color; }
            if (scoreText != null) { scoreText.text = FormatScore(row.Score); scoreText.color = color; }
        }

        private static string FormatScore(long value)
        {
            if (value >= 1_000_000_000) return $"{value / 1_000_000_000f:0.##}B";
            if (value >= 1_000_000) return $"{value / 1_000_000f:0.##}M";
            if (value >= 1_000) return $"{value / 1_000f:0.##}K";
            return value.ToString();
        }
    }
}
