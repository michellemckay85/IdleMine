# Store Submission Checklist

Steps only you (as the account holder) can do — Claude Code cannot access
App Store Connect, Google Play Console, or your developer accounts.

## Accounts

- [ ] Apple Developer Program membership active ($99/yr) —
      developer.apple.com/account
- [ ] Google Play Console account active ($25 one-time) —
      play.google.com/console

## App identity (must match the Unity project exactly)

- [ ] Decide the bundle ID / package name, e.g.
      `com.yourcompany.goldandgoblins`, and set it in Unity Player Settings
      (see `docs/SETUP_GUIDE.md` §3) **before** creating the store listings.

## App Store Connect (iOS)

1. [ ] Create a new App ID in the Apple Developer portal matching the bundle
       identifier, with In-App Purchase capability enabled.
2. [ ] Create the app record in App Store Connect (My Apps → +) using that
       bundle ID.
3. [ ] Fill in app information: name "Gold and Goblins", category, age
       rating questionnaire, privacy policy URL, screenshots (per required
       device sizes), description, keywords, support URL.
4. [ ] Under **Features → In-App Purchases**, create each product with the
       **exact** Product ID from the table below — Product ID must match
       `Assets/Scripts/IAP/IAPProductCatalog.cs` character-for-character.
5. [ ] Set up a **Paid Applications Agreement** / banking & tax info (Apple
       won't let IAP go live without this).
6. [ ] Create at least one **Sandbox Tester** account (Users and Access →
       Sandbox Testers) to test purchases before release.
7. [ ] Build & archive in Xcode from the project produced by
       `Gold And Goblins → Build → iOS Xcode Project`, then upload via
       Xcode Organizer or Transporter.
8. [ ] Submit the build + IAP products together for review (new IAP products
       are reviewed alongside the first app version that references them).

## Google Play Console (Android)

1. [ ] Create the app (All apps → Create app), package name matching Unity's
       Android package name exactly.
2. [ ] Complete the store listing (description, screenshots, feature
       graphic), content rating questionnaire, target audience, data safety
       form, privacy policy URL.
3. [ ] Under **Monetize → Products → In-app products**, create each
       consumable/non-consumable with the **exact** Product ID from the
       table below.
4. [ ] Under **Monetize → Products → Subscriptions**, create
       `goblin_ward_monthly` as a subscription with your chosen price and
       billing period (monthly).
5. [ ] Set up a **Play App Signing** keystore (Play Console will generate/
       manage this, or you upload your own upload key) — configure the
       matching upload keystore in Unity's Player Settings → Publishing
       Settings before running the Android build.
6. [ ] Add license testers (Settings → License testing) to test real
       purchase flows without being charged.
7. [ ] Upload the `.aab` from `Builds/Android/GoldAndGoblins.aab` to a
       testing track first (Internal testing), verify IAP end-to-end, then
       promote to Production.

## Product ID reference (must match on both stores and in code)

| Product ID | Store type |
|---|---|
| `gold_pack_small` | Consumable |
| `gold_pack_medium` | Consumable |
| `gold_pack_large` | Consumable |
| `gold_pack_mega` | Consumable |
| `gem_pack_small` | Consumable |
| `gem_pack_medium` | Consumable |
| `gem_pack_large` | Consumable |
| `gem_pack_mega` | Consumable |
| `remove_ads` | Non-consumable |
| `starter_vip_bundle` | Non-consumable |
| `goblin_ward_monthly` | Auto-renewing subscription |

Source of truth: `Assets/Scripts/IAP/IAPProductCatalog.cs`. If you rename or
add a product there, mirror the change in both consoles before shipping.

## Pricing

Both consoles let you pick a price tier per product; this repo doesn't
hardcode prices (the game reads the store's localized price at runtime via
`IAPManager.GetLocalizedPriceString`), so you're free to set/adjust prices in
each console without touching code.

## Before you submit

- [ ] Test every purchase (including Restore Purchases on iOS) on a real
      device using a sandbox/license tester account — the in-Editor "fake
      store" only validates game logic, not real store plumbing.
- [ ] Confirm the offline-earnings, goblin-raid, and upgrade math feel right
      at your intended pacing (tune constants in
      `Assets/Scripts/Core/GameConstants.cs` and
      `Assets/Scripts/Mining/UpgradeDefinition.cs`).
- [ ] Replace the `AdsGateway` TODOs with a real ad SDK if you want ad
      revenue in addition to IAP (not included here — see
      `Assets/Scripts/Ads/AdsGateway.cs`).
- [ ] Have a privacy policy URL ready — both stores require one, and IAP +
      any ad SDK both typically trigger additional data-safety disclosures.
