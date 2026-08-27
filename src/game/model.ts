export type TileType = 'dirt' | 'stone' | 'gold' | 'gem' | 'chest';

export interface Tile {
  type: TileType;
  maxHp: number;
  hp: number;
  goldReward: number;
  gemReward: number;
  alive: boolean;
}

export interface GoblinTarget {
  /** index into the current grid, or -1 if none assigned */
  tileIndex: number;
  /** seconds until this goblin's next hit lands */
  cooldown: number;
}

export interface GameState {
  gold: number;
  gems: number;
  rubies: number;

  depth: number;
  grid: Tile[];

  totalGoldEarnedThisRun: number;
  totalGoldEarnedAllTime: number;
  maxDepthReached: number;

  pickaxeLevel: number;
  goblinCount: number;
  goblinPowerLevel: number;
  goblinSpeedLevel: number;

  goblinTargets: GoblinTarget[];

  lastGoldPerSecond: number;
  lastSaveTime: number;

  createdAt: number;
}

export const GRID_COLS = 4;
export const GRID_ROWS = 6;
export const GRID_SIZE = GRID_COLS * GRID_ROWS;
