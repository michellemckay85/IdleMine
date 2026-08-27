import { GameController } from './controller';
import { loadGame } from './save';

const { state, offlineGold, offlineSeconds } = loadGame();

export const controller = new GameController(state);
export const initialOfflineGold = offlineGold;
export const initialOfflineSeconds = offlineSeconds;

let autosaveHandle: ReturnType<typeof setInterval> | null = null;

export function startAutosave(intervalMs = 10_000): void {
  if (autosaveHandle) return;
  autosaveHandle = setInterval(() => controller.save(), intervalMs);
  document.addEventListener('visibilitychange', () => {
    if (document.visibilityState === 'hidden') controller.save();
  });
  window.addEventListener('pagehide', () => controller.save());
}
