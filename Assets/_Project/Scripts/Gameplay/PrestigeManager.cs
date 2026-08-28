using System;
using UnityEngine;
using GoldAndGoblins.Core;
using GoldAndGoblins.Economy;

namespace GoldAndGoblins.Gameplay
{
    // "Reset" mechanic: trade current run's lifetime gold for a permanent multiplier,
    // then start back at depth 1 with base upgrade levels. Matches the prestige loop
    // used by most idle-miner games (this one included) to keep long-term progression alive.
    public class PrestigeManager : GoldAndGoblins.Utils.Singleton<PrestigeManager>
    {
        [SerializeField] private double lifetimeGoldPerPrestigePoint = 1_000_000;
        [SerializeField] private double multiplierPerPrestigePoint = 0.05;

        public int PrestigeLevel => SaveManager.Instance.Current.prestigeLevel;

        public double CurrentPrestigeMultiplier => 1.0 + PrestigeLevel * multiplierPerPrestigePoint;

        public int PotentialPrestigeGainFromCurrentRun()
        {
            var lifetime = SaveManager.Instance.Current.lifetimeGoldEarned;
            return (int)Math.Floor(lifetime / lifetimeGoldPerPrestigePoint);
        }

        public bool CanPrestige() => PotentialPrestigeGainFromCurrentRun() > 0;

        public void DoPrestige(MineGrid mineGrid, UpgradeSystem upgradeSystem)
        {
            var gain = PotentialPrestigeGainFromCurrentRun();
            if (gain <= 0) return;

            var data = SaveManager.Instance.Current;
            data.prestigeLevel += gain;
            data.gold = 0;
            data.lifetimeGoldEarned = 0;
            data.currentDepth = 1;
            data.upgradeLevels.Clear();

            upgradeSystem.Initialize();
            mineGrid.Initialize();

            CurrencyManager.Instance?.RefreshPersistentMultipliers(CurrentPrestigeMultiplier, data.vipActive);

            EventBus.Publish(new PrestigeEvent(data.prestigeLevel, CurrentPrestigeMultiplier));
        }
    }
}
