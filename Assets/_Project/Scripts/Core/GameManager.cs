using UnityEngine;
using GoldAndGoblins.Gameplay;
using GoldAndGoblins.LiveOps;
using GoldAndGoblins.Analytics;

namespace GoldAndGoblins.Core
{
    // Boots managers in dependency order. Attach this to a single "Managers" root
    // in the bootstrap scene alongside SaveManager, CurrencyManager, etc.
    [DefaultExecutionOrder(1000)]
    public class GameManager : GoldAndGoblins.Utils.Singleton<GameManager>
    {
        [SerializeField] private MineGrid mineGrid;
        [SerializeField] private UpgradeSystem upgradeSystem;
        [SerializeField] private IdleEarningsManager idleEarningsManager;
        [SerializeField] private EventManager eventManager;
        [SerializeField] private DailyRewardManager dailyRewardManager;

        protected override void Awake()
        {
            base.Awake();

            // SaveManager, CurrencyManager, IAPManager, AdsManager and AnalyticsManager
            // are independent singletons and initialize themselves in their own Awake.
            PrestigeManager.Instance?.ApplyMultiplierToEconomy();
            upgradeSystem?.Initialize();
            eventManager?.RefreshActiveEvents();
            idleEarningsManager?.ApplyOfflineEarnings();
            mineGrid?.Initialize();
            dailyRewardManager?.CheckAndOfferDailyReward();

            AnalyticsManager.Instance?.LogEvent("session_start");
        }

        private void OnApplicationQuit()
        {
            SaveManager.Instance?.Save();
        }
    }
}
