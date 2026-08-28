using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GoldAndGoblins.Core;
using GoldAndGoblins.Economy;
using GoldAndGoblins.Gameplay;

namespace GoldAndGoblins.Goblins
{
    // A goblin block turns tapping into a mini combat: taps deal drill damage to the
    // goblin's health bar instead of instantly breaking the block. Defeat grants its
    // own loot roll, then the block clears without paying out block gold too.
    public class GoblinCombatManager : GoldAndGoblins.Utils.Singleton<GoblinCombatManager>
    {
        [SerializeField] private List<GoblinDataSO> goblinPalette = new List<GoblinDataSO>();

        private Block currentBlock;
        private GoblinDataSO currentGoblin;
        private float currentHealth;
        private float currentMaxHealth;

        public bool IsEncounterActiveFor(Block block) => currentBlock == block && currentHealth > 0;

        public void StartEncounterForBlock(Block block, float initialDamage)
        {
            if (IsEncounterActiveFor(block))
            {
                ApplyDamage(initialDamage);
                return;
            }

            currentGoblin = PickGoblinForDepth(MineGrid.Instance.CurrentDepth);
            if (currentGoblin == null) return;

            currentBlock = block;
            currentMaxHealth = currentGoblin.HealthForDepth(MineGrid.Instance.CurrentDepth);
            currentHealth = currentMaxHealth;

            EventBus.Publish(new GoblinEncounterStartedEvent(currentGoblin.goblinId, currentMaxHealth));
            ApplyDamage(initialDamage);
        }

        public void ApplyDamage(float damage)
        {
            if (currentBlock == null || currentHealth <= 0) return;

            currentHealth = Mathf.Max(0, currentHealth - damage);
            EventBus.Publish(new GoblinHealthChangedEvent(currentHealth, currentMaxHealth));

            if (currentHealth <= 0)
            {
                ResolveDefeat();
            }
        }

        private void ResolveDefeat()
        {
            var gold = Random.Range((float)currentGoblin.goldLootMin, (float)currentGoblin.goldLootMax);
            CurrencyManager.Instance.AddGold(gold);

            long gems = 0;
            if (Random.value < currentGoblin.gemDropChance)
            {
                gems = Random.Range(currentGoblin.gemLootMin, currentGoblin.gemLootMax + 1);
                CurrencyManager.Instance.AddGems(gems);
            }

            EventBus.Publish(new GoblinDefeatedEvent(currentGoblin.goblinId, gold, gems));

            currentBlock.ForceBreakNoReward();
            currentBlock = null;
            currentGoblin = null;
        }

        private GoblinDataSO PickGoblinForDepth(int depth)
        {
            var eligible = goblinPalette.Where(g => g.minDepthTier <= depth).ToList();
            if (eligible.Count == 0) return goblinPalette.FirstOrDefault();
            return eligible[Random.Range(0, eligible.Count)];
        }
    }
}
