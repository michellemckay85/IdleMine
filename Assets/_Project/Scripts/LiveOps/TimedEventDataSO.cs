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

    public enum EventScheduleKind
    {
        OneShotWindow,
        EveryWeekendUtc,
        AlwaysOn
    }

    [CreateAssetMenu(fileName = "Event_", menuName = "Gold And Goblins/Live Event")]
    public class TimedEventDataSO : ScriptableObject
    {
        public string eventId;
        public string displayName;
        [TextArea] public string description;
        public LiveEventType eventType;
        public EventScheduleKind scheduleKind = EventScheduleKind.OneShotWindow;

        [Tooltip("UTC. Used when scheduleKind is OneShotWindow. Leave both blank to schedule manually via EventManager.ForceStart/End.")]
        public string startUtcIso8601;
        public string endUtcIso8601;

        [Tooltip("Applied to gold income while this event is active, e.g. 2 = double gold.")]
        public double goldMultiplier = 1.0;

        public Sprite bannerArt;
    }
}
