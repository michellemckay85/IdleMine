using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Purchasing;
using GoldAndGoblins.Economy;
using GoldAndGoblins.Gameplay;
using GoldAndGoblins.LiveOps;
using GoldAndGoblins.UI;

namespace GoldAndGoblins.EditorTools
{
    // Creates the UpgradeDataSO / IAPProductSO+ProductCatalogSO / TimedEventDataSO
    // assets the scaffold never shipped with real data for, and wires them into
    // UpgradeSystem, IAPManager, ShopUIController, and EventManager in the currently
    // open scene. Safe to re-run -- reuses existing assets by id instead of duplicating them.
    public static class GameDataSetup
    {
        private const string UpgradesPath = "Assets/_Project/ScriptableObjects/Upgrades";
        private const string IAPPath = "Assets/_Project/ScriptableObjects/IAP";
        private const string EventsPath = "Assets/_Project/ScriptableObjects/Events";

        [MenuItem("Gold And Goblins/Create Default Game Data")]
        public static void CreateDefaultGameData()
        {
            var upgrades = CreateUpgrades();
            var catalog = CreateProductCatalog();
            var events = CreateEvents();

            WireUpgradeSystem(upgrades);
            WireIAP(catalog);
            WireEvents(events);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[GameDataSetup] Created {upgrades.Count} upgrades, {catalog.products.Count} IAP products, " +
                      $"and {events.Count} live events, wired into the open scene.");
        }

        private static List<UpgradeDataSO> CreateUpgrades()
        {
            Directory.CreateDirectory(UpgradesPath);

            var specs = new[]
            {
                new UpgradeSpec("drill_power", "Pickaxe Power", "Deal more damage per hit.", UpgradeType.DrillPower, 10, 1.15, 1, 0.5, 0),
                new UpgradeSpec("drill_speed", "Swift Strikes", "Tap faster. Shortens the delay between hits.", UpgradeType.DrillSpeed, 25, 1.2, 2, 0.5, 0),
                new UpgradeSpec("gold_multiplier", "Gold Rush", "Earn more gold from every block.", UpgradeType.GoldMultiplier, 100, 1.25, 1, 0.15, 0),
                new UpgradeSpec("critical_chance", "Lucky Strikes", "Chance to deal triple damage on a hit.", UpgradeType.CriticalChance, 30, 1.18, 0, 0.02, 25),
                new UpgradeSpec("auto_miner_speed", "Auto-Miner", "Automatically mines nearby blocks over time.", UpgradeType.AutoMinerSpeed, 200, 1.3, 0, 0.5, 0),
                new UpgradeSpec("max_depth", "Deeper Shafts", "Unlock deeper floors of the mine.", UpgradeType.MaxDepthUnlock, 50, 1.35, 10, 5, 0),
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

        private static List<TimedEventDataSO> CreateEvents()
        {
            Directory.CreateDirectory(EventsPath);

            var specs = new[]
            {
                new EventSpec("double_gold_weekend", "Double Gold Weekend",
                    "All gold earned is doubled while the weekend event is live.",
                    LiveEventType.DoubleGoldWeekend, EventScheduleKind.EveryWeekendUtc, 2.0),
                new EventSpec("grand_opening", "Grand Opening",
                    "A launch bonus: slightly more gold from every block.",
                    LiveEventType.TreasureHunt, EventScheduleKind.AlwaysOn, 1.25),
                new EventSpec("goblin_invasion", "Goblin Invasion",
                    "The mine is crawling — extra gold to compensate.",
                    LiveEventType.GoblinInvasion, EventScheduleKind.OneShotWindow, 1.5),
            };

            var result = new List<TimedEventDataSO>();
            foreach (var spec in specs)
            {
                var path = $"{EventsPath}/Event_{spec.Id}.asset";
                var so = AssetDatabase.LoadAssetAtPath<TimedEventDataSO>(path);
                if (so == null)
                {
                    so = ScriptableObject.CreateInstance<TimedEventDataSO>();
                    AssetDatabase.CreateAsset(so, path);
                }

                so.eventId = spec.Id;
                so.displayName = spec.DisplayName;
                so.description = spec.Description;
                so.eventType = spec.Type;
                so.scheduleKind = spec.Schedule;
                so.goldMultiplier = spec.GoldMultiplier;
                EditorUtility.SetDirty(so);
                result.Add(so);
            }

            return result;
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

        private static void WireEvents(List<TimedEventDataSO> events)
        {
            var eventManager = Object.FindObjectOfType<EventManager>(true);
            if (eventManager == null)
            {
                Debug.LogWarning("[GameDataSetup] No EventManager in the open scene -- run 'Bootstrap Starter Scene' first.");
                return;
            }

            var so = new SerializedObject(eventManager);
            var prop = so.FindProperty("scheduledEvents");
            prop.arraySize = events.Count;
            for (var i = 0; i < events.Count; i++)
            {
                prop.GetArrayElementAtIndex(i).objectReferenceValue = events[i];
            }
            so.ApplyModifiedPropertiesWithoutUndo();
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

        private readonly struct EventSpec
        {
            public readonly string Id, DisplayName, Description;
            public readonly LiveEventType Type;
            public readonly EventScheduleKind Schedule;
            public readonly double GoldMultiplier;

            public EventSpec(string id, string displayName, string description, LiveEventType type,
                EventScheduleKind schedule, double goldMultiplier)
            {
                Id = id; DisplayName = displayName; Description = description; Type = type;
                Schedule = schedule; GoldMultiplier = goldMultiplier;
            }
        }
    }
}
