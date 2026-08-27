# App Store / Play Store submission checklist

Reference doc — tick these off as you go. Nothing here can be done from this
repo alone; it all requires your developer accounts.

## Both platforms

- [ ] Final app name, bundle identifier chosen (e.g. `com.yourcompany.goldandgoblins`),
      set in Unity's Player Settings for both iOS and Android.
- [ ] App icon exported at all required resolutions (Unity's Player Settings
      icon slots generate most of these for you from one source image).
- [ ] Privacy policy published at a stable URL (required if you show ads or
      collect any analytics — which this game's ad/analytics stubs will once
      you wire in a real provider).
- [ ] IAP product IDs created and match `Assets/_Project/ScriptableObjects/IAP/*`
      `IAPProductSO.productId` values **exactly**.
- [ ] Ad network account created (Unity Ads dashboard, or your chosen network),
      real Game IDs / ad unit IDs set on `UnityAdsProvider` (replace the
      `0000000` placeholders).
- [ ] Sandbox/test purchases verified before submitting for review.

## Apple App Store

- [ ] Apple Developer Program membership (paid, annual).
- [ ] App created in App Store Connect, bundle ID matches Xcode project.
- [ ] In-App Purchases created in App Store Connect (Consumable / Non-Consumable
      / Auto-Renewable Subscription — match `IAPProductSO.productType`), agreements/
      banking/tax info completed (required before IAP goes live).
- [ ] App Privacy questionnaire answered (data types collected, tracking used
      for advertising → triggers the App Tracking Transparency (ATT) prompt if
      you use IDFA for ad personalization).
- [ ] Age rating questionnaire.
- [ ] Screenshots for required device sizes (iPhone 6.7", 6.5", iPad if
      supported), App Preview video optional.
- [ ] Export compliance (encryption) declaration.
- [ ] Signing: certificate + provisioning profile (or automatic signing via
      Xcode with your Apple Developer account).
- [ ] Build uploaded via Xcode/Transporter, submitted for review.

## Google Play Store

- [ ] Google Play Developer account (one-time fee).
- [ ] App created in Play Console.
- [ ] In-app products / subscriptions created in Play Console, matching
      `IAPProductSO.productId`.
- [ ] Play Billing Library version compatible with Unity IAP's current release
      (Unity IAP bundles its own — check the package's release notes if Play
      Console flags an outdated billing library).
- [ ] Data Safety form completed.
- [ ] Content rating questionnaire (IARC).
- [ ] Target API level meets Play Store's current minimum (this changes yearly
      — check Play Console's requirements at submission time and set
      `Player Settings → Android → Target API Level` accordingly).
- [ ] App signing: enroll in Play App Signing, generate/upload your upload key.
- [ ] Feature graphic (1024x500), screenshots per required size, short and
      full store listing descriptions.
- [ ] Internal testing track → closed/open testing → production rollout
      (Play Store's staged rollout process; don't skip straight to production).

## Post-launch

- [ ] Crash reporting wired in (Firebase Crashlytics or similar) — not included
      in this scaffold.
- [ ] Real analytics provider implementing `IAnalyticsProvider`
      (`Assets/_Project/Scripts/Analytics/AnalyticsManager.cs`) instead of the
      debug-log stub.
- [ ] Server-side IAP receipt validation
      (`Assets/_Project/Scripts/Economy/IReceiptValidator.cs`) instead of
      `TrustClientReceiptValidator`.
- [ ] Live-ops content calendar — add `TimedEventDataSO` assets for upcoming
      events ahead of time.
