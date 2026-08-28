using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using GoldAndGoblins.Core;
using GoldAndGoblins.Economy;

namespace GoldAndGoblins.LiveOps
{
    // Scheduling is local (ISO 8601 UTC windows on the ScriptableObject) so events work
    // offline. Swap RefreshActiveEvents' data source for Unity Remote Config / Firebase
    // Remote Config later if you want to push new events without an app update.
    public class EventManager : GoldAndGoblins.Utils.Singleton<EventManager>
    {
        [SerializeField] private List<TimedEventDataSO> scheduledEvents = new List<TimedEventDataSO>();

        private readonly HashSet<string> activeEventIds = new HashSet<string>();
        private readonly HashSet<string> forcedEventIds = new HashSet<string>();

        public IReadOnlyCollection<string> ActiveEventIds => activeEventIds;

        public void RefreshActiveEvents()
        {
            var now = DateTime.UtcNow;
            var combinedMultiplier = 1.0;

            foreach (var evt in scheduledEvents)
            {
                var isActive = IsWithinWindow(evt, now);
                var wasActive = activeEventIds.Contains(evt.eventId);

                if (isActive && !wasActive)
                {
                    activeEventIds.Add(evt.eventId);
                    EventBus.Publish(new LiveEventStartedEvent(evt.eventId));
                }
                else if (!isActive && wasActive)
                {
                    activeEventIds.Remove(evt.eventId);
                    EventBus.Publish(new LiveEventEndedEvent(evt.eventId));
                }

                if (isActive)
                {
                    combinedMultiplier *= evt.goldMultiplier;
                }
            }

            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.EventGoldMultiplier = combinedMultiplier;
            }
        }

        public void ForceStart(string eventId)
        {
            forcedEventIds.Add(eventId);
            RefreshActiveEvents();
        }

        public void ForceEnd(string eventId)
        {
            forcedEventIds.Remove(eventId);
            RefreshActiveEvents();
        }

        public TimedEventDataSO GetEventData(string eventId) => scheduledEvents.FirstOrDefault(e => e.eventId == eventId);

        private bool IsWithinWindow(TimedEventDataSO evt, DateTime now)
        {
            if (forcedEventIds.Contains(evt.eventId)) return true;

            switch (evt.scheduleKind)
            {
                case EventScheduleKind.AlwaysOn:
                    return true;
                case EventScheduleKind.EveryWeekendUtc:
                    return now.DayOfWeek == DayOfWeek.Saturday || now.DayOfWeek == DayOfWeek.Sunday;
                default:
                    if (string.IsNullOrEmpty(evt.startUtcIso8601) || string.IsNullOrEmpty(evt.endUtcIso8601))
                    {
                        return false; // manually controlled via ForceStart/ForceEnd
                    }

                    var hasStart = DateTime.TryParse(evt.startUtcIso8601, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var start);
                    var hasEnd = DateTime.TryParse(evt.endUtcIso8601, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var end);
                    return hasStart && hasEnd && now >= start && now <= end;
            }
        }
    }
}
