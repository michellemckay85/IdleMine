using System;

namespace GoldAndGoblins.Mining
{
    /// <summary>
    /// Static definition of a purchasable mine upgrade. Plain data (not a ScriptableObject) so the
    /// whole upgrade tree lives in source control as readable code rather than binary/YAML assets.
    /// </summary>
    [Serializable]
    public struct UpgradeDefinition
    {
        public string id;
        public string displayName;
        public string description;
        public double baseCost;
        public double costMultiplierPerLevel;
        public double goldPerSecondPerLevel;
        public int maxLevel; // 0 = unlimited

        public UpgradeDefinition(string id, string displayName, string description, double baseCost,
            double costMultiplierPerLevel, double goldPerSecondPerLevel, int maxLevel = 0)
        {
            this.id = id;
            this.displayName = displayName;
            this.description = description;
            this.baseCost = baseCost;
            this.costMultiplierPerLevel = costMultiplierPerLevel;
            this.goldPerSecondPerLevel = goldPerSecondPerLevel;
            this.maxLevel = maxLevel;
        }

        public double CostForLevel(int currentLevel)
        {
            return baseCost * Math.Pow(costMultiplierPerLevel, currentLevel);
        }
    }

    /// <summary>The full upgrade tree for the mine. Add new upgrades here.</summary>
    public static class UpgradeCatalog
    {
        public static readonly UpgradeDefinition[] All =
        {
            new UpgradeDefinition(
                id: "pickaxe",
                displayName: "Sharper Pickaxe",
                description: "A better edge chips more gold per swing.",
                baseCost: 10,
                costMultiplierPerLevel: 1.15,
                goldPerSecondPerLevel: 1),
            new UpgradeDefinition(
                id: "miner_hire",
                displayName: "Hire a Miner",
                description: "Another pair of hands digging around the clock.",
                baseCost: 150,
                costMultiplierPerLevel: 1.16,
                goldPerSecondPerLevel: 6),
            new UpgradeDefinition(
                id: "mine_cart",
                displayName: "Reinforced Mine Cart",
                description: "Hauls bigger loads out of the shaft.",
                baseCost: 1200,
                costMultiplierPerLevel: 1.17,
                goldPerSecondPerLevel: 30),
            new UpgradeDefinition(
                id: "depth_charge",
                displayName: "Mine Depth Charge",
                description: "Blasts deeper into richer veins of ore.",
                baseCost: 15000,
                costMultiplierPerLevel: 1.18,
                goldPerSecondPerLevel: 180),
        };

        public static bool TryGet(string id, out UpgradeDefinition definition)
        {
            foreach (UpgradeDefinition def in All)
            {
                if (def.id == id)
                {
                    definition = def;
                    return true;
                }
            }
            definition = default;
            return false;
        }
    }
}
