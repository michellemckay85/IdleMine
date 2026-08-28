using UnityEngine;
using UnityEngine.Purchasing;

namespace GoldAndGoblins.Economy
{
    public enum RewardKind
    {
        Gems,
        Gold,
        RemoveAds,
        VipPass,
        StarterBundle
    }

    [CreateAssetMenu(fileName = "Product_", menuName = "Gold And Goblins/IAP Product")]
    public class IAPProductSO : ScriptableObject
    {
        [Tooltip("Must exactly match the product ID configured in App Store Connect AND Google Play Console.")]
        public string productId;

        public ProductType productType = ProductType.Consumable;
        public string displayName;
        [TextArea] public string description;

        public RewardKind rewardKind;
        public long gemAmount;
        public double goldAmount;

        [Tooltip("Only for StarterBundle: extra gems granted alongside the primary reward.")]
        public long bonusGems;
    }
}
