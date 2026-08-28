using GoldAndGoblins.Core;

namespace GoldAndGoblins.Economy
{
    public class CurrencyManager : GoldAndGoblins.Utils.Singleton<CurrencyManager>
    {
        public double Gold => SaveManager.Instance.Current.gold;
        public long Gems => SaveManager.Instance.Current.gems;

        public double UpgradeGoldMultiplier { get; set; } = 1.0;
        public double EventGoldMultiplier { get; set; } = 1.0;
        public double PrestigeGoldMultiplier { get; set; } = 1.0;
        public double ActiveGoldMultiplier => UpgradeGoldMultiplier * EventGoldMultiplier * PrestigeGoldMultiplier;

        public void AddGold(double amount)
        {
            if (amount <= 0) return;
            var applied = amount * ActiveGoldMultiplier;
            var data = SaveManager.Instance.Current;
            data.gold += applied;
            data.lifetimeGoldEarned += applied;
            data.totalGoldEverEarned += applied;
            EventBus.Publish(new GoldChangedEvent(data.gold, applied));
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
