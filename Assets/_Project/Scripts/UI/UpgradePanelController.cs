using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GoldAndGoblins.Core;
using GoldAndGoblins.Gameplay;

namespace GoldAndGoblins.UI
{
    public class UpgradePanelController : MonoBehaviour
    {
        [SerializeField] private UpgradeRowController rowPrefab; // optional legacy; ignored if present but broken
        [SerializeField] private Transform rowContainer;

        private readonly System.Collections.Generic.List<UpgradeRowController> spawnedRows =
            new System.Collections.Generic.List<UpgradeRowController>();

        private void OnEnable()
        {
            EventBus.Subscribe<GoldChangedEvent>(OnGoldChanged);
            // Rebuild when opening the panel so a late UpgradeSystem still populates rows.
            if (Application.isPlaying && UpgradeSystem.Instance != null && spawnedRows.Count == 0)
            {
                BuildRows();
            }
        }

        private void Start() => BuildRows();

        private void OnDisable()
        {
            EventBus.Unsubscribe<GoldChangedEvent>(OnGoldChanged);
        }

        private void BuildRows()
        {
            if (UpgradeSystem.Instance == null)
            {
                Debug.LogWarning("[UpgradePanel] UpgradeSystem not ready yet.");
                return;
            }

            EnsureRowContainer();
            if (rowContainer == null)
            {
                Debug.LogError("[UpgradePanel] No rowContainer — open Gold And Goblins → Build UI Layout once.");
                return;
            }

            foreach (var row in spawnedRows)
            {
                if (row != null) Destroy(row.gameObject);
            }
            spawnedRows.Clear();

            // Clear leftover broken children from older prefab instantiations.
            for (var i = rowContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(rowContainer.GetChild(i).gameObject);
            }

            var upgrades = UpgradeSystem.Instance.AllUpgrades;
            if (upgrades == null || upgrades.Count == 0)
            {
                Debug.LogWarning("[UpgradePanel] No upgrades in UpgradeSystem — run Create Default Game Data.");
                return;
            }

            foreach (var upgrade in upgrades)
            {
                if (upgrade == null) continue;
                var row = UiRuntimeRowFactory.CreateUpgradeRow(rowContainer, upgrade);
                spawnedRows.Add(row);
            }

            if (rowContainer is RectTransform rt) LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
            Debug.Log($"[UpgradePanel] Built {spawnedRows.Count} upgrade rows.");
        }

        private void EnsureRowContainer()
        {
            if (rowContainer != null) return;

            var scroll = GetComponentInChildren<ScrollRect>(true);
            if (scroll != null && scroll.content != null)
            {
                rowContainer = scroll.content;
                return;
            }

            var vlg = GetComponentInChildren<VerticalLayoutGroup>(true);
            if (vlg != null) rowContainer = vlg.transform;
        }

        private void OnGoldChanged(GoldChangedEvent evt)
        {
            foreach (var row in spawnedRows) row.Refresh();
        }
    }
}
