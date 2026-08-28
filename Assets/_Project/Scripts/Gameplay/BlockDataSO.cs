using UnityEngine;

namespace GoldAndGoblins.Gameplay
{
    public enum BlockKind
    {
        Dirt,
        Stone,
        OreCommon,
        OreRare,
        KeyBlock,
        ChestBlock,
        GoblinBlock
    }

    [CreateAssetMenu(fileName = "Block_", menuName = "Gold And Goblins/Block")]
    public class BlockDataSO : ScriptableObject
    {
        public string blockId;
        public BlockKind kind;

        public float baseHealth = 10;
        public double baseGoldReward = 5;

        [Tooltip("Minimum mine depth at which this block type can appear.")]
        public int minDepthTier = 1;

        [Tooltip("Assign your art prefab here once imported (mesh + GritLine toon shader material).")]
        public GameObject visualPrefab;

        public float HealthForDepth(int depth) => baseHealth * Mathf.Pow(1.12f, Mathf.Max(0, depth - minDepthTier));
        public double GoldForDepth(int depth) => baseGoldReward * Mathf.Pow(1.12f, Mathf.Max(0, depth - minDepthTier));
    }
}
