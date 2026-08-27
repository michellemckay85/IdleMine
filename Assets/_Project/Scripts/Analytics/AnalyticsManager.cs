using System.Collections.Generic;
using UnityEngine;

namespace GoldAndGoblins.Analytics
{
    public interface IAnalyticsProvider
    {
        void LogEvent(string eventName, Dictionary<string, object> parameters = null);
    }

    // Debug.Log stub so event calls are visible during development without wiring a
    // real SDK. Swap for Firebase Analytics / GA4 / Unity Analytics before shipping --
    // App Store and Play Store both expect the data-safety disclosures for whichever
    // SDK you pick, so decide this before your store listing submission.
    public class DebugLogAnalyticsProvider : IAnalyticsProvider
    {
        public void LogEvent(string eventName, Dictionary<string, object> parameters = null)
        {
            var paramString = parameters == null ? "" : string.Join(", ", parameters);
            Debug.Log($"[Analytics] {eventName} {paramString}");
        }
    }

    public class AnalyticsManager : GoldAndGoblins.Utils.Singleton<AnalyticsManager>
    {
        private IAnalyticsProvider provider = new DebugLogAnalyticsProvider();

        public void SetProvider(IAnalyticsProvider newProvider) => provider = newProvider;

        public void LogEvent(string eventName, Dictionary<string, object> parameters = null) =>
            provider.LogEvent(eventName, parameters);
    }
}
