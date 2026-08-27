import Phaser from 'phaser';
import { GAME_WIDTH, GAME_HEIGHT } from '../config';
import { GRID_COLS, GRID_ROWS } from '../game/model';
import type { Tile } from '../game/model';
import {
  controller,
  initialOfflineGold,
  initialOfflineSeconds,
  startAutosave,
} from '../game/instance';
import {
  goblinCost,
  pickaxeUpgradeCost,
  goblinPowerUpgradeCost,
  goblinSpeedUpgradeCost,
  isGoblinSpeedMaxed,
  rubiesForPrestige,
  tapPower,
  goblinPower,
} from '../game/economy';
import { formatNumber, formatDuration } from '../game/format';

const TILE_W = 150;
const TILE_H = 108;
const TILE_GAP = 10;
const GRID_TOP = 118;
const GRID_LEFT = (GAME_WIDTH - (GRID_COLS * TILE_W + (GRID_COLS - 1) * TILE_GAP)) / 2;

interface TileVisual {
  container: Phaser.GameObjects.Container;
  face: Phaser.GameObjects.Image;
  hpBg: Phaser.GameObjects.Rectangle;
  hpFill: Phaser.GameObjects.Rectangle;
}

interface UpgradeButton {
  container: Phaser.GameObjects.Container;
  bg: Phaser.GameObjects.Image;
  title: Phaser.GameObjects.Text;
  sub: Phaser.GameObjects.Text;
  costFn: () => number;
  isMaxed: () => boolean;
  buy: () => boolean;
}

export class MineScene extends Phaser.Scene {
  private tiles: TileVisual[] = [];
  private goldText!: Phaser.GameObjects.Text;
  private gemsText!: Phaser.GameObjects.Text;
  private rubiesText!: Phaser.GameObjects.Text;
  private depthText!: Phaser.GameObjects.Text;
  private gpsText!: Phaser.GameObjects.Text;
  private goblinCampText!: Phaser.GameObjects.Text;
  private goblinIcons: Phaser.GameObjects.Image[] = [];
  private upgradeButtons: UpgradeButton[] = [];
  private prestigeContainer!: Phaser.GameObjects.Container;
  private prestigeBg!: Phaser.GameObjects.Image;
  private prestigeLabel!: Phaser.GameObjects.Text;

  constructor() {
    super('mine');
  }

  create(): void {
    this.cameras.main.setBackgroundColor('#1a1410');

    this.buildTopBar();
    this.buildGrid();
    this.buildGoblinCamp();
    this.buildUpgradePanel();
    this.buildPrestigeButton();

    controller.events.on('tileHit', ({ index, tile }) => this.onTileHit(index, tile));
    controller.events.on('tileDestroyed', ({ index, tile, gold, gems }) =>
      this.onTileDestroyed(index, tile, gold, gems)
    );
    controller.events.on('goblinHit', ({ tileIndex }) => this.pulseTile(tileIndex));
    controller.events.on('depthChanged', () => this.rebuildGrid());

    this.refreshAll();
    startAutosave();

    if (initialOfflineGold > 0) {
      this.showOfflinePopup(initialOfflineGold, initialOfflineSeconds);
    }
  }

  update(_time: number, deltaMs: number): void {
    controller.update(deltaMs / 1000);
    this.refreshHud();
    this.refreshButtons();
  }

  // --- Top bar -----------------------------------------------------------

  private buildTopBar(): void {
    const bar = this.add.rectangle(0, 0, GAME_WIDTH, 96, 0x120d09, 0.95).setOrigin(0, 0);
    bar.setStrokeStyle(2, 0x000000, 0.4);

    this.add.image(28, 34, 'coin').setScale(1.1);
    this.goldText = this.add
      .text(52, 24, '0', { fontFamily: 'sans-serif', fontSize: '26px', color: '#ffe9a8' })
      .setOrigin(0, 0);

    this.add.image(28, 68, 'gem-icon').setScale(0.9);
    this.gemsText = this.add
      .text(48, 58, '0', { fontFamily: 'sans-serif', fontSize: '20px', color: '#9be8ff' })
      .setOrigin(0, 0);

    this.add.image(200, 68, 'ruby-icon').setScale(0.9);
    this.rubiesText = this.add
      .text(220, 58, '0', { fontFamily: 'sans-serif', fontSize: '20px', color: '#ffb3c6' })
      .setOrigin(0, 0);

    this.depthText = this.add
      .text(GAME_WIDTH - 20, 24, 'Depth 1', {
        fontFamily: 'sans-serif',
        fontSize: '24px',
        color: '#ffffff',
      })
      .setOrigin(1, 0);

    this.gpsText = this.add
      .text(GAME_WIDTH - 20, 58, '', {
        fontFamily: 'sans-serif',
        fontSize: '17px',
        color: '#8a8a8a',
      })
      .setOrigin(1, 0);
  }

  // --- Grid ----------------------------------------------------------------

  private buildGrid(): void {
    for (let i = 0; i < GRID_COLS * GRID_ROWS; i++) {
      const col = i % GRID_COLS;
      const row = Math.floor(i / GRID_COLS);
      const x = GRID_LEFT + col * (TILE_W + TILE_GAP);
      const y = GRID_TOP + row * (TILE_H + TILE_GAP);

      const container = this.add.container(x, y);
      const slot = this.add.image(0, 0, 'tile-slot').setOrigin(0, 0);
      const face = this.add.image(0, 0, 'tile-dirt').setOrigin(0, 0);
      const hpBg = this.add
        .rectangle(8, TILE_H - 14, TILE_W - 16, 8, 0x000000, 0.5)
        .setOrigin(0, 0);
      const hpFill = this.add
        .rectangle(8, TILE_H - 14, TILE_W - 16, 8, 0x6fdc6f, 1)
        .setOrigin(0, 0);
      container.add([slot, face, hpBg, hpFill]);
      container.setSize(TILE_W, TILE_H);
      container.setInteractive(
        new Phaser.Geom.Rectangle(0, 0, TILE_W, TILE_H),
        Phaser.Geom.Rectangle.Contains
      );
      container.on('pointerdown', () => controller.tapTile(i));

      this.tiles.push({ container, face, hpBg, hpFill });
    }
    this.rebuildGrid();
  }

  private rebuildGrid(): void {
    const grid = controller.state.grid;
    for (let i = 0; i < this.tiles.length; i++) {
      const tile = grid[i];
      const visual = this.tiles[i];
      visual.face.setTexture(`tile-${tile.type}`);
      visual.face.setVisible(tile.alive);
      visual.face.setAlpha(1);
      visual.face.setScale(1);
      visual.hpBg.setVisible(tile.alive);
      visual.hpFill.setVisible(tile.alive);
      visual.hpFill.width = TILE_W - 16;
    }
  }

  private onTileHit(index: number, tile: Tile): void {
    const visual = this.tiles[index];
    if (!visual) return;
    const ratio = Phaser.Math.Clamp(tile.hp / tile.maxHp, 0, 1);
    visual.hpFill.width = (TILE_W - 16) * ratio;
    this.pulseTile(index);
  }

  private pulseTile(index: number): void {
    const visual = this.tiles[index];
    if (!visual) return;
    this.tweens.add({
      targets: visual.face,
      scaleX: 0.9,
      scaleY: 0.9,
      duration: 60,
      yoyo: true,
      ease: 'Quad.easeOut',
    });
  }

  private onTileDestroyed(index: number, _tile: Tile, gold: number, gems: number): void {
    const visual = this.tiles[index];
    if (!visual) return;
    const x = GRID_LEFT + (index % GRID_COLS) * (TILE_W + TILE_GAP) + TILE_W / 2;
    const y = GRID_TOP + Math.floor(index / GRID_COLS) * (TILE_H + TILE_GAP) + TILE_H / 2;

    this.tweens.add({
      targets: visual.face,
      alpha: 0,
      scaleX: 0.6,
      scaleY: 0.6,
      duration: 180,
      onComplete: () => {
        visual.face.setVisible(false);
        visual.hpBg.setVisible(false);
        visual.hpFill.setVisible(false);
      },
    });

    this.spawnBurst(x, y);
    this.spawnFloatText(x, y, `+${formatNumber(gold)}`, '#ffe9a8');
    if (gems > 0) {
      this.spawnFloatText(x, y + 26, `+${gems} gem${gems > 1 ? 's' : ''}`, '#9be8ff');
    }
  }

  private spawnBurst(x: number, y: number): void {
    const particles = this.add.particles(x, y, 'particle', {
      speed: { min: 60, max: 160 },
      angle: { min: 0, max: 360 },
      scale: { start: 1, end: 0 },
      lifespan: 350,
      quantity: 10,
      tint: [0xf2c94c, 0xffffff],
    });
    this.time.delayedCall(400, () => particles.destroy());
  }

  private spawnFloatText(x: number, y: number, text: string, color: string): void {
    const t = this.add
      .text(x, y, text, {
        fontFamily: 'sans-serif',
        fontSize: '22px',
        color,
        fontStyle: 'bold',
      })
      .setOrigin(0.5);
    this.tweens.add({
      targets: t,
      y: y - 50,
      alpha: 0,
      duration: 700,
      ease: 'Quad.easeOut',
      onComplete: () => t.destroy(),
    });
  }

  // --- Goblin camp -----------------------------------------------------------

  private buildGoblinCamp(): void {
    const y = GRID_TOP + GRID_ROWS * (TILE_H + TILE_GAP) + 6;
    this.add
      .rectangle(GRID_LEFT, y, GRID_COLS * TILE_W + (GRID_COLS - 1) * TILE_GAP, 70, 0x140d09, 0.5)
      .setOrigin(0, 0)
      .setStrokeStyle(2, 0x000000, 0.3);

    this.goblinCampText = this.add
      .text(GRID_LEFT + 10, y + 8, 'Goblins: 0', {
        fontFamily: 'sans-serif',
        fontSize: '18px',
        color: '#c9e8c9',
      })
      .setOrigin(0, 0);

    for (let i = 0; i < 12; i++) {
      const icon = this.add
        .image(GRID_LEFT + 14 + i * 32, y + 46, 'goblin')
        .setScale(0.5)
        .setVisible(false);
      this.goblinIcons.push(icon);
    }
  }

  private refreshGoblinCamp(): void {
    const count = controller.state.goblinCount;
    this.goblinCampText.setText(`Goblins: ${count}`);
    const shown = Math.min(count, this.goblinIcons.length);
    for (let i = 0; i < this.goblinIcons.length; i++) {
      this.goblinIcons[i].setVisible(i < shown);
    }
  }

  // --- Upgrade panel -----------------------------------------------------------

  private buildUpgradePanel(): void {
    const panelTop = GRID_TOP + GRID_ROWS * (TILE_H + TILE_GAP) + 86;
    const btnW = 336;
    const btnH = 110;
    const gap = 14;
    const startX = (GAME_WIDTH - (2 * btnW + gap)) / 2;

    const specs: Omit<UpgradeButton, 'container' | 'bg' | 'title' | 'sub'>[] = [
      {
        costFn: () => pickaxeUpgradeCost(controller.state.pickaxeLevel),
        isMaxed: () => false,
        buy: () => controller.upgradePickaxe(),
      },
      {
        costFn: () => goblinCost(controller.state.goblinCount),
        isMaxed: () => false,
        buy: () => controller.buyGoblin(),
      },
      {
        costFn: () => goblinPowerUpgradeCost(controller.state.goblinPowerLevel),
        isMaxed: () => false,
        buy: () => controller.upgradeGoblinPower(),
      },
      {
        costFn: () => goblinSpeedUpgradeCost(controller.state.goblinSpeedLevel),
        isMaxed: () => isGoblinSpeedMaxed(controller.state.goblinSpeedLevel),
        buy: () => controller.upgradeGoblinSpeed(),
      },
    ];
    const icons = ['pickaxe-icon', 'goblin', 'goblin', 'goblin'];
    const names = ['Pickaxe', 'Hire Goblin', 'Goblin Power', 'Goblin Speed'];

    specs.forEach((spec, i) => {
      const col = i % 2;
      const row = Math.floor(i / 2);
      const x = startX + col * (btnW + gap) + btnW / 2;
      const y = panelTop + row * (btnH + gap) + btnH / 2;

      const container = this.add.container(x, y);
      const bg = this.add.image(0, 0, 'button-green');
      const icon = this.add.image(-btnW / 2 + 34, -8, icons[i]).setScale(0.55);
      const title = this.add
        .text(-btnW / 2 + 60, -30, names[i], {
          fontFamily: 'sans-serif',
          fontSize: '19px',
          color: '#ffffff',
          fontStyle: 'bold',
        })
        .setOrigin(0, 0);
      const sub = this.add
        .text(-btnW / 2 + 60, -2, '', {
          fontFamily: 'sans-serif',
          fontSize: '16px',
          color: '#e6ffe6',
        })
        .setOrigin(0, 0);

      container.add([bg, icon, title, sub]);
      container.setSize(btnW, btnH);
      container.setInteractive({ useHandCursor: true });
      container.on('pointerdown', () => {
        if (!spec.buy()) this.shake(container);
      });

      this.upgradeButtons.push({ container, bg, title, sub, ...spec });
    });
  }

  private refreshButtons(): void {
    const state = controller.state;
    const infos = [
      `Lv.${state.pickaxeLevel} · +${tapPower(state).toFixed(0)} dmg/tap`,
      `${state.goblinCount} hired`,
      `Lv.${state.goblinPowerLevel} · ${goblinPower(state).toFixed(0)} dmg/hit`,
      `Lv.${state.goblinSpeedLevel}/25`,
    ];
    this.upgradeButtons.forEach((btn, i) => {
      if (btn.isMaxed()) {
        btn.sub.setText(`${infos[i]} · MAXED`);
        btn.bg.setTexture('button-disabled');
        return;
      }
      const cost = btn.costFn();
      const affordable = state.gold >= cost;
      btn.sub.setText(`${infos[i]}\nCost: ${formatNumber(cost)}`);
      btn.bg.setTexture(affordable ? 'button-green' : 'button-disabled');
    });
  }

  // --- Prestige -----------------------------------------------------------

  private buildPrestigeButton(): void {
    const panelTop = GRID_TOP + GRID_ROWS * (TILE_H + TILE_GAP) + 86 + 2 * 110 + 14 + 20;
    const y = panelTop + 45;
    this.prestigeContainer = this.add.container(GAME_WIDTH / 2, y);
    this.prestigeBg = this.add.image(0, 0, 'button-purple');
    this.prestigeLabel = this.add
      .text(0, 0, 'Return to Surface', {
        fontFamily: 'sans-serif',
        fontSize: '20px',
        color: '#ffffff',
        fontStyle: 'bold',
        align: 'center',
      })
      .setOrigin(0.5);
    this.prestigeContainer.add([this.prestigeBg, this.prestigeLabel]);
    this.prestigeContainer.setSize(320, 100);
    this.prestigeContainer.setInteractive({ useHandCursor: true });
    this.prestigeContainer.on('pointerdown', () => {
      if (!controller.canPrestige()) {
        this.shake(this.prestigeContainer);
        return;
      }
      const gained = controller.prestige();
      this.rebuildGrid();
      this.showToast(`Prestiged! +${gained} Rubies`);
    });
  }

  private refreshPrestige(): void {
    const gain = rubiesForPrestige(controller.state);
    this.prestigeLabel.setText(
      gain > 0 ? `Return to Surface\n+${gain} Rubies` : 'Return to Surface\n(keep mining)'
    );
    this.prestigeBg.setTexture(gain > 0 ? 'button-purple' : 'button-disabled');
  }

  // --- HUD -----------------------------------------------------------

  private refreshHud(): void {
    const state = controller.state;
    this.goldText.setText(formatNumber(state.gold));
    this.gemsText.setText(formatNumber(state.gems));
    this.rubiesText.setText(formatNumber(state.rubies));
    this.depthText.setText(`Depth ${state.depth}`);
    this.gpsText.setText(`${formatNumber(state.lastGoldPerSecond)} gold/s`);
    this.refreshGoblinCamp();
    this.refreshPrestige();
  }

  private refreshAll(): void {
    this.rebuildGrid();
    this.refreshHud();
    this.refreshButtons();
  }

  // --- Helpers -----------------------------------------------------------

  private shake(target: Phaser.GameObjects.Container): void {
    this.tweens.add({
      targets: target,
      x: target.x + 8,
      duration: 40,
      yoyo: true,
      repeat: 3,
    });
  }

  private showToast(message: string): void {
    const t = this.add
      .text(GAME_WIDTH / 2, GAME_HEIGHT / 2, message, {
        fontFamily: 'sans-serif',
        fontSize: '28px',
        color: '#ffffff',
        backgroundColor: '#000000cc',
        padding: { x: 20, y: 14 },
        align: 'center',
      })
      .setOrigin(0.5)
      .setDepth(1000);
    this.tweens.add({
      targets: t,
      alpha: 0,
      delay: 1200,
      duration: 500,
      onComplete: () => t.destroy(),
    });
  }

  private showOfflinePopup(gold: number, seconds: number): void {
    const overlay = this.add
      .rectangle(0, 0, GAME_WIDTH, GAME_HEIGHT, 0x000000, 0.7)
      .setOrigin(0, 0)
      .setDepth(2000)
      .setInteractive();
    const box = this.add
      .rectangle(GAME_WIDTH / 2, GAME_HEIGHT / 2, 560, 320, 0x241a12, 1)
      .setStrokeStyle(3, 0xf2c94c)
      .setDepth(2001);
    const title = this.add
      .text(GAME_WIDTH / 2, GAME_HEIGHT / 2 - 110, 'Welcome back!', {
        fontFamily: 'sans-serif',
        fontSize: '30px',
        color: '#f2c94c',
        fontStyle: 'bold',
      })
      .setOrigin(0.5)
      .setDepth(2002);
    const body = this.add
      .text(
        GAME_WIDTH / 2,
        GAME_HEIGHT / 2 - 30,
        `Your goblins kept digging while you\nwere away for ${formatDuration(seconds)}.\n\n+${formatNumber(
          gold
        )} gold`,
        {
          fontFamily: 'sans-serif',
          fontSize: '22px',
          color: '#ffffff',
          align: 'center',
        }
      )
      .setOrigin(0.5)
      .setDepth(2002);
    const btn = this.add
      .text(GAME_WIDTH / 2, GAME_HEIGHT / 2 + 110, 'Collect', {
        fontFamily: 'sans-serif',
        fontSize: '24px',
        color: '#1a1410',
        backgroundColor: '#f2c94c',
        padding: { x: 30, y: 12 },
      })
      .setOrigin(0.5)
      .setDepth(2002)
      .setInteractive({ useHandCursor: true });

    const closeAll = () => {
      overlay.destroy();
      box.destroy();
      title.destroy();
      body.destroy();
      btn.destroy();
    };
    overlay.on('pointerdown', closeAll);
    btn.on('pointerdown', closeAll);
  }
}
