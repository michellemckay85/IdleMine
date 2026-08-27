using System;
using System.Collections.Generic;

namespace GoldAndGoblins.Core
{
    [Serializable]
    public class SaveData
    {
        public int saveVersion = 1;
        public string lastSaveUtcTicks = "0";

        public double gold;
        public long gems;

        public int currentDepth = 1;
        public int prestigeLevel;
        public double lifetimeGoldEarned;

        public List<UpgradeLevelEntry> upgradeLevels = new List<UpgradeLevelEntry>();

        public bool removeAdsPurchased;
        public bool vipActive;
        public List<string> ownedNonConsumableProductIds = new List<string>();

        public int dailyRewardStreak;
        public string lastDailyRewardUtcTicks = "0";

        public List<string> claimedOneTimeEventRewardIds = new List<string>();

        public DateTime LastSaveUtc
        {
            get => new DateTime(long.Parse(lastSaveUtcTicks), DateTimeKind.Utc);
            set => lastSaveUtcTicks = value.Ticks.ToString();
        }
    }

    [Serializable]
    public class UpgradeLevelEntry
    {
        public string upgradeId;
        public int level;
    }
}
