using UnityEngine;
using GoldAndGoblins.Gameplay;
using GoldAndGoblins.LiveOps;
using GoldAndGoblins.Analytics;
using GoldAndGoblins.Economy;

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
        [SerializeField] private PrestigeManager prestigeManager;

        protected override void Awake()
        {
            base.Awake();

            // SaveManager, CurrencyManager, IAPManager, AdsManager and AnalyticsManager
            // are independent singletons and initialize themselves in their own Awake.
            upgradeSystem?.Initialize();

            // Prestige + VIP multipliers must be restored from save before any gold is granted
            // (including offline earnings below). Serialized field is preferred; Instance is a
            // safe fallback so an older scene that never assigned prestigeManager still works.
            if (prestigeManager == null) prestigeManager = PrestigeManager.Instance;
            var prestigeMult = prestigeManager != null ? prestigeManager.CurrentPrestigeMultiplier : 1.0;
            var vipActive = SaveManager.Instance != null && SaveManager.Instance.Current.vipActive;
            CurrencyManager.Instance?.RefreshPersistentMultipliers(prestigeMult, vipActive);

            mineGrid?.Initialize();
            idleEarningsManager?.ApplyOfflineEarnings();
            eventManager?.RefreshActiveEvents();
            dailyRewardManager?.CheckAndOfferDailyReward();

            AnalyticsManager.Instance?.LogEvent("session_start");
        }

        private void OnApplicationQuit()
        {
            SaveManager.Instance?.Save();
        }
    }
}
