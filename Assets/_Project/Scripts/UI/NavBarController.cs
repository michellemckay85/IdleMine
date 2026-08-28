using UnityEngine;

namespace GoldAndGoblins.UI
{
    // Bottom nav: Upgrades / Shop / Leaderboard buttons, each opening its panel and
    // closing the other two. Prestige is a HUD button but still gets closed here so
    // only one overlay is visible at a time.
    public class NavBarController : MonoBehaviour
    {
        [SerializeField] private GameObject upgradePanel;
        [SerializeField] private GameObject shopPanel;
        [SerializeField] private GameObject leaderboardPanel;
        [SerializeField] private PrestigePanelController prestigePanel;

        public void ShowUpgrades() => ShowOnly(upgradePanel);
        public void ShowShop() => ShowOnly(shopPanel);
        public void ShowLeaderboard() => ShowOnly(leaderboardPanel);
        public void ShowPrestige()
        {
            ShowOnly(null);
            prestigePanel?.Open();
        }

        public void CloseAll()
        {
            if (upgradePanel != null) upgradePanel.SetActive(false);
            if (shopPanel != null) shopPanel.SetActive(false);
            if (leaderboardPanel != null) leaderboardPanel.SetActive(false);
            prestigePanel?.Close();
        }

        private void ShowOnly(GameObject panelToShow)
        {
            if (upgradePanel != null) upgradePanel.SetActive(upgradePanel == panelToShow);
            if (shopPanel != null) shopPanel.SetActive(shopPanel == panelToShow);
            if (leaderboardPanel != null) leaderboardPanel.SetActive(leaderboardPanel == panelToShow);
            prestigePanel?.Close();
        }
    }
}
