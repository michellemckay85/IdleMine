using UnityEngine;

namespace GoldAndGoblins.LiveOps
{
    public enum LiveEventType
    {
        DoubleGoldWeekend,
        GoblinInvasion,
        TreasureHunt,
        LimitedShopOffer
    }

    [CreateAssetMenu(fileName = "Event_", menuName = "Gold And Goblins/Live Event")]
    public class TimedEventDataSO : ScriptableObject
    {
        public string eventId;
        public string displayName;
        [TextArea] public string description;
        public LiveEventType eventType;

        [Tooltip("UTC. Leave both blank to schedule manually via EventManager.ForceStart/End.")]
        public string startUtcIso8601;
        public string endUtcIso8601;

        [Tooltip("Applied to gold income while this event is active, e.g. 2 = double gold.")]
        public double goldMultiplier = 1.0;

        public Sprite bannerArt;
    }
}
