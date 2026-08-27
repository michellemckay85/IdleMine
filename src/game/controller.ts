import type { GameState, Tile } from './model';
import { GRID_SIZE } from './model';
import { generateGrid } from './grid';
import { Emitter } from './events';
import {
  tapPower,
  goblinPower,
  goblinAttackInterval,
  prestigeMultiplier,
  estimateGoldPerSecond,
  goblinCost,
  pickaxeUpgradeCost,
  goblinPowerUpgradeCost,
  goblinSpeedUpgradeCost,
  isGoblinSpeedMaxed,
  rubiesForPrestige,
} from './economy';
import { createNewGame, saveGame } from './save';

export interface TileHitPayload {
  index: number;
  tile: Tile;
  damage: number;
  source: 'tap' | 'goblin';
}

export interface TileDestroyedPayload {
  index: number;
  tile: Tile;
  gold: number;
  gems: number;
  source: 'tap' | 'goblin';
}

export interface GoblinHitPayload {
  goblinIndex: number;
  tileIndex: number;
}

export interface ControllerEvents extends Record<string, unknown> {
  tileHit: TileHitPayload;
  tileDestroyed: TileDestroyedPayload;
  goblinHit: GoblinHitPayload;
  depthChanged: { depth: number };
  stateChanged: undefined;
  prestiged: { rubiesGained: number };
}

const RECALC_GPS_INTERVAL = 2; // seconds

export class GameController {
  state: GameState;
  events = new Emitter<ControllerEvents>();
  private gpsTimer = 0;

  constructor(state: GameState) {
    this.state = state;
    this.syncGoblinTargets();
  }

  private syncGoblinTargets(): void {
    const targets = this.state.goblinTargets;
    while (targets.length < this.state.goblinCount) {
      targets.push({ tileIndex: -1, cooldown: goblinAttackInterval(this.state) });
    }
    while (targets.length > this.state.goblinCount) {
      targets.pop();
    }
  }

  private pickAliveTileIndex(): number {
    const alive: number[] = [];
    for (let i = 0; i < this.state.grid.length; i++) {
      if (this.state.grid[i].alive) alive.push(i);
    }
    if (alive.length === 0) return -1;
    return alive[Math.floor(Math.random() * alive.length)];
  }

  private awardTileRewards(tile: Tile): { gold: number; gems: number } {
    const gold = Math.round(tile.goldReward * prestigeMultiplier(this.state));
    const gems = tile.gemReward;
    this.state.gold += gold;
    this.state.gems += gems;
    this.state.totalGoldEarnedThisRun += gold;
    this.state.totalGoldEarnedAllTime += gold;
    return { gold, gems };
  }

  private destroyTile(index: number, source: 'tap' | 'goblin'): void {
    const tile = this.state.grid[index];
    tile.alive = false;
    tile.hp = 0;
    const { gold, gems } = this.awardTileRewards(tile);
    this.events.emit('tileDestroyed', { index, tile, gold, gems, source });

    if (this.state.grid.every((t) => !t.alive)) {
      this.advanceDepth();
    }
  }

  private advanceDepth(): void {
    this.state.depth += 1;
    this.state.maxDepthReached = Math.max(this.state.maxDepthReached, this.state.depth);
    this.state.grid = generateGrid(this.state.depth);
    for (const t of this.state.goblinTargets) t.tileIndex = -1;
    this.events.emit('depthChanged', { depth: this.state.depth });
  }

  tapTile(index: number): void {
    const tile = this.state.grid[index];
    if (!tile || !tile.alive) return;
    const damage = tapPower(this.state);
    tile.hp -= damage;
    this.events.emit('tileHit', { index, tile, damage, source: 'tap' });
    if (tile.hp <= 0) {
      this.destroyTile(index, 'tap');
    }
  }

  update(deltaSeconds: number): void {
    this.syncGoblinTargets();
    const interval = goblinAttackInterval(this.state);
    const power = goblinPower(this.state);

    for (let g = 0; g < this.state.goblinTargets.length; g++) {
      const target = this.state.goblinTargets[g];
      if (target.tileIndex === -1 || !this.state.grid[target.tileIndex]?.alive) {
        target.tileIndex = this.pickAliveTileIndex();
        target.cooldown = interval;
        if (target.tileIndex === -1) continue;
      }

      target.cooldown -= deltaSeconds;
      if (target.cooldown <= 0) {
        const tile = this.state.grid[target.tileIndex];
        if (tile && tile.alive) {
          tile.hp -= power;
          this.events.emit('goblinHit', { goblinIndex: g, tileIndex: target.tileIndex });
          if (tile.hp <= 0) {
            this.destroyTile(target.tileIndex, 'goblin');
          }
        }
        target.tileIndex = this.pickAliveTileIndex();
        target.cooldown = interval;
      }
    }

    this.gpsTimer += deltaSeconds;
    if (this.gpsTimer >= RECALC_GPS_INTERVAL) {
      this.gpsTimer = 0;
      this.state.lastGoldPerSecond = estimateGoldPerSecond(this.state);
    }
  }

  // --- Upgrades -------------------------------------------------------------

  buyGoblin(): boolean {
    const cost = goblinCost(this.state.goblinCount);
    if (this.state.gold < cost) return false;
    this.state.gold -= cost;
    this.state.goblinCount += 1;
    this.syncGoblinTargets();
    this.events.emit('stateChanged', undefined);
    return true;
  }

  upgradePickaxe(): boolean {
    const cost = pickaxeUpgradeCost(this.state.pickaxeLevel);
    if (this.state.gold < cost) return false;
    this.state.gold -= cost;
    this.state.pickaxeLevel += 1;
    this.events.emit('stateChanged', undefined);
    return true;
  }

  upgradeGoblinPower(): boolean {
    const cost = goblinPowerUpgradeCost(this.state.goblinPowerLevel);
    if (this.state.gold < cost) return false;
    this.state.gold -= cost;
    this.state.goblinPowerLevel += 1;
    this.events.emit('stateChanged', undefined);
    return true;
  }

  upgradeGoblinSpeed(): boolean {
    if (isGoblinSpeedMaxed(this.state.goblinSpeedLevel)) return false;
    const cost = goblinSpeedUpgradeCost(this.state.goblinSpeedLevel);
    if (this.state.gold < cost) return false;
    this.state.gold -= cost;
    this.state.goblinSpeedLevel += 1;
    this.events.emit('stateChanged', undefined);
    return true;
  }

  // --- Prestige ---------------------------------------------------------------

  canPrestige(): boolean {
    return rubiesForPrestige(this.state) > 0;
  }

  prestige(): number {
    const rubiesGained = rubiesForPrestige(this.state);
    if (rubiesGained <= 0) return 0;

    const keepRubies = this.state.rubies + rubiesGained;
    const keepAllTime = this.state.totalGoldEarnedAllTime;
    const keepMaxDepth = this.state.maxDepthReached;
    const createdAt = this.state.createdAt;

    this.state = createNewGame();
    this.state.rubies = keepRubies;
    this.state.totalGoldEarnedAllTime = keepAllTime;
    this.state.maxDepthReached = keepMaxDepth;
    this.state.createdAt = createdAt;

    this.events.emit('prestiged', { rubiesGained });
    this.events.emit('depthChanged', { depth: this.state.depth });
    this.events.emit('stateChanged', undefined);
    return rubiesGained;
  }

  save(): void {
    saveGame(this.state);
  }
}

export { GRID_SIZE };
