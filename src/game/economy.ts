import type { GameState } from './model';

// --- Upgrade cost curves ---------------------------------------------------

const GOBLIN_BASE_COST = 25;
const GOBLIN_GROWTH = 1.15;

const PICKAXE_BASE_COST = 20;
const PICKAXE_GROWTH = 1.12;

const GOBLIN_POWER_BASE_COST = 30;
const GOBLIN_POWER_GROWTH = 1.13;

const GOBLIN_SPEED_BASE_COST = 45;
const GOBLIN_SPEED_GROWTH = 1.16;
const MAX_GOBLIN_SPEED_LEVEL = 25;

export function goblinCost(goblinCount: number): number {
  return Math.round(GOBLIN_BASE_COST * Math.pow(GOBLIN_GROWTH, goblinCount));
}

export function pickaxeUpgradeCost(level: number): number {
  return Math.round(PICKAXE_BASE_COST * Math.pow(PICKAXE_GROWTH, level));
}

export function goblinPowerUpgradeCost(level: number): number {
  return Math.round(GOBLIN_POWER_BASE_COST * Math.pow(GOBLIN_POWER_GROWTH, level));
}

export function goblinSpeedUpgradeCost(level: number): number {
  return Math.round(GOBLIN_SPEED_BASE_COST * Math.pow(GOBLIN_SPEED_GROWTH, level));
}

export function isGoblinSpeedMaxed(level: number): boolean {
  return level >= MAX_GOBLIN_SPEED_LEVEL;
}

// --- Derived combat stats ---------------------------------------------------

/** Damage dealt by a single tap of the pickaxe. */
export function tapPower(state: GameState): number {
  return 5 + state.pickaxeLevel * 3 * prestigeMultiplier(state);
}

/** Damage dealt by each goblin per hit. */
export function goblinPower(state: GameState): number {
  return 3 + state.goblinPowerLevel * 2 * prestigeMultiplier(state);
}

/** Seconds between goblin hits (lower = faster). */
export function goblinAttackInterval(state: GameState): number {
  const speedLevel = Math.min(state.goblinSpeedLevel, MAX_GOBLIN_SPEED_LEVEL);
  return Math.max(0.4, 2.2 - speedLevel * 0.07);
}

/** Permanent multiplier granted by prestige currency (rubies). */
export function prestigeMultiplier(state: GameState): number {
  return 1 + state.rubies * 0.02;
}

/** Estimated steady-state gold/sec from all goblins combined, used for offline earnings. */
export function estimateGoldPerSecond(state: GameState): number {
  if (state.goblinCount <= 0 || state.grid.length === 0) return 0;
  const avgReward =
    state.grid.reduce((sum, t) => sum + t.goldReward, 0) / state.grid.length;
  const avgHp = state.grid.reduce((sum, t) => sum + t.maxHp, 0) / state.grid.length;
  if (avgHp <= 0) return 0;
  const dps = state.goblinCount * (goblinPower(state) / goblinAttackInterval(state));
  return dps * (avgReward / avgHp);
}

// --- Prestige ---------------------------------------------------------------

export function rubiesForPrestige(state: GameState): number {
  return Math.floor(Math.sqrt(state.totalGoldEarnedThisRun / 500));
}

// --- Tile scaling by depth ---------------------------------------------------

export function depthHpMultiplier(depth: number): number {
  return Math.pow(1.18, depth - 1);
}

export function depthRewardMultiplier(depth: number): number {
  return Math.pow(1.22, depth - 1);
}
