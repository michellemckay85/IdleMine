# Gold and Goblins

An idle mining game — dig through layers of rock for gold and gems, hire
goblin miners who dig for you automatically, upgrade your pickaxe, and
prestige for permanent bonuses.

Built with [Phaser 3](https://phaser.io/) + TypeScript + Vite, wrapped with
[Capacitor](https://capacitorjs.com/) for iOS and Android.

## Gameplay

- Tap tiles to mine them with your pickaxe.
- Hire goblins to mine automatically, even while you're away (idle/offline
  earnings).
- Deeper layers have tougher rock and better rewards.
- Upgrade pickaxe power, goblin power, and goblin speed with gold.
- Find gems in rare tiles; find chests for a big payout.
- "Return to Surface" to prestige: convert your run's earnings into Rubies,
  a permanent gold multiplier, and start over deeper and stronger.

## Development

```bash
npm install
npm run dev       # start the web dev server at http://localhost:5173
npm run build     # type-check and build the web app to dist/
```

## Mobile builds (iOS / Android)

The native Android and iOS projects are already scaffolded under `android/`
and `ios/`. After changing game code, rebuild the web app and sync it into
both native projects:

```bash
npm run build
npx cap sync
```

### Android (Google Play)

Requires [Android Studio](https://developer.android.com/studio) with the
Android SDK installed.

```bash
npx cap open android
```

This opens the project in Android Studio, where you can run it on an
emulator/device and build a signed `.aab` for the Play Store via
**Build > Generate Signed Bundle**.

### iOS (Apple App Store)

Requires a Mac with [Xcode](https://developer.apple.com/xcode/) and
[CocoaPods](https://cocoapods.org/) installed.

```bash
cd ios/App && pod install && cd ../..
npx cap open ios
```

This opens the project in Xcode, where you can run it on the simulator/a
device and archive a build for App Store submission via
**Product > Archive**. You'll need an Apple Developer account, a bundle ID
matching `capacitor.config.ts` (`com.goldandgoblins.app` — change this to
your own before publishing), and app icons/screenshots for the store
listing.

## Project structure

```
src/
  config.ts            game canvas size constants
  main.ts               Phaser game bootstrap
  game/
    model.ts            core types (GameState, Tile, grid dimensions)
    grid.ts              tile generation & weighted loot table
    economy.ts           upgrade cost curves & derived combat stats
    controller.ts        game loop: tapping, goblin AI, rewards, prestige
    save.ts               localStorage persistence + offline earnings calc
    instance.ts           singleton controller + autosave wiring
    format.ts              number/duration formatting helpers
    events.ts               tiny typed event emitter
  scenes/
    BootScene.ts          procedurally generates all textures/icons
    MineScene.ts           renders the grid, HUD, upgrade buttons, prestige
```

No external art assets are used — all sprites (tiles, goblins, coins, gems,
buttons) are drawn procedurally at boot time with Phaser's Graphics API, so
there's nothing to license or replace before shipping (though swapping in
real art is easy: just point the relevant `Image` calls at new texture keys
loaded from files instead).
