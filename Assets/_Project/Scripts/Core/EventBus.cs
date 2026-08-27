using System;
using System.Collections.Generic;

namespace GoldAndGoblins.Core
{
    public static class EventBus
    {
        private static readonly Dictionary<Type, Delegate> handlers = new Dictionary<Type, Delegate>();

        public static void Subscribe<T>(Action<T> handler)
        {
            var type = typeof(T);
            handlers[type] = handlers.TryGetValue(type, out var existing)
                ? Delegate.Combine(existing, handler)
                : handler;
        }

        public static void Unsubscribe<T>(Action<T> handler)
        {
            var type = typeof(T);
            if (!handlers.TryGetValue(type, out var existing)) return;

            var combined = Delegate.Remove(existing, handler);
            if (combined == null)
            {
                handlers.Remove(type);
            }
            else
            {
                handlers[type] = combined;
            }
        }

        public static void Publish<T>(T eventData)
        {
            if (handlers.TryGetValue(typeof(T), out var existing) && existing is Action<T> action)
            {
                action.Invoke(eventData);
            }
        }

        public static void Clear() => handlers.Clear();
    }

    public readonly struct GoldChangedEvent
    {
        public readonly double NewTotal;
        public readonly double Delta;
        public GoldChangedEvent(double newTotal, double delta) { NewTotal = newTotal; Delta = delta; }
    }

    public readonly struct GemsChangedEvent
    {
        public readonly long NewTotal;
        public readonly long Delta;
        public GemsChangedEvent(long newTotal, long delta) { NewTotal = newTotal; Delta = delta; }
    }

    public readonly struct BlockBrokenEvent
    {
        public readonly int Row;
        public readonly int Col;
        public readonly double GoldAwarded;
        public BlockBrokenEvent(int row, int col, double goldAwarded) { Row = row; Col = col; GoldAwarded = goldAwarded; }
    }

    public readonly struct DepthAdvancedEvent
    {
        public readonly int NewDepth;
        public DepthAdvancedEvent(int newDepth) { NewDepth = newDepth; }
    }

    public readonly struct UpgradePurchasedEvent
    {
        public readonly string UpgradeId;
        public readonly int NewLevel;
        public UpgradePurchasedEvent(string upgradeId, int newLevel) { UpgradeId = upgradeId; NewLevel = newLevel; }
    }

    public readonly struct WelcomeBackEvent
    {
        public readonly double OfflineSeconds;
        public readonly double GoldEarned;
        public WelcomeBackEvent(double offlineSeconds, double goldEarned) { OfflineSeconds = offlineSeconds; GoldEarned = goldEarned; }
    }

    public readonly struct PrestigeEvent
    {
        public readonly int NewPrestigeLevel;
        public readonly double PrestigeMultiplier;
        public PrestigeEvent(int newPrestigeLevel, double prestigeMultiplier) { NewPrestigeLevel = newPrestigeLevel; PrestigeMultiplier = prestigeMultiplier; }
    }

    public readonly struct GoblinEncounterStartedEvent
    {
        public readonly string GoblinId;
        public readonly float MaxHealth;
        public GoblinEncounterStartedEvent(string goblinId, float maxHealth) { GoblinId = goblinId; MaxHealth = maxHealth; }
    }

    public readonly struct GoblinHealthChangedEvent
    {
        public readonly float CurrentHealth;
        public readonly float MaxHealth;
        public GoblinHealthChangedEvent(float currentHealth, float maxHealth) { CurrentHealth = currentHealth; MaxHealth = maxHealth; }
    }

    public readonly struct GoblinDefeatedEvent
    {
        public readonly string GoblinId;
        public readonly double GoldLoot;
        public readonly long GemLoot;
        public GoblinDefeatedEvent(string goblinId, double goldLoot, long gemLoot) { GoblinId = goblinId; GoldLoot = goldLoot; GemLoot = gemLoot; }
    }

    public readonly struct LiveEventStartedEvent
    {
        public readonly string EventId;
        public LiveEventStartedEvent(string eventId) { EventId = eventId; }
    }

    public readonly struct LiveEventEndedEvent
    {
        public readonly string EventId;
        public LiveEventEndedEvent(string eventId) { EventId = eventId; }
    }

    public readonly struct PurchaseCompletedEvent
    {
        public readonly string ProductId;
        public PurchaseCompletedEvent(string productId) { ProductId = productId; }
    }

    public readonly struct PurchaseFailedEvent
    {
        public readonly string ProductId;
        public readonly string Reason;
        public PurchaseFailedEvent(string productId, string reason) { ProductId = productId; Reason = reason; }
    }
}
