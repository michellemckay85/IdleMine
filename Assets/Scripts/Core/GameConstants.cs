namespace GoldAndGoblins.Core
{
    /// <summary>Central place for save keys and tunable caps so Core/Mining/IAP scripts agree on names.</summary>
    public static class GameConstants
    {
        public const string SaveFileName = "goldandgoblins_save.json";

        public const float BaseOfflineCapHours = 12f;
        public const float VipOfflineCapHours = 24f;

        public const float GoblinRaidIntervalSeconds = 180f;
        public const float GoblinRaidWindowSeconds = 10f;
        public const float GoblinRaidStealFraction = 0.15f;
        public const int GoblinRaidTapsToDefend = 8;
    }
}
