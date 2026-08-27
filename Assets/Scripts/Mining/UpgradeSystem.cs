using System;
using System.Collections.Generic;
using GoldAndGoblins.Core;
using UnityEngine;

namespace GoldAndGoblins.Mining
{
    /// <summary>Tracks purchased levels for every UpgradeDefinition and spends Gold via CurrencyManager.</summary>
    public class UpgradeSystem : MonoBehaviour
    {
        [SerializeField] private CurrencyManager currencyManager;

        public event Action<string, int> OnUpgradeLevelChanged;

        private readonly Dictionary<string, int> _levels = new Dictionary<string, int>();

        public void InitializeFromSave(GameSaveData data)
        {
            _levels.Clear();
            foreach (UpgradeLevelEntry entry in data.upgradeLevels)
            {
                _levels[entry.upgradeId] = entry.level;
            }
        }

        public void ApplyToSave(GameSaveData data)
        {
            data.upgradeLevels.Clear();
            foreach (KeyValuePair<string, int> kv in _levels)
            {
                data.upgradeLevels.Add(new UpgradeLevelEntry { upgradeId = kv.Key, level = kv.Value });
            }
        }

        public int GetLevel(string upgradeId)
        {
            return _levels.TryGetValue(upgradeId, out int level) ? level : 0;
        }

        public double GetNextCost(string upgradeId)
        {
            if (!UpgradeCatalog.TryGet(upgradeId, out UpgradeDefinition def)) return double.PositiveInfinity;
            return def.CostForLevel(GetLevel(upgradeId));
        }

        public bool CanPurchase(string upgradeId)
        {
            if (!UpgradeCatalog.TryGet(upgradeId, out UpgradeDefinition def)) return false;
            int level = GetLevel(upgradeId);
            if (def.maxLevel > 0 && level >= def.maxLevel) return false;
            return currencyManager.Gold >= def.CostForLevel(level);
        }

        public bool TryPurchase(string upgradeId)
        {
            if (!CanPurchase(upgradeId)) return false;
            if (!UpgradeCatalog.TryGet(upgradeId, out UpgradeDefinition def)) return false;

            double cost = def.CostForLevel(GetLevel(upgradeId));
            if (!currencyManager.TrySpendGold(cost)) return false;

            int newLevel = GetLevel(upgradeId) + 1;
            _levels[upgradeId] = newLevel;
            OnUpgradeLevelChanged?.Invoke(upgradeId, newLevel);
            return true;
        }

        /// <summary>Sum of gold/second contributed by every purchased upgrade level.</summary>
        public double TotalGoldPerSecond()
        {
            double total = 0;
            foreach (UpgradeDefinition def in UpgradeCatalog.All)
            {
                total += def.goldPerSecondPerLevel * GetLevel(def.id);
            }
            return total;
        }
    }
}
