# Gold and Goblins

A mobile idle-mining game: tap (or auto-mine) through a vertical mine shaft
breaking blocks for gold, fight the occasional goblin guarding the good stuff,
spend gold on upgrades, collect keys to unlock chests, prestige for a permanent
multiplier once a run is deep enough, and monetize with IAP + rewarded ads +
live events. Built in Unity 2022.3 LTS (URP) for iOS + Android.

## Status

This is a scaffolded, playable-in-editor foundation, not a finished, art-complete
game. Every core system below is implemented in code and wired together by the
editor bootstrap tool; what's still needed before this is store-ready is art,
shader materials, real store/ad accounts, and a testing pass — all called out
under [What you still need to do](#what-you-still-need-to-do).

## Getting started

1. Open this folder in Unity Hub with **Unity 2022.3 LTS** (any recent patch —
   `ProjectSettings/ProjectVersion.txt` pins `2022.3.50f1`, Hub will offer to
   install it or you can point it at whatever 2022.3.x you have).
2. Let Unity resolve packages (`Packages/manifest.json` pulls in Unity IAP,
   Unity Ads, URP, TextMeshPro, Newtonsoft Json, Mobile Notifications).
3. Menu: **Gold And Goblins → Bootstrap Starter Scene**. This builds
   `Assets/_Project/Scenes/Main.unity` with every manager wired up and a
   placeholder UI hierarchy, so you have something to press Play on immediately.
4. Press Play. You'll see log output (mine grid won't render without art —
   see below) but the underlying loop (currency, upgrades, save/load, offline
   earnings) is running.

## Architecture

```
Assets/_Project/Scripts/
  Core/       GameManager (boot order), SaveManager (JSON save/load + offline
              timestamp), EventBus (typed pub/sub every system uses to decouple
              from each other and from UI)
  Economy/    CurrencyManager (gold/gems), UpgradeSystem + UpgradeDataSO
              (ScriptableObject-driven cost/value curves), IAPManager (Unity IAP),
              ProductCatalogSO / IAPProductSO, IReceiptValidator
  Gameplay/   MineGrid, Block, BlockDataSO, DrillInputController (tap + auto-mine),
              IdleEarningsManager (offline progress), PrestigeManager (reset loop)
  Goblins/    GoblinDataSO, GoblinCombatManager (tap-to-fight mini combat + loot)
  LiveOps/    EventManager (timed events with gold multipliers), TimedEventDataSO,
              DailyRewardManager (login streak rewards)
  Ads/        IAdsProvider abstraction, MockAdsProvider (no SDK needed for
              testing), UnityAdsProvider (real com.unity.ads implementation),
              AdsManager facade
  Analytics/  IAnalyticsProvider abstraction, DebugLogAnalyticsProvider stub
  UI/         HUD, upgrade panel/rows, shop panel/rows, event banner, welcome-back
              popup, daily-reward popup, goblin health bar — all driven by EventBus
Assets/_Project/Editor/
  ProjectBootstrapper.cs   "Gold And Goblins → Bootstrap Starter Scene" menu command
Assets/_Project/ScriptableObjects/
  Blocks/ Upgrades/ Goblins/ IAP/ Events/   -- create your data assets here via
  the Create menu (Assets → Create → Gold And Goblins → ...)
```

Everything talks through `EventBus` (see `Core/EventBus.cs` for the full event
list) rather than direct references between gameplay and UI, so you can reskin
the UI or swap in new gameplay screens without touching manager code.

## Plugging in your art and the GritLine Toon Shader

I don't have access to your local art assets or the GritLine package from this
session — only what's in this git repo. Two ways to get them in:

- **Locally (recommended):** open the project in Unity, import your art package
  and the GritLine Toon Shader package, drop assets under
  `Assets/_Project/Art/` and `Assets/_Project/Shaders/` (see the README in each
  folder), build block/goblin prefabs with GritLine materials applied, then
  assign those prefabs to the `visualPrefab` field on your `BlockDataSO` /
  `GoblinDataSO` assets. Commit and push — I can pick up from there.
- **Send me the files:** if you want me to wire the prefabs/materials up in this
  session, share the asset files and I'll do the Inspector-equivalent wiring
  via the ScriptableObject/prefab files directly.

The project defaults to **URP**. If GritLine Toon Shader targets Built-in RP
only, say so and I'll switch the render pipeline package to match — nothing in
the gameplay code is render-pipeline-specific.

## Monetization implemented

- **IAP** (`Economy/IAPManager.cs`, Unity IAP): consumable gem/gold packs,
  non-consumable Remove Ads, VIP pass, starter bundle — all defined as
  `IAPProductSO` assets in a `ProductCatalogSO`. **Product IDs must exactly
  match** what you create in App Store Connect and Google Play Console.
- **Rewarded ads**: double offline earnings, ad-gated bonuses — behind
  `IAdsProvider` so you can ship with `MockAdsProvider` during development and
  swap to `UnityAdsProvider` (or AdMob/LevelPlay by implementing the same
  interface) for release builds.
- **Interstitials**: gated by `removeAdsPurchased`, shown via `AdsManager`.
- **Live events**: `EventManager` runs scheduled (or manually toggled) timed
  events with gold multipliers and a shop/UI banner — double-gold weekends,
  goblin invasions, limited shop offers.
- **Daily rewards**: `DailyRewardManager` tracks a login streak and grants
  scaling rewards, resetting the streak if a day is missed.
- **Prestige**: `PrestigeManager` lets players reset progress once they've
  earned enough lifetime gold, for a permanent multiplier — the standard
  idle-game retention loop.

IAP, ads, and events are all local/client-side, which is normal for this
genre but means **IAP receipts are trusted, not server-validated** (see
`IReceiptValidator` — replace `TrustClientReceiptValidator` before launch if
you want real fraud protection). The leaderboard (below) is the first
feature that actually talks to a backend.

## Social / backend features (Unity Gaming Services)

Chosen backend for cross-player features (leaderboards, and eventually
league/alliance/trade/chat) is **Unity Gaming Services (UGS)**. Only the
leaderboard is built so far — the rest are large, separate features to be
added one at a time.

- **Leaderboard** (`Social/LeaderboardManager.cs`): ranks players by
  `SaveData.totalGoldEverEarned` — a cumulative stat that, unlike
  `lifetimeGoldEarned`, never resets on prestige. Anonymous sign-in via
  `AuthenticationService`, scores submitted/fetched via
  `LeaderboardsService`. UI: `LeaderboardPanelController` +
  `LeaderboardRowController`, opened from the nav bar's "Ranks" button.

**Required one-time setup this code can't do for you** (needs your Unity
account):
1. In Unity: **Edit → Project Settings → Services** — sign in and link (or
   create) a Unity Gaming Services project.
2. In the [Unity Cloud Dashboard](https://cloud.unity.com) for that project,
   open **Leaderboards** and create one with the exact ID
   `total_gold_earned`, sort order **Descending**, score format **Long**.

Until that's done, the leaderboard panel will show "unavailable" rather than
erroring — it fails soft.

**Not yet built**: league, alliance/guild, trade, and global+alliance chat
with translation. All four need real backend design (shared persistent
state, server-authoritative trade to prevent duplication exploits, chat
infrastructure, a paid translation API) — much larger scope than anything
else in this repo. Build one at a time against UGS services as they're
tackled (Cloud Save for persistent group state, Lobby/Relay or a
third-party chat SDK for messaging).

## What you still need to do

This is the honest gap list between "scaffolded" and "on the App Store":

- **Art & shader integration** (see above) — nothing renders without it.
- **Scene/prefab polish in-editor** — the bootstrapper gives you a working
  hierarchy, but real UI layout, animations, and juice (screen shake, particle
  bursts, SFX) need to be built by hand in Unity.
- **Store accounts & product setup**: create the app in App Store Connect and
  Google Play Console, create matching IAP product IDs, set up a shared/reused
  Unity Ads (or other network) account and ad unit IDs.
- **Server-side receipt validation** before relying on IAP in production.
- **Full `ProjectSettings.asset`** fields: bundle identifier
  (`com.yourcompany.goldandgoblins` or similar), version/build number, icons,
  splash screen, min iOS/Android OS versions, orientation lock, target API
  level for Android (Play Store requires a recent one — check current
  requirements at submission time).
- **Privacy**: a privacy policy URL (required by both stores if you collect any
  data or show ads), App Store's App Privacy questionnaire (data linked to
  users, tracking for ads → ATT prompt on iOS), Play Store's Data Safety form.
- **Age rating / content rating questionnaires** on both stores.
- **Store listing assets**: screenshots per required device size, feature
  graphic (Play Store), app icon in all required resolutions, promotional text.
- **Testing**: a real device pass on both platforms, IAP sandbox testing
  (Apple sandbox tester account, Google license testers), ad mediation testing.
- **Build & signing**: iOS signing certificate + provisioning profile (or
  Xcode automatic signing) via Apple Developer Program membership; Android
  keystore + Play App Signing enrollment.

I can help with any of these next — happy to build out full block/goblin
prefabs once art is available, write more live-event content, add more UI
polish, or draft the store listing copy.
