using System;

namespace GoldAndGoblins.Ads
{
    public interface IAdsProvider
    {
        void Initialize();
        bool IsRewardedReady();
        void ShowRewarded(string placementId, Action onRewardGranted, Action onFailedOrSkipped);
        bool IsInterstitialReady();
        void ShowInterstitial(string placementId);
    }
}
