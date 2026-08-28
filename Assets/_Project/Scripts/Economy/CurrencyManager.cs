using GoldAndGoblins.Core;

namespace GoldAndGoblins.Economy
{
    public class CurrencyManager : GoldAndGoblins.Utils.Singleton<CurrencyManager>
    {
        public const double VipGoldBonusMultiplier = 2.0;

        public double Gold => SaveManager.Instance.Current.gold;
        public long Gems => SaveManager.Instance.Current.gems;

        public double UpgradeGoldMultiplier { get; set; } = 1.0;
        public double EventGoldMultiplier { get; set; } = 1.0;
        public double PrestigeGoldMultiplier { get; set; } = 1.0;
        public double VipGoldMultiplier { get; set; } = 1.0;

        public double ActiveGoldMultiplier =>
            UpgradeGoldMultiplier * EventGoldMultiplier * PrestigeGoldMultiplier * VipGoldMultiplier;

        // Call once at boot (and again after prestige / VIP purchase) so multipliers match SaveData.
        public void RefreshPersistentMultipliers(double prestigeMultiplier, bool vipActive)
        {
            PrestigeGoldMultiplier = prestigeMultiplier > 0 ? prestigeMultiplier : 1.0;
            VipGoldMultiplier = vipActive ? VipGoldBonusMultiplier : 1.0;
        }

        public double AddGold(double amount)
        {
            if (amount <= 0) return 0;
            var applied = amount * ActiveGoldMultiplier;
            var data = SaveManager.Instance.Current;
            data.gold += applied;
            data.lifetimeGoldEarned += applied;
            data.totalGoldEverEarned += applied;
            EventBus.Publish(new GoldChangedEvent(data.gold, applied));
            return applied;
        }

        public bool TrySpendGold(double amount)
        {
            var data = SaveManager.Instance.Current;
            if (data.gold < amount) return false;
            data.gold -= amount;
            EventBus.Publish(new GoldChangedEvent(data.gold, -amount));
            return true;
        }

        public void AddGems(long amount)
        {
            if (amount <= 0) return;
            var data = SaveManager.Instance.Current;
            data.gems += amount;
            EventBus.Publish(new GemsChangedEvent(data.gems, amount));
        }

        public bool TrySpendGems(long amount)
        {
            var data = SaveManager.Instance.Current;
            if (data.gems < amount) return false;
            data.gems -= amount;
            EventBus.Publish(new GemsChangedEvent(data.gems, -amount));
            return true;
        }
    }
}
