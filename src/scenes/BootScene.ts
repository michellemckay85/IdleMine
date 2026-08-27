import Phaser from 'phaser';
import { TILE_COLORS } from '../game/grid';

const TILE_W = 150;
const TILE_H = 108;

export class BootScene extends Phaser.Scene {
  constructor() {
    super('boot');
  }

  create(): void {
    this.makeTileSlot();
    for (const type of Object.keys(TILE_COLORS) as (keyof typeof TILE_COLORS)[]) {
      this.makeTileFace(type, TILE_COLORS[type]);
    }
    this.makeGoblin();
    this.makeCoin();
    this.makeGem(0xff5c8a, 'ruby-icon');
    this.makeGem(0x56ccf2, 'gem-icon');
    this.makePickaxe();
    this.makePanel();
    this.makeButton();
    this.makeParticle();

    this.scene.start('mine');
  }

  private makeTileSlot(): void {
    const g = this.add.graphics();
    g.fillStyle(0x2a1f18, 1);
    g.fillRoundedRect(0, 0, TILE_W, TILE_H, 14);
    g.lineStyle(3, 0x140d09, 1);
    g.strokeRoundedRect(1.5, 1.5, TILE_W - 3, TILE_H - 3, 14);
    g.generateTexture('tile-slot', TILE_W, TILE_H);
    g.destroy();
  }

  private makeTileFace(type: string, color: number): void {
    const g = this.add.graphics();
    g.fillStyle(color, 1);
    g.fillRoundedRect(0, 0, TILE_W, TILE_H, 14);
    g.fillStyle(0xffffff, 0.14);
    g.fillRoundedRect(6, 6, TILE_W - 12, TILE_H / 2 - 8, 10);
    g.lineStyle(3, 0x000000, 0.25);
    g.strokeRoundedRect(1.5, 1.5, TILE_W - 3, TILE_H - 3, 14);
    if (type === 'chest') {
      g.fillStyle(0x6b3a12, 1);
      g.fillRoundedRect(TILE_W / 2 - 30, TILE_H / 2 - 4, 60, 30, 4);
      g.fillStyle(0xf2c94c, 1);
      g.fillRect(TILE_W / 2 - 30, TILE_H / 2 - 4, 60, 6);
    }
    g.generateTexture(`tile-${type}`, TILE_W, TILE_H);
    g.destroy();
  }

  private makeGoblin(): void {
    const g = this.add.graphics();
    const cx = 30;
    const cy = 34;
    g.fillStyle(0x4c9a4c, 1);
    g.fillEllipse(cx, cy, 46, 40);
    g.fillStyle(0x3c7d3c, 1);
    g.fillTriangle(cx - 22, cy - 10, cx - 32, cy - 30, cx - 10, cy - 16);
    g.fillTriangle(cx + 22, cy - 10, cx + 32, cy - 30, cx + 10, cy - 16);
    g.fillStyle(0xffffff, 1);
    g.fillCircle(cx - 9, cy - 2, 6);
    g.fillCircle(cx + 9, cy - 2, 6);
    g.fillStyle(0x1a1a1a, 1);
    g.fillCircle(cx - 9, cy - 2, 3);
    g.fillCircle(cx + 9, cy - 2, 3);
    g.generateTexture('goblin', 60, 60);
    g.destroy();
  }

  private makeCoin(): void {
    const g = this.add.graphics();
    g.fillStyle(0xf2c94c, 1);
    g.fillCircle(16, 16, 16);
    g.lineStyle(2, 0xc99a1e, 1);
    g.strokeCircle(16, 16, 14);
    g.fillStyle(0xc99a1e, 1);
    g.fillCircle(16, 16, 5);
    g.generateTexture('coin', 32, 32);
    g.destroy();
  }

  private makeGem(color: number, key: string): void {
    const g = this.add.graphics();
    g.fillStyle(color, 1);
    g.fillTriangle(16, 2, 30, 14, 16, 30);
    g.fillTriangle(16, 2, 2, 14, 16, 30);
    g.fillStyle(0xffffff, 0.35);
    g.fillTriangle(16, 2, 22, 14, 16, 18);
    g.generateTexture(key, 32, 32);
    g.destroy();
  }

  private makePickaxe(): void {
    const g = this.add.graphics();
    g.lineStyle(6, 0x8a5a34, 1);
    g.lineBetween(6, 30, 26, 10);
    g.fillStyle(0xb0b0b0, 1);
    g.fillTriangle(18, 2, 32, 10, 20, 20);
    g.fillTriangle(18, 2, 4, 10, 16, 20);
    g.generateTexture('pickaxe-icon', 36, 36);
    g.destroy();
  }

  private makePanel(): void {
    const g = this.add.graphics();
    g.fillStyle(0x241a12, 0.92);
    g.fillRoundedRect(0, 0, 200, 100, 16);
    g.lineStyle(2, 0x000000, 0.3);
    g.strokeRoundedRect(1, 1, 198, 98, 16);
    g.generateTexture('panel-bg', 200, 100);
    g.destroy();
  }

  private makeButton(): void {
    const g = this.add.graphics();
    g.fillStyle(0x3a6b3a, 1);
    g.fillRoundedRect(0, 0, 320, 110, 16);
    g.fillStyle(0xffffff, 0.12);
    g.fillRoundedRect(4, 4, 312, 46, 12);
    g.lineStyle(3, 0x1f3d1f, 1);
    g.strokeRoundedRect(1.5, 1.5, 317, 107, 16);
    g.generateTexture('button-green', 320, 110);
    g.destroy();

    const g2 = this.add.graphics();
    g2.fillStyle(0x555555, 1);
    g2.fillRoundedRect(0, 0, 320, 110, 16);
    g2.lineStyle(3, 0x333333, 1);
    g2.strokeRoundedRect(1.5, 1.5, 317, 107, 16);
    g2.generateTexture('button-disabled', 320, 110);
    g2.destroy();

    const g3 = this.add.graphics();
    g3.fillStyle(0x8a3aa0, 1);
    g3.fillRoundedRect(0, 0, 320, 100, 16);
    g3.fillStyle(0xffffff, 0.12);
    g3.fillRoundedRect(4, 4, 312, 42, 12);
    g3.lineStyle(3, 0x4d1f5c, 1);
    g3.strokeRoundedRect(1.5, 1.5, 317, 97, 16);
    g3.generateTexture('button-purple', 320, 100);
    g3.destroy();
  }

  private makeParticle(): void {
    const g = this.add.graphics();
    g.fillStyle(0xffffff, 1);
    g.fillCircle(4, 4, 4);
    g.generateTexture('particle', 8, 8);
    g.destroy();
  }
}
