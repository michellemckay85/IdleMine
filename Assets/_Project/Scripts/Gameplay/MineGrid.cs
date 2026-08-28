using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GoldAndGoblins.Core;
using GoldAndGoblins.Goblins;

namespace GoldAndGoblins.Gameplay
{
    public class MineGrid : GoldAndGoblins.Utils.Singleton<MineGrid>
    {
        [SerializeField] private int rows = 5;
        [SerializeField] private int cols = 4;
        [SerializeField] private float cellSpacing = 1.2f;
        [SerializeField] private Transform gridRoot;
        [SerializeField] private Block blockPrefab;
        [SerializeField] private List<BlockDataSO> blockPalette = new List<BlockDataSO>();
        [SerializeField] private int keysRequiredForChest = 3;
        [SerializeField] private GoblinCombatManager goblinCombatManager;

        private Block[,] grid;
        private int keysCollectedThisDepth;

        public int CurrentDepth => SaveManager.Instance.Current.currentDepth;

        public void Initialize()
        {
            EventBus.Subscribe<BlockBrokenEvent>(OnBlockBroken);
            BuildGridForCurrentDepth();
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<BlockBrokenEvent>(OnBlockBroken);
        }

        private void BuildGridForCurrentDepth()
        {
            keysCollectedThisDepth = 0;
            grid = new Block[rows, cols];

            if (gridRoot == null || blockPrefab == null)
            {
                Debug.LogWarning("[MineGrid] gridRoot/blockPrefab not assigned yet -- wire these up once art prefabs are imported.");
                return;
            }

            foreach (Transform child in gridRoot)
            {
                Destroy(child.gameObject);
            }

            var spawned = 0;
            for (var r = 0; r < rows; r++)
            {
                for (var c = 0; c < cols; c++)
                {
                    var data = PickBlockDataFor(r, c);
                    if (data == null)
                    {
                        Debug.LogWarning($"[MineGrid] No BlockDataSO for cell ({r},{c}) -- is blockPalette empty?");
                        continue;
                    }

                    var instance = Instantiate(blockPrefab, gridRoot);
                    // Center the shaft on (0,0) so a portrait camera aimed at the origin frames it.
                    var x = (c - (cols - 1) * 0.5f) * cellSpacing;
                    var y = -(r - (rows - 1) * 0.5f) * cellSpacing;
                    instance.transform.localPosition = new Vector3(x, y, 0);
                    instance.Setup(data, r, c, CurrentDepth);
                    instance.IsLocked = data.kind == BlockKind.ChestBlock;
                    grid[r, c] = instance;
                    spawned++;
                }
            }

            Debug.Log($"[MineGrid] Built {spawned} blocks at depth {CurrentDepth}.");
        }

        private BlockDataSO PickBlockDataFor(int row, int col)
        {
            var eligible = blockPalette.Where(b => b.minDepthTier <= CurrentDepth && b.kind != BlockKind.ChestBlock && b.kind != BlockKind.KeyBlock).ToList();
            var isLastCell = row == rows - 1 && col == cols - 1;

            if (isLastCell)
            {
                var chest = blockPalette.FirstOrDefault(b => b.kind == BlockKind.ChestBlock);
                if (chest != null) return chest;
            }

            if (row == 0 && (col == 0 || col == cols - 1))
            {
                var key = blockPalette.FirstOrDefault(b => b.kind == BlockKind.KeyBlock);
                if (key != null) return key;
            }

            if (eligible.Count == 0) return blockPalette.FirstOrDefault();
            return eligible[Random.Range(0, eligible.Count)];
        }

        public void TapBlock(int row, int col, float drillDamage)
        {
            if (grid == null || row < 0 || row >= rows || col < 0 || col >= cols) return;
            var block = grid[row, col];
            if (block == null || block.IsBroken) return;

            if (block.Data.kind == BlockKind.GoblinBlock)
            {
                goblinCombatManager?.StartEncounterForBlock(block, drillDamage);
                return;
            }

            if (block.Data.kind == BlockKind.ChestBlock && block.IsLocked)
            {
                return; // needs keysRequiredForChest keys collected first
            }

            block.ApplyDamage(drillDamage);
        }

        public void TapNextAvailableBlock(float drillDamage)
        {
            if (grid == null) return;
            for (var r = 0; r < rows; r++)
            for (var c = 0; c < cols; c++)
            {
                var block = grid[r, c];
                if (block != null && !block.IsBroken && !block.IsLocked && block.Data.kind != BlockKind.GoblinBlock)
                {
                    TapBlock(r, c, drillDamage);
                    return;
                }
            }
        }

        private void OnBlockBroken(BlockBrokenEvent evt)
        {
            var block = grid?[evt.Row, evt.Col];
            if (block != null && block.Data.kind == BlockKind.KeyBlock)
            {
                keysCollectedThisDepth++;
                if (keysCollectedThisDepth >= keysRequiredForChest)
                {
                    UnlockChest();
                }
            }

            if (AllBlocksCleared())
            {
                AdvanceDepth();
            }
        }

        private void UnlockChest()
        {
            for (var r = 0; r < rows; r++)
            for (var c = 0; c < cols; c++)
            {
                if (grid[r, c] != null && grid[r, c].Data.kind == BlockKind.ChestBlock)
                {
                    grid[r, c].IsLocked = false;
                }
            }
        }

        private bool AllBlocksCleared()
        {
            for (var r = 0; r < rows; r++)
            for (var c = 0; c < cols; c++)
            {
                if (grid[r, c] != null && !grid[r, c].IsBroken) return false;
            }
            return true;
        }

        private void AdvanceDepth()
        {
            SaveManager.Instance.Current.currentDepth++;
            EventBus.Publish(new DepthAdvancedEvent(CurrentDepth));
            BuildGridForCurrentDepth();
        }
    }
}
