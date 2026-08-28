using UnityEngine;
using GoldAndGoblins.Core;

namespace GoldAndGoblins.Gameplay
{
    public class Block : MonoBehaviour
    {
        public BlockDataSO Data { get; private set; }
        public int Row { get; private set; }
        public int Col { get; private set; }
        public bool IsBroken { get; private set; }
        public bool IsLocked { get; set; }

        private float currentHealth;
        private float maxHealth;
        private double goldReward;
        private GameObject spawnedVisual;

        public void Setup(BlockDataSO data, int row, int col, int depth)
        {
            Data = data;
            Row = row;
            Col = col;
            IsBroken = false;

            maxHealth = data.HealthForDepth(depth);
            currentHealth = maxHealth;
            goldReward = data.GoldForDepth(depth);

            if (spawnedVisual != null) Destroy(spawnedVisual);
            if (data.visualPrefab != null)
            {
                spawnedVisual = Instantiate(data.visualPrefab, transform.position, transform.rotation, transform);
            }
        }

        public void ApplyDamage(float damage)
        {
            if (IsBroken || IsLocked) return;

            currentHealth -= damage;
            if (currentHealth <= 0)
            {
                Break();
            }
        }

        private void Break()
        {
            IsBroken = true;
            EventBus.Publish(new BlockBrokenEvent(Row, Col, goldReward));
            Economy.CurrencyManager.Instance.AddGold(goldReward);
            Cleanup();
        }

        // Used by systems (goblin combat, chest-unlock) that grant their own reward
        // and just need the block cleared afterward without double-paying gold.
        public void ForceBreakNoReward()
        {
            if (IsBroken) return;
            IsBroken = true;
            EventBus.Publish(new BlockBrokenEvent(Row, Col, 0));
            Cleanup();
        }

        private void Cleanup()
        {
            if (spawnedVisual != null) Destroy(spawnedVisual);
            gameObject.SetActive(false);
        }

        public float HealthFraction => maxHealth <= 0 ? 0 : Mathf.Clamp01(currentHealth / maxHealth);
    }
}
