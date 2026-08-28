using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GoldAndGoblins.Core;
using GoldAndGoblins.Economy;

namespace GoldAndGoblins.Gameplay
{
    public class UpgradeSystem : GoldAndGoblins.Utils.Singleton<UpgradeSystem>
    {
        [SerializeField] private List<UpgradeDataSO> upgrades = new List<UpgradeDataSO>();

        private readonly Dictionary<string, int> levels = new Dictionary<string, int>();
        private readonly Dictionary<string, UpgradeDataSO> upgradesById = new Dictionary<string, UpgradeDataSO>();

        public void Initialize()
        {
            upgradesById.Clear();
            foreach (var upgrade in upgrades)
            {
                upgradesById[upgrade.upgradeId] = upgrade;
            }

            levels.Clear();
            foreach (var entry in SaveManager.Instance.Current.upgradeLevels)
            {
                levels[entry.upgradeId] = entry.level;
            }

            RecalculateDerivedStats();
        }

        public int GetLevel(string upgradeId) => levels.TryGetValue(upgradeId, out var level) ? level : 0;

        public double GetCurrentValue(UpgradeType type)
        {
            var upgrade = upgrades.FirstOrDefault(u => u.upgradeType == type);
            return upgrade == null ? 0 : upgrade.ValueForLevel(GetLevel(upgrade.upgradeId));
        }

        public double GetNextCost(string upgradeId)
        {
            if (!upgradesById.TryGetValue(upgradeId, out var upgrade)) return double.MaxValue;
            return upgrade.CostForLevel(GetLevel(upgradeId));
        }

        public bool CanPurchase(string upgradeId)
        {
            if (!upgradesById.TryGetValue(upgradeId, out var upgrade)) return false;
            var level = GetLevel(upgradeId);
            if (upgrade.maxLevel > 0 && level >= upgrade.maxLevel) return false;
            return CurrencyManager.Instance.Gold >= upgrade.CostForLevel(level);
        }

        public bool Purchase(string upgradeId)
        {
            if (!CanPurchase(upgradeId)) return false;

            var upgrade = upgradesById[upgradeId];
            var level = GetLevel(upgradeId);
            var cost = upgrade.CostForLevel(level);

            if (!CurrencyManager.Instance.TrySpendGold(cost)) return false;

            var newLevel = level + 1;
            levels[upgradeId] = newLevel;
            PersistLevel(upgradeId, newLevel);
            RecalculateDerivedStats();

            EventBus.Publish(new UpgradePurchasedEvent(upgradeId, newLevel));
            return true;
        }

        private void RecalculateDerivedStats()
        {
            CurrencyManager.Instance.UpgradeGoldMultiplier = System.Math.Max(1.0, GetCurrentValue(UpgradeType.GoldMultiplier));
        }

        private void PersistLevel(string upgradeId, int level)
        {
            var saveList = SaveManager.Instance.Current.upgradeLevels;
            var entry = saveList.FirstOrDefault(e => e.upgradeId == upgradeId);
            if (entry != null)
            {
                entry.level = level;
            }
            else
            {
                saveList.Add(new UpgradeLevelEntry { upgradeId = upgradeId, level = level });
            }
        }

        public IReadOnlyList<UpgradeDataSO> AllUpgrades => upgrades;
    }
}
