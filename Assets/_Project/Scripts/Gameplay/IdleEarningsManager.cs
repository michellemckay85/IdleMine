using System;
using UnityEngine;
using GoldAndGoblins.Core;
using GoldAndGoblins.Economy;

namespace GoldAndGoblins.Gameplay
{
    public class IdleEarningsManager : GoldAndGoblins.Utils.Singleton<IdleEarningsManager>
    {
        [SerializeField] private double baseIdleGoldPerSecond = 1.0;
        [SerializeField] private double maxOfflineHours = 8.0;
        [SerializeField] private double adExtendedOfflineHours = 16.0;

        private double pendingOfflineSeconds;

        public void ApplyOfflineEarnings()
        {
            var data = SaveManager.Instance.Current;
            var elapsed = (DateTime.UtcNow - data.LastSaveUtc).TotalSeconds;
            if (elapsed < 30) return; // not worth a popup for a quick app-switch

            var cappedSeconds = Math.Min(elapsed, maxOfflineHours * 3600.0);
            var rate = CurrentIdleRatePerSecond();
            var earned = cappedSeconds * rate;

            if (earned > 0)
            {
                var applied = CurrencyManager.Instance.AddGold(earned);
                pendingOfflineSeconds = cappedSeconds;
                EventBus.Publish(new WelcomeBackEvent(cappedSeconds, applied));
            }
        }

        // Call from the "watch ad to double offline earnings" button.
        public void ClaimExtendedOfflineBonus()
        {
            if (pendingOfflineSeconds <= 0) return;

            var extraSeconds = Math.Min(pendingOfflineSeconds, adExtendedOfflineHours * 3600.0) ;
            var bonusEarned = extraSeconds * CurrentIdleRatePerSecond();
            CurrencyManager.Instance.AddGold(bonusEarned);
            pendingOfflineSeconds = 0;
        }

        // Base idle rate only. Upgrade / prestige / VIP / event multipliers are applied
        // once inside CurrencyManager.AddGold — do not multiply them here or gold is doubled.
        private double CurrentIdleRatePerSecond() => baseIdleGoldPerSecond;
    }
}
