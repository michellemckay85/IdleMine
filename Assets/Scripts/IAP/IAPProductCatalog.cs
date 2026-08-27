using UnityEngine.Purchasing;

namespace GoldAndGoblins.IAP
{
    public enum RewardKind
    {
        Gold,
        Gems,
        RemoveAds,
        VipBundle,
        GoblinWardSubscription
    }

    public struct IAPProductDefinition
    {
        public string id;
        public ProductType type;
        public RewardKind reward;
        public double goldAmount;
        public long gemAmount;
        public float permanentGoldMultiplier; // only used by RewardKind.VipBundle

        public IAPProductDefinition(string id, ProductType type, RewardKind reward,
            double goldAmount = 0, long gemAmount = 0, float permanentGoldMultiplier = 1f)
        {
            this.id = id;
            this.type = type;
            this.reward = reward;
            this.goldAmount = goldAmount;
            this.gemAmount = gemAmount;
            this.permanentGoldMultiplier = permanentGoldMultiplier;
        }
    }

    /// <summary>
    /// Single source of truth for every purchasable product. The `id` strings here MUST exactly
    /// match the product IDs you create in App Store Connect and the Google Play Console — see
    /// docs/STORE_SUBMISSION_CHECKLIST.md.
    /// </summary>
    public static class IAPProductCatalog
    {
        public static readonly IAPProductDefinition[] All =
        {
            new IAPProductDefinition("gold_pack_small", ProductType.Consumable, RewardKind.Gold, goldAmount: 1000),
            new IAPProductDefinition("gold_pack_medium", ProductType.Consumable, RewardKind.Gold, goldAmount: 6000),
            new IAPProductDefinition("gold_pack_large", ProductType.Consumable, RewardKind.Gold, goldAmount: 14000),
            new IAPProductDefinition("gold_pack_mega", ProductType.Consumable, RewardKind.Gold, goldAmount: 32000),

            new IAPProductDefinition("gem_pack_small", ProductType.Consumable, RewardKind.Gems, gemAmount: 50),
            new IAPProductDefinition("gem_pack_medium", ProductType.Consumable, RewardKind.Gems, gemAmount: 300),
            new IAPProductDefinition("gem_pack_large", ProductType.Consumable, RewardKind.Gems, gemAmount: 700),
            new IAPProductDefinition("gem_pack_mega", ProductType.Consumable, RewardKind.Gems, gemAmount: 1600),

            new IAPProductDefinition("remove_ads", ProductType.NonConsumable, RewardKind.RemoveAds),

            new IAPProductDefinition("starter_vip_bundle", ProductType.NonConsumable, RewardKind.VipBundle,
                goldAmount: 5000, gemAmount: 100, permanentGoldMultiplier: 1.25f),

            new IAPProductDefinition("goblin_ward_monthly", ProductType.Subscription, RewardKind.GoblinWardSubscription),
        };

        public static bool TryGet(string id, out IAPProductDefinition definition)
        {
            foreach (IAPProductDefinition def in All)
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
