using System;
using UnityEngine;
using GoldAndGoblins.Core;
using GoldAndGoblins.Economy;

namespace GoldAndGoblins.LiveOps
{
    [Serializable]
    public class DailyRewardEntry
    {
        public long gems;
        public double gold;
    }

    public class DailyRewardManager : GoldAndGoblins.Utils.Singleton<DailyRewardManager>
    {
        [SerializeField]
        private DailyRewardEntry[] rewardsByDay =
        {
            new DailyRewardEntry { gold = 50 },
            new DailyRewardEntry { gold = 100 },
            new DailyRewardEntry { gems = 5 },
            new DailyRewardEntry { gold = 250 },
            new DailyRewardEntry { gems = 10 },
            new DailyRewardEntry { gold = 500 },
            new DailyRewardEntry { gems = 25, gold = 1000 } // day 7 jackpot
        };

        public bool HasRewardReadyToday { get; private set; }
        public int CurrentStreak => SaveManager.Instance.Current.dailyRewardStreak;

        public void CheckAndOfferDailyReward()
        {
            var data = SaveManager.Instance.Current;
            var neverClaimed = string.IsNullOrEmpty(data.lastDailyRewardUtcTicks) || data.lastDailyRewardUtcTicks == "0";
            if (neverClaimed)
            {
                HasRewardReadyToday = true;
                return;
            }

            var last = new DateTime(long.Parse(data.lastDailyRewardUtcTicks), DateTimeKind.Utc);
            HasRewardReadyToday = (DateTime.UtcNow.Date - last.Date).Days >= 1;
        }

        public void ClaimDailyReward()
        {
            if (!HasRewardReadyToday) return;

            var data = SaveManager.Instance.Current;
            var neverClaimed = string.IsNullOrEmpty(data.lastDailyRewardUtcTicks) || data.lastDailyRewardUtcTicks == "0";
            var now = DateTime.UtcNow;

            var brokeStreak = !neverClaimed && (now.Date - new DateTime(long.Parse(data.lastDailyRewardUtcTicks), DateTimeKind.Utc).Date).Days > 1;
            data.dailyRewardStreak = brokeStreak || neverClaimed ? 1 : data.dailyRewardStreak + 1;
            if (data.dailyRewardStreak > rewardsByDay.Length) data.dailyRewardStreak = 1;

            var reward = rewardsByDay[Mathf.Clamp(data.dailyRewardStreak - 1, 0, rewardsByDay.Length - 1)];
            if (reward.gold > 0) CurrencyManager.Instance.AddGold(reward.gold);
            if (reward.gems > 0) CurrencyManager.Instance.AddGems(reward.gems);

            data.lastDailyRewardUtcTicks = now.Ticks.ToString();
            HasRewardReadyToday = false;
        }
    }
}
