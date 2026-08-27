import type { GameState } from './model';
import { generateGrid } from './grid';

const SAVE_KEY = 'gold-and-goblins-save-v1';
const MAX_OFFLINE_SECONDS = 8 * 60 * 60; // cap offline earnings at 8 hours
const OFFLINE_EFFICIENCY = 0.5; // goblins work at half rate while you're away

export function createNewGame(): GameState {
  return {
    gold: 0,
    gems: 0,
    rubies: 0,
    depth: 1,
    grid: generateGrid(1),
    totalGoldEarnedThisRun: 0,
    totalGoldEarnedAllTime: 0,
    maxDepthReached: 1,
    pickaxeLevel: 0,
    goblinCount: 0,
    goblinPowerLevel: 0,
    goblinSpeedLevel: 0,
    goblinTargets: [],
    lastGoldPerSecond: 0,
    lastSaveTime: Date.now(),
    createdAt: Date.now(),
  };
}

export function saveGame(state: GameState): void {
  state.lastSaveTime = Date.now();
  try {
    localStorage.setItem(SAVE_KEY, JSON.stringify(state));
  } catch {
    // storage unavailable (private mode, quota) - fail silently, keep playing
  }
}

export interface LoadResult {
  state: GameState;
  offlineSeconds: number;
  offlineGold: number;
}

export function loadGame(): LoadResult {
  let raw: string | null = null;
  try {
    raw = localStorage.getItem(SAVE_KEY);
  } catch {
    raw = null;
  }

  if (!raw) {
    return { state: createNewGame(), offlineSeconds: 0, offlineGold: 0 };
  }

  try {
    const state: GameState = JSON.parse(raw);
    const elapsedMs = Date.now() - state.lastSaveTime;
    const offlineSeconds = Math.max(0, Math.min(elapsedMs / 1000, MAX_OFFLINE_SECONDS));
    const offlineGold = Math.round(
      offlineSeconds * state.lastGoldPerSecond * OFFLINE_EFFICIENCY
    );
    if (offlineGold > 0) {
      state.gold += offlineGold;
      state.totalGoldEarnedThisRun += offlineGold;
      state.totalGoldEarnedAllTime += offlineGold;
    }
    return { state, offlineSeconds, offlineGold };
  } catch {
    return { state: createNewGame(), offlineSeconds: 0, offlineGold: 0 };
  }
}

export function resetSave(): void {
  try {
    localStorage.removeItem(SAVE_KEY);
  } catch {
    // ignore
  }
}
