# Gold and Goblins

A mobile idle-mining game: tap (or auto-mine) through a vertical mine shaft
breaking blocks for gold, fight the occasional goblin guarding the good stuff,
spend gold on upgrades, collect keys to unlock chests, prestige for a permanent
multiplier once a run is deep enough, and monetize with IAP + rewarded ads +
live events. Built in Unity 2022.3 LTS (URP) for iOS + Android.

## Status

This is a scaffolded, playable-in-editor foundation, not a finished, store-ready
game. Core systems are implemented and wired by the editor bootstrap tools; KayKit
art, UI sprites, and the GritLine toon shader are in the repo. What's still needed
before this is store-ready is polish, real store/ad accounts, and a testing pass —
all called out under [What you still need to do](#what-you-still-need-to-do).

## Getting started

1. Open this folder in Unity Hub with **Unity 2022.3 LTS** (any recent patch —
   `ProjectSettings/ProjectVersion.txt` pins `2022.3.50f1`, Hub will offer to
   install it or you can point it at whatever 2022.3.x you have).
2. Let Unity resolve packages (`Packages/manifest.json` pulls in Unity IAP,
   Unity Ads, URP, TextMeshPro, Newtonsoft Json, Mobile Notifications).
3. Menu, in this order:
   **Gold And Goblins → Bootstrap Starter Scene**
   **→ Wire Up Imported Art**
   **→ Wire Up Scene**
   **→ Wire Up GritLine Materials**
   **→ Build UI Layout**
   **→ Create Default Game Data**
   This builds `Assets/_Project/Scenes/Main.unity` with managers, art, UI, upgrades,
   IAP products, and live events wired up.
4. Press Play. Tap blocks, buy upgrades, hit the depth cap, prestige from the HUD.

## Architecture

```
Assets/_Project/Scripts/
  Core/       GameManager (boot order), SaveManager (JSON save/load + offline
              timestamp), EventBus (typed pub/sub every system uses to decouple
              from each other and from UI)
  Economy/    CurrencyManager (gold/gems), UpgradeSystem + UpgradeDataSO
              (ScriptableObject-driven cost/value curves), IAPManager (Unity IAP),
              ProductCatalogSO / IAPProductSO, IReceiptValidator
  Gameplay/   MineGrid, Block, BlockDataSO, DrillInputController (tap + auto-mine +
              tap cooldown from DrillSpeed), IdleEarningsManager (offline progress),
              PrestigeManager (reset loop; multiplier applies to ALL gold),
              FeedbackManager (camera shake on breaks)
  Goblins/    GoblinDataSO, GoblinCombatManager (tap-to-fight mini combat + loot)
  LiveOps/    EventManager (AlwaysOn / every-weekend-UTC / one-shot windows),
              TimedEventDataSO, DailyRewardManager (login streak rewards)
  Ads/        IAdsProvider abstraction, MockAdsProvider (no SDK needed for
              testing), UnityAdsProvider (real com.unity.ads implementation),
              AdsManager facade
  Analytics/  IAnalyticsProvider abstraction, DebugLogAnalyticsProvider stub
  UI/         HUD (gold/gems/depth/prestige), upgrade panel/rows, shop panel/rows,
              prestige panel, event banner, welcome-back popup, daily-reward popup,
              goblin health bar, leaderboard — all driven by EventBus
Assets/_Project/Editor/
  ProjectBootstrapper.cs   "Gold And Goblins → Bootstrap Starter Scene" menu command
Assets/_Project/ScriptableObjects/
  Blocks/ Upgrades/ Goblins/ IAP/ Events/   -- create your data assets here via
  the Create menu (Assets → Create → Gold And Goblins → ...)
```

Everything talks through `EventBus` (see `Core/EventBus.cs` for the full event
list) rather than direct references between gameplay and UI, so you can reskin
the UI or swap in new gameplay screens without touching manager code.

## Art and the GritLine Toon Shader

KayKit meshes, UI sprites, VFX textures, and the GritLine toon shader already live
under `Assets/_Project/Art/` and `Assets/_Project/Shaders/GritlineToonShader/`.
Editor menu commands wire visual prefabs onto block/goblin data and apply GritLine
materials. Remaining art gap: `character_skeleton_warrior.fbx` has no texture in
the repo, so the goblin material is an untextured tint until you drop the atlas
into `Art/Goblins` and re-run **Wire Up GritLine Materials**.

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

- **Art & shader integration** — KayKit meshes, UI sprites, and GritLine materials
  are imported and wired by the editor tools. Remaining gap: the skeleton goblin
  has no texture atlas in the repo (flat tint until one is dropped in
  `Art/Goblins` and **Wire Up GritLine Materials** is re-run). Particle bursts
  and SFX are still missing; camera shake is in.
- **Scene/prefab polish in-editor** — run the menu commands above on `Main.unity`
  so the existing scene picks up prestige UI, default upgrades, and live events.
  Animations and more juice still need a pass in the Editor.
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

I can help with any of these next — juice (particles/SFX), more goblin types,
store listing copy, or wiring a real analytics/receipt-validation provider.
