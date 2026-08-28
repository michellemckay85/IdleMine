using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Purchasing;
using GoldAndGoblins.Economy;
using GoldAndGoblins.Gameplay;
using GoldAndGoblins.UI;

namespace GoldAndGoblins.EditorTools
{
    // Creates the UpgradeDataSO / IAPProductSO+ProductCatalogSO assets the scaffold
    // never shipped with real data for, and wires them into UpgradeSystem, IAPManager,
    // and ShopUIController in the currently open scene. Safe to re-run -- reuses
    // existing assets by id instead of duplicating them.
    //
    // Only ships upgrades for UpgradeType values that actually have a gameplay effect
    // wired up today (DrillPower, GoldMultiplier, CriticalChance, AutoMinerSpeed).
    // DrillSpeed and MaxDepthUnlock exist as enum options but nothing in the codebase
    // reads them yet (no tap-cooldown or depth-gating mechanic), so selling upgrades
    // for them would charge players for nothing -- add those once the mechanic exists.
    public static class GameDataSetup
    {
        private const string UpgradesPath = "Assets/_Project/ScriptableObjects/Upgrades";
        private const string IAPPath = "Assets/_Project/ScriptableObjects/IAP";

        [MenuItem("Gold And Goblins/Create Default Game Data")]
        public static void CreateDefaultGameData()
        {
            var upgrades = CreateUpgrades();
            var catalog = CreateProductCatalog();

            WireUpgradeSystem(upgrades);
            WireIAP(catalog);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[GameDataSetup] Created {upgrades.Count} upgrades and {catalog.products.Count} IAP products, " +
                      "wired into UpgradeSystem/IAPManager/ShopUIController in the open scene.");
        }

        private static List<UpgradeDataSO> CreateUpgrades()
        {
            Directory.CreateDirectory(UpgradesPath);

            var specs = new[]
            {
                new UpgradeSpec("drill_power", "Pickaxe Power", "Deal more damage per hit.", UpgradeType.DrillPower, 10, 1.15, 1, 0.5, 0),
                new UpgradeSpec("gold_multiplier", "Gold Rush", "Earn more gold from every block.", UpgradeType.GoldMultiplier, 100, 1.25, 1, 0.15, 0),
                new UpgradeSpec("critical_chance", "Lucky Strikes", "Chance to deal triple damage on a hit.", UpgradeType.CriticalChance, 30, 1.18, 0, 0.02, 25),
                new UpgradeSpec("auto_miner_speed", "Auto-Miner", "Automatically mines nearby blocks over time.", UpgradeType.AutoMinerSpeed, 200, 1.3, 0, 0.5, 0),
            };

            var result = new List<UpgradeDataSO>();
            foreach (var spec in specs)
            {
                var path = $"{UpgradesPath}/Upgrade_{spec.Id}.asset";
                var so = AssetDatabase.LoadAssetAtPath<UpgradeDataSO>(path);
                if (so == null)
                {
                    so = ScriptableObject.CreateInstance<UpgradeDataSO>();
                    AssetDatabase.CreateAsset(so, path);
                }

                so.upgradeId = spec.Id;
                so.displayName = spec.DisplayName;
                so.description = spec.Description;
                so.upgradeType = spec.Type;
                so.baseCost = spec.BaseCost;
                so.costGrowthRate = spec.CostGrowthRate;
                so.baseValue = spec.BaseValue;
                so.valuePerLevel = spec.ValuePerLevel;
                so.maxLevel = spec.MaxLevel;
                EditorUtility.SetDirty(so);

                result.Add(so);
            }

            return result;
        }

        private static ProductCatalogSO CreateProductCatalog()
        {
            Directory.CreateDirectory(IAPPath);

            var specs = new[]
            {
                new ProductSpec("gems_small", "Handful of Gems", "50 gems", ProductType.Consumable, RewardKind.Gems, gemAmount: 50),
                new ProductSpec("gems_medium", "Bag of Gems", "300 gems (+10% bonus)", ProductType.Consumable, RewardKind.Gems, gemAmount: 330),
                new ProductSpec("gems_large", "Chest of Gems", "1000 gems (+25% bonus)", ProductType.Consumable, RewardKind.Gems, gemAmount: 1250),
                new ProductSpec("remove_ads", "Remove Ads", "No more interstitial ads, forever.", ProductType.NonConsumable, RewardKind.RemoveAds),
                new ProductSpec("vip_pass", "VIP Pass", "Permanent VIP perks.", ProductType.NonConsumable, RewardKind.VipPass),
                new ProductSpec("starter_bundle", "Starter Bundle", "200 gems + 5,000 gold. One-time offer.", ProductType.Consumable, RewardKind.StarterBundle, gemAmount: 200, goldAmount: 5000),
            };

            var products = new List<IAPProductSO>();
            foreach (var spec in specs)
            {
                var path = $"{IAPPath}/Product_{spec.Id}.asset";
                var so = AssetDatabase.LoadAssetAtPath<IAPProductSO>(path);
                if (so == null)
                {
                    so = ScriptableObject.CreateInstance<IAPProductSO>();
                    AssetDatabase.CreateAsset(so, path);
                }

                so.productId = spec.Id;
                so.productType = spec.Type;
                so.displayName = spec.DisplayName;
                so.description = spec.Description;
                so.rewardKind = spec.Reward;
                so.gemAmount = spec.GemAmount;
                so.goldAmount = spec.GoldAmount;
                so.bonusGems = spec.BonusGems;
                EditorUtility.SetDirty(so);

                products.Add(so);
            }

            var catalogPath = $"{IAPPath}/ProductCatalog.asset";
            var catalog = AssetDatabase.LoadAssetAtPath<ProductCatalogSO>(catalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<ProductCatalogSO>();
                AssetDatabase.CreateAsset(catalog, catalogPath);
            }
            catalog.products = products;
            EditorUtility.SetDirty(catalog);

            return catalog;
        }

        private static void WireUpgradeSystem(List<UpgradeDataSO> upgrades)
        {
            var upgradeSystem = Object.FindObjectOfType<UpgradeSystem>(true);
            if (upgradeSystem == null)
            {
                Debug.LogWarning("[GameDataSetup] No UpgradeSystem in the open scene -- run 'Bootstrap Starter Scene' first.");
                return;
            }

            var so = new SerializedObject(upgradeSystem);
            var prop = so.FindProperty("upgrades");
            prop.arraySize = upgrades.Count;
            for (var i = 0; i < upgrades.Count; i++)
            {
                prop.GetArrayElementAtIndex(i).objectReferenceValue = upgrades[i];
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void WireIAP(ProductCatalogSO catalog)
        {
            var iapManager = Object.FindObjectOfType<IAPManager>(true);
            if (iapManager != null)
            {
                AssignSerializedField(iapManager, "catalog", catalog);
            }

            var shopUI = Object.FindObjectOfType<ShopUIController>(true);
            if (shopUI != null)
            {
                AssignSerializedField(shopUI, "catalog", catalog);
            }
        }

        private static void AssignSerializedField(Object target, string fieldName, Object value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(fieldName);
            if (prop == null)
            {
                Debug.LogWarning($"[GameDataSetup] Could not find serialized field '{fieldName}' on {target.GetType().Name}");
                return;
            }
            prop.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private readonly struct UpgradeSpec
        {
            public readonly string Id, DisplayName, Description;
            public readonly UpgradeType Type;
            public readonly double BaseCost, CostGrowthRate, BaseValue, ValuePerLevel;
            public readonly int MaxLevel;

            public UpgradeSpec(string id, string displayName, string description, UpgradeType type,
                double baseCost, double costGrowthRate, double baseValue, double valuePerLevel, int maxLevel)
            {
                Id = id; DisplayName = displayName; Description = description; Type = type;
                BaseCost = baseCost; CostGrowthRate = costGrowthRate; BaseValue = baseValue;
                ValuePerLevel = valuePerLevel; MaxLevel = maxLevel;
            }
        }

        private readonly struct ProductSpec
        {
            public readonly string Id, DisplayName, Description;
            public readonly ProductType Type;
            public readonly RewardKind Reward;
            public readonly long GemAmount, BonusGems;
            public readonly double GoldAmount;

            public ProductSpec(string id, string displayName, string description, ProductType type, RewardKind reward,
                long gemAmount = 0, double goldAmount = 0, long bonusGems = 0)
            {
                Id = id; DisplayName = displayName; Description = description; Type = type; Reward = reward;
                GemAmount = gemAmount; GoldAmount = goldAmount; BonusGems = bonusGems;
            }
        }
    }
}
