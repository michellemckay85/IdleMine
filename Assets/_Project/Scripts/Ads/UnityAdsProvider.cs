using System;
using UnityEngine;
using UnityEngine.Advertisements;

namespace GoldAndGoblins.Ads
{
    // Real Unity Ads (com.unity.ads) implementation. Set your Game IDs in the
    // inspector once you've created an ad unit in the Unity Dashboard for each
    // platform, and set rewardedPlacementId / interstitialPlacementId to match.
    public class UnityAdsProvider : MonoBehaviour, IAdsProvider, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
    {
        [SerializeField] private string androidGameId = "0000000";
        [SerializeField] private string iosGameId = "0000000";
        [SerializeField] private bool testMode = true;

        [SerializeField] private string rewardedPlacementId = "Rewarded_Android";
        [SerializeField] private string interstitialPlacementId = "Interstitial_Android";

        private Action pendingRewardCallback;
        private Action pendingFailCallback;
        private bool rewardedLoaded;
        private bool interstitialLoaded;

        public void Initialize()
        {
#if UNITY_IOS
            var gameId = iosGameId;
#else
            var gameId = androidGameId;
#endif
            Advertisement.Initialize(gameId, testMode, this);
        }

        public void OnInitializationComplete()
        {
            Debug.Log("[UnityAdsProvider] Initialized.");
            Advertisement.Load(rewardedPlacementId, this);
            Advertisement.Load(interstitialPlacementId, this);
        }

        public void OnInitializationFailed(UnityAdsInitializationError error, string message) =>
            Debug.LogError($"[UnityAdsProvider] Init failed: {error} - {message}");

        public bool IsRewardedReady() => rewardedLoaded;

        public void ShowRewarded(string placementId, Action onRewardGranted, Action onFailedOrSkipped)
        {
            pendingRewardCallback = onRewardGranted;
            pendingFailCallback = onFailedOrSkipped;
            Advertisement.Show(string.IsNullOrEmpty(placementId) ? rewardedPlacementId : placementId, this);
        }

        public bool IsInterstitialReady() => interstitialLoaded;

        public void ShowInterstitial(string placementId) =>
            Advertisement.Show(string.IsNullOrEmpty(placementId) ? interstitialPlacementId : placementId, this);

        public void OnUnityAdsAdLoaded(string placementId)
        {
            if (placementId == rewardedPlacementId) rewardedLoaded = true;
            if (placementId == interstitialPlacementId) interstitialLoaded = true;
        }

        public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message) =>
            Debug.LogWarning($"[UnityAdsProvider] Failed to load {placementId}: {error} - {message}");

        public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
        {
            pendingFailCallback?.Invoke();
            ClearPending();
        }

        public void OnUnityAdsShowStart(string placementId) { }

        public void OnUnityAdsShowClick(string placementId) { }

        public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
        {
            if (placementId == rewardedPlacementId)
            {
                if (showCompletionState == UnityAdsShowCompletionState.COMPLETED)
                {
                    pendingRewardCallback?.Invoke();
                }
                else
                {
                    pendingFailCallback?.Invoke();
                }
                rewardedLoaded = false;
                Advertisement.Load(rewardedPlacementId, this);
            }
            else if (placementId == interstitialPlacementId)
            {
                interstitialLoaded = false;
                Advertisement.Load(interstitialPlacementId, this);
            }

            ClearPending();
        }

        private void ClearPending()
        {
            pendingRewardCallback = null;
            pendingFailCallback = null;
        }
    }
}
