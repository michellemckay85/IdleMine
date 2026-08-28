using UnityEngine;

namespace GoldAndGoblins.UI
{
    // Bottom nav: Upgrades / Shop buttons each open their panel and close the other.
    public class NavBarController : MonoBehaviour
    {
        [SerializeField] private GameObject upgradePanel;
        [SerializeField] private GameObject shopPanel;

        public void ShowUpgrades()
        {
            if (upgradePanel != null) upgradePanel.SetActive(true);
            if (shopPanel != null) shopPanel.SetActive(false);
        }

        public void ShowShop()
        {
            if (shopPanel != null) shopPanel.SetActive(true);
            if (upgradePanel != null) upgradePanel.SetActive(false);
        }

        public void CloseAll()
        {
            if (upgradePanel != null) upgradePanel.SetActive(false);
            if (shopPanel != null) shopPanel.SetActive(false);
        }
    }
}
