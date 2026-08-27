using System;
using System.Collections.Generic;

namespace GoldAndGoblins.Core
{
    [Serializable]
    public class UpgradeLevelEntry
    {
        public string upgradeId;
        public int level;
    }

    /// <summary>
    /// Plain-data save model. Kept flat and JsonUtility-friendly (no Dictionary/DateTime fields)
    /// so it can round-trip through JsonUtility without a custom converter.
    /// </summary>
    [Serializable]
    public class GameSaveData
    {
        public double gold;
        public long gems;

        public List<UpgradeLevelEntry> upgradeLevels = new List<UpgradeLevelEntry>();

        public bool removeAdsPurchased;
        public bool vipBundlePurchased;
        public float permanentGoldMultiplier = 1f;

        public bool goblinWardSubscriptionActive;
        public string goblinWardExpiryUtc = string.Empty;

        // ISO-8601 UTC timestamp of the last time the game was saved/backgrounded.
        // Used on load to compute idle/offline earnings.
        public string lastSaveTimeUtc = string.Empty;

        public static GameSaveData CreateNew()
        {
            return new GameSaveData
            {
                gold = 0,
                gems = 0,
                upgradeLevels = new List<UpgradeLevelEntry>(),
                removeAdsPurchased = false,
                vipBundlePurchased = false,
                permanentGoldMultiplier = 1f,
                goblinWardSubscriptionActive = false,
                goblinWardExpiryUtc = string.Empty,
                lastSaveTimeUtc = DateTime.UtcNow.ToString("o")
            };
        }
    }
}
