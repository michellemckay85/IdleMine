using System;
using GoldAndGoblins.Core;
using UnityEngine;

namespace GoldAndGoblins.Mining
{
    /// <summary>
    /// Periodically threatens a goblin raid on the player's banked gold. While the game is in
    /// the foreground, a raid opens a short defend window during which the player must tap
    /// RegisterDefendTap() enough times; failing to do so costs a fraction of current gold.
    /// If the "Goblin Ward" subscription (see IAP) is active, raids auto-resolve as a win with
    /// a small bonus and never threaten the player's gold.
    /// </summary>
    public class GoblinRaidManager : MonoBehaviour
    {
        [SerializeField] private CurrencyManager currencyManager;

        public event Action OnRaidStarted;
        public event Action<int, int> OnDefendProgress; // (currentTaps, tapsRequired)
        public event Action<double> OnRaidDefended;      // bonus gold awarded
        public event Action<double> OnRaidFailed;        // gold stolen

        public bool GoblinWardActive { get; private set; }
        public bool RaidInProgress { get; private set; }

        private float _timeUntilNextRaid;
        private float _raidWindowRemaining;
        private int _defendTaps;
        private DateTime? _goblinWardExpiryUtc;

        public void InitializeFromSave(GameSaveData data)
        {
            GoblinWardActive = data.goblinWardSubscriptionActive;
            if (!string.IsNullOrEmpty(data.goblinWardExpiryUtc) &&
                DateTime.TryParse(data.goblinWardExpiryUtc, null,
                    System.Globalization.DateTimeStyles.RoundtripKind, out DateTime expiry))
            {
                _goblinWardExpiryUtc = expiry;
                if (expiry <= DateTime.UtcNow) GoblinWardActive = false;
            }

            _timeUntilNextRaid = GameConstants.GoblinRaidIntervalSeconds;
        }

        public void ApplyToSave(GameSaveData data)
        {
            data.goblinWardSubscriptionActive = GoblinWardActive;
            data.goblinWardExpiryUtc = _goblinWardExpiryUtc?.ToString("o") ?? string.Empty;
        }

        /// <summary>Called by IAPManager when the goblin_ward_monthly subscription state changes.</summary>
        public void SetGoblinWardActive(bool active, DateTime? expiryUtc)
        {
            GoblinWardActive = active;
            _goblinWardExpiryUtc = expiryUtc;
        }

        private void Update()
        {
            if (RaidInProgress)
            {
                TickRaidWindow();
                return;
            }

            _timeUntilNextRaid -= Time.deltaTime;
            if (_timeUntilNextRaid <= 0f)
            {
                StartRaid();
            }
        }

        private void StartRaid()
        {
            RaidInProgress = true;
            _defendTaps = 0;
            _raidWindowRemaining = GameConstants.GoblinRaidWindowSeconds;
            _timeUntilNextRaid = GameConstants.GoblinRaidIntervalSeconds;

            if (GoblinWardActive)
            {
                // Goblin Ward subscribers never lose gold and get a small thank-you bonus.
                double bonus = Math.Max(10, currencyManager.Gold * 0.02);
                currencyManager.AddGoldRaw(bonus);
                RaidInProgress = false;
                OnRaidDefended?.Invoke(bonus);
                return;
            }

            OnRaidStarted?.Invoke();
        }

        private void TickRaidWindow()
        {
            _raidWindowRemaining -= Time.deltaTime;
            if (_raidWindowRemaining <= 0f)
            {
                ResolveRaidFailure();
            }
        }

        /// <summary>Call from the raid UI's "Defend!" button while a raid is in progress.</summary>
        public void RegisterDefendTap()
        {
            if (!RaidInProgress) return;

            _defendTaps++;
            OnDefendProgress?.Invoke(_defendTaps, GameConstants.GoblinRaidTapsToDefend);

            if (_defendTaps >= GameConstants.GoblinRaidTapsToDefend)
            {
                ResolveRaidSuccess();
            }
        }

        private void ResolveRaidSuccess()
        {
            RaidInProgress = false;
            double bonus = Math.Max(5, currencyManager.Gold * 0.05);
            currencyManager.AddGoldRaw(bonus);
            OnRaidDefended?.Invoke(bonus);
        }

        private void ResolveRaidFailure()
        {
            RaidInProgress = false;
            double stolen = currencyManager.Gold * GameConstants.GoblinRaidStealFraction;
            currencyManager.AddGoldRaw(-stolen);
            OnRaidFailed?.Invoke(stolen);
        }
    }
}
