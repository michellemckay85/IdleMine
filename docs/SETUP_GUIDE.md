# Setup Guide: from this repo to a playable build

## 1. Install prerequisites

- **Unity Hub** + **Unity 2022.3 LTS** (any recent 2022.3.x patch is fine;
  `ProjectSettings/ProjectVersion.txt` pins `2022.3.50f1` but Unity Hub will
  offer to switch/download the closest match).
- When installing via Unity Hub, include the **iOS Build Support** and
  **Android Build Support** modules.
- **Mac + Xcode** (latest stable) — required to build/sign/submit the iOS app.
  You cannot produce an App Store build on Windows/Linux.
- **Android Studio** (or just its command-line SDK/NDK/JDK) for the Android
  SDK/build tools Unity's Android module needs.

## 2. Open the project

1. Clone this repo.
2. Unity Hub → *Add* → select the repo's root folder (the one containing
   `Assets/`, `Packages/`, `ProjectSettings/`).
3. Open it. On first open, Unity will resolve packages from
   `Packages/manifest.json`, including **In App Purchasing** and
   **TextMeshPro**. If prompted, import the TMP Essential Resources
   (Window → TextMeshPro → Import TMP Essential Resources).

## 3. Set the bundle identifier / package name

Edit → Project Settings → Player:
- **iOS → Bundle Identifier**: e.g. `com.yourcompany.goldandgoblins`
- **Android → Package Name**: same reverse-DNS string, e.g.
  `com.yourcompany.goldandgoblins`
- Set **Company Name** / **Product Name** to your studio and "Gold and
  Goblins".
- Set an app icon (Player Settings → Icon) for both platforms — required by
  both stores.

This identifier is what ties your build to the App Store Connect / Play
Console app listings — pick it now and don't change it later.

## 4. Build one scene

Create `Assets/Scenes/Main.unity` (File → New Scene → Save As) and add it to
Build Settings (File → Build Settings → Add Open Scenes).

### GameManager object

1. Create an empty GameObject named `GameManager`.
2. Add components: `GameManager`, `CurrencyManager`, `UpgradeSystem`,
   `IdleMineManager`, `GoblinRaidManager`, `AdsGateway`, `IAPManager`.
3. Wire each script's Inspector fields to the sibling components on the same
   object (e.g. `GameManager.currencyManager` → the `CurrencyManager`
   component, `IAPManager.goblinRaidManager` → the `GoblinRaidManager`
   component, etc). Every field name in the scripts matches the component
   type it expects.

### Canvas / UI

1. GameObject → UI → Canvas (this also creates an EventSystem).
2. **Top bar**: two `TextMeshPro - Text` elements for gold/gems. Add a
   `CurrencyDisplayUI` component (anywhere, e.g. on the Canvas) and assign
   `currencyManager`, `goldText`, `gemsText`.
3. **Tap button**: a big `Button` in the middle of the screen. Add
   `MineTapButtonUI`, assign `idleMineManager` and `tapButton`.
4. **Upgrades panel**: one row per upgrade in `UpgradeCatalog`
   (`pickaxe`, `miner_hire`, `mine_cart`, `depth_charge`). For each row add
   an `UpgradeButtonUI` with `upgradeId` set to the matching string, plus
   `upgradeSystem`, `nameText`, `levelText`, `costText`, `buyButton`.
5. **Goblin raid panel**: a `GameObject` (initially inactive) containing a
   filled `Image` (progress bar, Image Type = Filled) and a "Defend!"
   `Button`, plus a result `TMP_Text`. Add `GoblinRaidUI` to it (or a parent)
   and assign all the fields.
6. **Shop panel**: one `ShopItemUI` per row in `IAPProductCatalog.All` (11
   rows — 4 gold packs, 4 gem packs, remove ads, VIP bundle, Goblin Ward
   subscription). Each needs `productId` set to the exact catalog string
   (e.g. `gold_pack_medium`), a title `TMP_Text`, a price `TMP_Text`, and a
   buy `Button`. Add one `ShopUI` component referencing `iapManager` and the
   array of all `ShopItemUI` rows.
7. **Restore Purchases button** (required by Apple): add
   `RestorePurchasesButtonUI` to a button in the shop/settings panel.

You can reuse one row prefab for shop items and upgrade buttons rather than
building 11+4 rows by hand — instantiate copies at runtime, or just duplicate
GameObjects in the Editor. Either is fine; the scripts don't care how the
GameObjects were created.

## 5. Configure Unity IAP

`IAPProductCatalog.All` already declares every product to `UnityPurchasing`
at runtime, so you don't need to duplicate product definitions in the Unity
Dashboard for the app to function. Two things you do need:

- **Real device/sandbox testing** requires the product IDs to exist in App
  Store Connect / Play Console first (see
  `docs/STORE_SUBMISSION_CHECKLIST.md`) — the Unity Editor can fake purchases
  via the "fake store" for quick UI testing, but real purchase flows only
  work once the store-side products exist and the build is signed with a
  matching bundle ID/package name.
- If you want Unity Analytics/Unity Gaming Services dashboards for IAP, link
  the project under Edit → Project Settings → Services with your Unity ID.
  This is optional — not required for IAP to function.

### Quick in-Editor test

Unity IAP falls back to a local "fake store" when running in the Editor, so
you can press Play and click Buy on shop items to confirm the reward
(gold/gems/flags) is applied correctly before ever touching a real store
account.

## 6. Play-test the loop

Press Play and confirm:
- Gold increases from tapping and from idle upgrades.
- Buying an upgrade spends gold and increases gold/sec.
- A goblin raid appears roughly every 3 minutes (tune
  `GameConstants.GoblinRaidIntervalSeconds` for faster testing) and tapping
  Defend enough times repels it; letting the timer run out steals gold.
- Shop purchases (fake store in-editor) grant the right currency/flags.
- Stopping and restarting Play mode preserves progress (it's saved to
  `Application.persistentDataPath`) and grants offline earnings.

## 7. Building

Use the menu items added by `Assets/Editor/BuildScript.cs`:
- **Gold And Goblins → Build → iOS Xcode Project** — outputs an Xcode
  project to `Builds/iOS`. Open it in Xcode to sign and archive.
- **Gold And Goblins → Build → Android App Bundle (.aab)** — outputs
  `Builds/Android/GoldAndGoblins.aab`, ready to upload to Play Console (after
  you've configured a signing keystore under Project Settings → Player →
  Publishing Settings).

From here, follow `docs/STORE_SUBMISSION_CHECKLIST.md` for the store-side
setup and submission.
