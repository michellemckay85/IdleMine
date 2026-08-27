import { GRID_SIZE } from './model';
import type { Tile, TileType } from './model';
import { depthHpMultiplier, depthRewardMultiplier } from './economy';

interface TileSpec {
  type: TileType;
  weight: number;
  baseHp: number;
  baseGold: number;
  baseGems: number;
}

const TILE_SPECS: TileSpec[] = [
  { type: 'dirt', weight: 55, baseHp: 12, baseGold: 3, baseGems: 0 },
  { type: 'stone', weight: 27, baseHp: 28, baseGold: 7, baseGems: 0 },
  { type: 'gold', weight: 13, baseHp: 45, baseGold: 22, baseGems: 0 },
  { type: 'gem', weight: 4, baseHp: 70, baseGold: 30, baseGems: 1 },
  { type: 'chest', weight: 1, baseHp: 90, baseGold: 120, baseGems: 3 },
];

const TOTAL_WEIGHT = TILE_SPECS.reduce((sum, s) => sum + s.weight, 0);

function pickTileSpec(): TileSpec {
  let roll = Math.random() * TOTAL_WEIGHT;
  for (const spec of TILE_SPECS) {
    if (roll < spec.weight) return spec;
    roll -= spec.weight;
  }
  return TILE_SPECS[0];
}

export function generateGrid(depth: number): Tile[] {
  const hpMult = depthHpMultiplier(depth);
  const rewardMult = depthRewardMultiplier(depth);
  const tiles: Tile[] = [];
  for (let i = 0; i < GRID_SIZE; i++) {
    const spec = pickTileSpec();
    const maxHp = Math.round(spec.baseHp * hpMult);
    tiles.push({
      type: spec.type,
      maxHp,
      hp: maxHp,
      goldReward: Math.round(spec.baseGold * rewardMult),
      gemReward: spec.baseGems,
      alive: true,
    });
  }
  return tiles;
}

export const TILE_COLORS: Record<TileType, number> = {
  dirt: 0x8a5a34,
  stone: 0x7d7d7d,
  gold: 0xf2c94c,
  gem: 0x56ccf2,
  chest: 0xb968f0,
};
