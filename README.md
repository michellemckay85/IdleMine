# Gold and Goblins

An idle/incremental mining game: tap and hire miners to dig gold, upgrade your
mine, and defend against periodic goblin raids that try to steal your hoard.
Built for Unity, targeting iOS (App Store) and Android (Google Play), with
in-app purchases for currency packs, an ad-removal unlock, a VIP starter
bundle, and a "Goblin Ward" subscription.

This repository contains the game's C# source (no Unity Editor-generated
scene/prefab files — see "Why no scenes?" below). To get a running, testable
build you'll open this in the Unity Editor and wire up a UI scene once,
following `docs/SETUP_GUIDE.md`.

## What's here

```
Assets/Scripts/Core/    GameManager, SaveSystem, CurrencyManager, save data model
Assets/Scripts/Mining/  Idle gold/sec loop, upgrade tree, goblin raid mechanic
Assets/Scripts/IAP/     Unity IAP integration + the product catalog
Assets/Scripts/Ads/     Ad-gating stub (respects the "remove ads" purchase)
Assets/Scripts/UI/      MonoBehaviours that bind the above to uGUI/TextMeshPro
Assets/Editor/          Menu commands to build the iOS Xcode project / Android .aab
Packages/manifest.json  Unity IAP + TextMeshPro package dependencies
docs/SETUP_GUIDE.md               Open-in-Unity, scene wiring, IAP dashboard config
docs/STORE_SUBMISSION_CHECKLIST.md App Store Connect + Google Play Console steps
```

## Core game loop

- **Idle mining**: gold/second scales with purchased upgrades (`UpgradeCatalog`
  in `Mining/UpgradeDefinition.cs`); tapping the ore also grants a manual
  bonus. Offline time is credited on load, capped at 12h (24h for VIP owners).
- **Goblin raids**: every few minutes a raid threatens to steal a slice of
  your banked gold unless you tap fast enough to defend. "Goblin Ward"
  subscribers auto-repel raids for a small bonus instead.
- **Upgrades**: Sharper Pickaxe, Hire a Miner, Reinforced Mine Cart, Mine
  Depth Charge — each with exponentially scaling gold cost.

## In-app purchases (Unity IAP)

Defined once in `Assets/Scripts/IAP/IAPProductCatalog.cs` and used to build
both the store configuration and the shop UI, so the IDs only need to be
typed in one place in code:

| Product ID            | Type          | Grants                                   |
|------------------------|---------------|-------------------------------------------|
| `gold_pack_small`      | Consumable    | 1,000 gold |
| `gold_pack_medium`     | Consumable    | 6,000 gold |
| `gold_pack_large`      | Consumable    | 14,000 gold |
| `gold_pack_mega`       | Consumable    | 32,000 gold |
| `gem_pack_small`       | Consumable    | 50 gems |
| `gem_pack_medium`      | Consumable    | 300 gems |
| `gem_pack_large`       | Consumable    | 700 gems |
| `gem_pack_mega`        | Consumable    | 1,600 gems |
| `remove_ads`           | Non-consumable| Disables ad gateway calls |
| `starter_vip_bundle`   | Non-consumable| 5,000 gold + 100 gems + permanent 1.25x gold multiplier + doubled offline cap |
| `goblin_ward_monthly`  | Subscription  | Auto-repels goblin raids, grants a small bonus each raid |

These same IDs must be created identically in App Store Connect and the
Google Play Console — see `docs/STORE_SUBMISSION_CHECKLIST.md`.

## Why no scenes/prefabs?

Unity's `.unity` scene and `.prefab` files are YAML with GUID references that
the Editor generates and validates; hand-writing them outside the Editor
risks producing files Unity can't open cleanly. Instead every MonoBehaviour
here is designed to be dropped onto GameObjects and wired via the Inspector
in a few minutes — `docs/SETUP_GUIDE.md` walks through exactly which objects
and references to create.

## Limitations of this repository

Nobody can *finish* a store submission from source code alone — actually
publishing "Gold and Goblins" requires steps only you can do as the account
holder:

- An active Apple Developer Program membership ($99/yr) and Google Play
  Console account ($25 one-time).
- Creating the app listings, screenshots, and store copy in App Store Connect
  and Play Console.
- Creating the IAP products/subscription in both consoles with the IDs above.
- Code signing (Apple certificates/provisioning profiles via Xcode; an
  Android upload keystore).
- Submitting the build for review.

Everything up to "press submit" — the game itself and the IAP wiring — is
covered here and in the docs.
