using System;
using GoldAndGoblins.Core;
using UnityEngine;

namespace GoldAndGoblins.Ads
{
    /// <summary>
    /// Thin gate in front of whichever ad SDK you integrate (AdMob, Unity LevelPlay, etc). This
    /// project intentionally does not bundle an ad SDK - wire ShowInterstitial/ShowRewarded into
    /// your chosen SDK's callbacks. Every entry point respects the "remove_ads" purchase.
    /// </summary>
    public class AdsGateway : MonoBehaviour
    {
        public bool AdsRemoved { get; private set; }

        public void InitializeFromSave(GameSaveData data)
        {
            AdsRemoved = data.removeAdsPurchased;
        }

        public void SetAdsRemoved(bool removed)
        {
            AdsRemoved = removed;
        }

        /// <summary>Call at natural break points (e.g. after a goblin raid resolves). No-ops if ads were removed.</summary>
        public void ShowInterstitial(Action onClosed = null)
        {
            if (AdsRemoved)
            {
                onClosed?.Invoke();
                return;
            }

            // TODO: integrate ad SDK here, e.g. AdMob's InterstitialAd.Show().
            Debug.Log("[AdsGateway] Would show interstitial ad here.");
            onClosed?.Invoke();
        }

        /// <summary>Opt-in rewarded ad (e.g. "watch to double offline earnings"). Still offered even if ads were removed, since it's player-initiated.</summary>
        public void ShowRewarded(Action<bool> onComplete)
        {
            // TODO: integrate ad SDK here, e.g. AdMob's RewardedAd.Show().
            Debug.Log("[AdsGateway] Would show rewarded ad here.");
            onComplete?.Invoke(true);
        }
    }
}
