using System;
using UnityEngine;
using GoldAndGoblins.Core;

namespace GoldAndGoblins.Gameplay
{
    // "Reset" mechanic: trade current run's lifetime gold for a permanent multiplier,
    // then start back at depth 1 with base upgrade levels. Matches the prestige loop
    // used by most idle-miner games (this one included) to keep long-term progression alive.
    public class PrestigeManager : GoldAndGoblins.Utils.Singleton<PrestigeManager>
    {
        [SerializeField] private double lifetimeGoldPerPrestigePoint = 1_000_000;
        [SerializeField] private double multiplierPerPrestigePoint = 0.05;

        public int PrestigeLevel => SaveManager.Instance != null && SaveManager.Instance.Current != null
            ? SaveManager.Instance.Current.prestigeLevel
            : 0;

        public double MultiplierPerPoint => multiplierPerPrestigePoint;

        public double CurrentPrestigeMultiplier => 1.0 + PrestigeLevel * multiplierPerPrestigePoint;

        public double LifetimeGoldPerPrestigePoint => lifetimeGoldPerPrestigePoint;

        public void ApplyMultiplierToEconomy()
        {
            if (Economy.CurrencyManager.Instance != null)
            {
                Economy.CurrencyManager.Instance.PrestigeGoldMultiplier = CurrentPrestigeMultiplier;
            }
        }

        public int PotentialPrestigeGainFromCurrentRun()
        {
            if (SaveManager.Instance == null || SaveManager.Instance.Current == null) return 0;
            var lifetime = SaveManager.Instance.Current.lifetimeGoldEarned;
            return (int)Math.Floor(lifetime / lifetimeGoldPerPrestigePoint);
        }

        public bool CanPrestige() => PotentialPrestigeGainFromCurrentRun() > 0;

        public double LifetimeGoldUntilNextPrestigePoint()
        {
            if (SaveManager.Instance == null || SaveManager.Instance.Current == null) return lifetimeGoldPerPrestigePoint;
            var lifetime = SaveManager.Instance.Current.lifetimeGoldEarned;
            var remainder = lifetime % lifetimeGoldPerPrestigePoint;
            return lifetimeGoldPerPrestigePoint - remainder;
        }

        public void DoPrestige(MineGrid mineGrid, UpgradeSystem upgradeSystem)
        {
            var gain = PotentialPrestigeGainFromCurrentRun();
            if (gain <= 0) return;

            var data = SaveManager.Instance.Current;
            var goldBefore = data.gold;
            data.prestigeLevel += gain;
            data.gold = 0;
            data.lifetimeGoldEarned = 0;
            data.currentDepth = 1;
            data.upgradeLevels.Clear();

            ApplyMultiplierToEconomy();
            upgradeSystem?.Initialize();
            mineGrid?.RebuildGrid();
            SaveManager.Instance.Save();

            EventBus.Publish(new GoldChangedEvent(0, -goldBefore));
            EventBus.Publish(new PrestigeEvent(data.prestigeLevel, CurrentPrestigeMultiplier));
        }
    }
}
