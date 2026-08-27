using System;
using UnityEngine;
using GoldAndGoblins.Core;

namespace GoldAndGoblins.Ads
{
    public class AdsManager : GoldAndGoblins.Utils.Singleton<AdsManager>
    {
        [SerializeField] private MonoBehaviour providerBehaviour; // must implement IAdsProvider (MockAdsProvider or UnityAdsProvider)

        private IAdsProvider provider;

        protected override void Awake()
        {
            base.Awake();
            provider = providerBehaviour as IAdsProvider;
            if (provider == null)
            {
                Debug.LogWarning("[AdsManager] No provider assigned, falling back to MockAdsProvider.");
                provider = gameObject.AddComponent<MockAdsProvider>();
            }
            provider.Initialize();
        }

        public bool RemoveAdsPurchased => SaveManager.Instance.Current.removeAdsPurchased;

        public void ShowRewardedAd(string placementId, Action onRewardGranted, Action onFailedOrSkipped = null)
        {
            if (provider.IsRewardedReady())
            {
                provider.ShowRewarded(placementId, onRewardGranted, onFailedOrSkipped);
            }
            else
            {
                onFailedOrSkipped?.Invoke();
            }
        }

        public void ShowInterstitialIfAllowed(string placementId)
        {
            if (RemoveAdsPurchased) return;
            if (provider.IsInterstitialReady())
            {
                provider.ShowInterstitial(placementId);
            }
        }
    }
}
