using UnityEngine;

namespace GoldAndGoblins.Gameplay
{
    public enum UpgradeType
    {
        DrillPower,
        DrillSpeed,
        GoldMultiplier,
        CriticalChance,
        AutoMinerSpeed,
        MaxDepthUnlock
    }

    [CreateAssetMenu(fileName = "Upgrade_", menuName = "Gold And Goblins/Upgrade")]
    public class UpgradeDataSO : ScriptableObject
    {
        public string upgradeId;
        public string displayName;
        [TextArea] public string description;
        public UpgradeType upgradeType;

        public double baseCost = 10;
        public double costGrowthRate = 1.15;

        public double baseValue = 1;
        public double valuePerLevel = 0.5;

        public int maxLevel = 0; // 0 = uncapped

        public double CostForLevel(int currentLevel) => baseCost * System.Math.Pow(costGrowthRate, currentLevel);
        public double ValueForLevel(int level) => baseValue + valuePerLevel * level;
    }
}
