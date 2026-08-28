using UnityEngine;

namespace GoldAndGoblins.Goblins
{
    [CreateAssetMenu(fileName = "Goblin_", menuName = "Gold And Goblins/Goblin")]
    public class GoblinDataSO : ScriptableObject
    {
        public string goblinId;
        public string displayName;
        public GameObject visualPrefab;

        public int minDepthTier = 1;
        public float baseHealth = 50;
        public double goldLootMin = 20;
        public double goldLootMax = 60;

        [Range(0f, 1f)] public float gemDropChance = 0.1f;
        public long gemLootMin = 1;
        public long gemLootMax = 5;

        public float HealthForDepth(int depth) => baseHealth * Mathf.Pow(1.15f, Mathf.Max(0, depth - 1));
    }
}
