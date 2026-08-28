using System.IO;
using UnityEditor;
using UnityEngine;
using GoldAndGoblins.Gameplay;
using GoldAndGoblins.Goblins;

namespace GoldAndGoblins.EditorTools
{
    // Turns the imported meshes under Assets/_Project/Art/{Blocks,Goblins,Environment,Characters}
    // into visual-only prefabs (mesh + renderer, no collider -- Block already provides one) under
    // Art/Prefabs, and assigns the block/goblin ones to BlockDataSO / GoblinDataSO assets. Still
    // manual afterward: build a base Block prefab (Block component + Collider), wire the
    // palettes on MineGrid/GoblinCombatManager, hand-place the environment prefabs, and assign
    // the Art/UI sprites to the HUD/shop/popup Image components -- see the log message below.
    public static class ArtImportSetup
    {
        private const string BlocksArtPath = "Assets/_Project/Art/Blocks";
        private const string GoblinsArtPath = "Assets/_Project/Art/Goblins";
        private const string EnvironmentArtPath = "Assets/_Project/Art/Environment";
        private const string CharactersArtPath = "Assets/_Project/Art/Characters";
        private const string VisualPrefabPath = "Assets/_Project/Art/Prefabs";
        private const string BlockDataPath = "Assets/_Project/ScriptableObjects/Blocks";
        private const string GoblinDataPath = "Assets/_Project/ScriptableObjects/Goblins";

        private struct BlockSpec
        {
            public string id;
            public BlockKind kind;
            public string meshAssetPath;
            public float baseHealth;
            public double baseGoldReward;
            public int minDepthTier;
        }

        [MenuItem("Gold And Goblins/Wire Up Imported Art (Blocks + Goblin)")]
        public static void WireUpArt()
        {
            Directory.CreateDirectory(VisualPrefabPath);
            Directory.CreateDirectory(BlockDataPath);
            Directory.CreateDirectory(GoblinDataPath);

            var goblinMeshPath = $"{GoblinsArtPath}/character_skeleton_warrior.fbx";

            var blocks = new[]
            {
                new BlockSpec { id = "dirt", kind = BlockKind.Dirt, meshAssetPath = $"{BlocksArtPath}/dirt.fbx", baseHealth = 8, baseGoldReward = 3, minDepthTier = 1 },
                new BlockSpec { id = "stone", kind = BlockKind.Stone, meshAssetPath = $"{BlocksArtPath}/stone.fbx", baseHealth = 14, baseGoldReward = 5, minDepthTier = 1 },
                new BlockSpec { id = "ore_common", kind = BlockKind.OreCommon, meshAssetPath = $"{BlocksArtPath}/stone_with_copper.fbx", baseHealth = 20, baseGoldReward = 12, minDepthTier = 1 },
                new BlockSpec { id = "ore_rare", kind = BlockKind.OreRare, meshAssetPath = $"{BlocksArtPath}/stone_with_gold.fbx", baseHealth = 30, baseGoldReward = 30, minDepthTier = 3 },
                new BlockSpec { id = "key", kind = BlockKind.KeyBlock, meshAssetPath = $"{BlocksArtPath}/key.fbx", baseHealth = 10, baseGoldReward = 0, minDepthTier = 1 },
                new BlockSpec { id = "chest", kind = BlockKind.ChestBlock, meshAssetPath = $"{BlocksArtPath}/chest_gold.fbx", baseHealth = 40, baseGoldReward = 100, minDepthTier = 1 },
                new BlockSpec { id = "goblin", kind = BlockKind.GoblinBlock, meshAssetPath = goblinMeshPath, baseHealth = 15, baseGoldReward = 0, minDepthTier = 1 },
            };

            foreach (var spec in blocks)
            {
                var visualPrefab = BuildVisualPrefab(spec.meshAssetPath, spec.id);

                var so = LoadOrCreate<BlockDataSO>($"{BlockDataPath}/Block_{spec.id}.asset");
                so.blockId = spec.id;
                so.kind = spec.kind;
                so.baseHealth = spec.baseHealth;
                so.baseGoldReward = spec.baseGoldReward;
                so.minDepthTier = spec.minDepthTier;
                so.visualPrefab = visualPrefab;
                EditorUtility.SetDirty(so);
            }

            var goblinVisual = BuildVisualPrefab(goblinMeshPath, "goblin_skeleton");
            var goblinSo = LoadOrCreate<GoblinDataSO>($"{GoblinDataPath}/Goblin_Skeleton.asset");
            goblinSo.goblinId = "skeleton_grunt";
            goblinSo.displayName = "Skeleton Grunt";
            goblinSo.visualPrefab = goblinVisual;
            goblinSo.minDepthTier = 1;
            goblinSo.baseHealth = 50;
            goblinSo.goldLootMin = 20;
            goblinSo.goldLootMax = 60;
            goblinSo.gemDropChance = 0.1f;
            goblinSo.gemLootMin = 1;
            goblinSo.gemLootMax = 5;
            EditorUtility.SetDirty(goblinSo);

            // Environment dressing and the miner character have no data-driven slot in code --
            // just stage them as prefabs so they're ready to hand-place in the scene.
            BuildVisualPrefab($"{EnvironmentArtPath}/wall_doorway.fbx", "env_wall_doorway");
            BuildVisualPrefab($"{EnvironmentArtPath}/floor_dirt_large_rocky.fbx", "env_floor_dirt");
            BuildVisualPrefab($"{EnvironmentArtPath}/torch_lit.fbx", "env_torch_lit");
            BuildVisualPrefab($"{CharactersArtPath}/Barbarian.fbx", "character_miner");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[ArtImportSetup] Built visual prefabs under Art/Prefabs (blocks, goblin, environment dressing, miner character) " +
                      "and BlockDataSO/GoblinDataSO assets under ScriptableObjects/. Still needed: a base Block prefab (Block component + " +
                      "Collider) assigned to MineGrid.blockPrefab, these BlockDataSO assets dropped into MineGrid.blockPalette / " +
                      "GoblinCombatManager.goblinPalette, environment prefabs hand-placed around the grid, and UI sprites from Art/UI " +
                      "assigned to the HUD/shop/popup/health-bar Image components in the Inspector.");
        }

        private static GameObject BuildVisualPrefab(string meshAssetPath, string id)
        {
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(meshAssetPath);
            if (source == null)
            {
                Debug.LogWarning($"[ArtImportSetup] Mesh not found at '{meshAssetPath}' -- skipping visual prefab for '{id}'.");
                return null;
            }

            var prefabPath = $"{VisualPrefabPath}/Visual_{id}.prefab";
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(source);
            var prefab = PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
            Object.DestroyImmediate(instance);
            return prefab;
        }

        private static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            var existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null) return existing;

            var asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }
    }
}
