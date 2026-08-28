using System;
using System.Collections;
using UnityEngine;

namespace GoldAndGoblins.Ads
{
    // Always "succeeds" after a short simulated delay. Lets you build and test the
    // rewarded/interstitial flow before an ad network account is set up. Swap for
    // UnityAdsProvider (or your mediation SDK of choice) before shipping.
    public class MockAdsProvider : MonoBehaviour, IAdsProvider
    {
        public void Initialize() => Debug.Log("[MockAdsProvider] Initialized (no real ad network).");

        public bool IsRewardedReady() => true;

        public void ShowRewarded(string placementId, Action onRewardGranted, Action onFailedOrSkipped)
        {
            StartCoroutine(SimulateAd(onRewardGranted));
        }

        public bool IsInterstitialReady() => true;

        public void ShowInterstitial(string placementId) => Debug.Log($"[MockAdsProvider] Showing interstitial: {placementId}");

        private IEnumerator SimulateAd(Action onComplete)
        {
            Debug.Log("[MockAdsProvider] Simulating rewarded ad playback...");
            yield return new WaitForSeconds(1.5f);
            onComplete?.Invoke();
        }
    }
}
