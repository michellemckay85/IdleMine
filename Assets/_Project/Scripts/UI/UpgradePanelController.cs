using UnityEngine;
using UnityEngine.UI;
using GoldAndGoblins.Core;
using GoldAndGoblins.Gameplay;

namespace GoldAndGoblins.UI
{
    public class UpgradePanelController : MonoBehaviour
    {
        [SerializeField] private UpgradeRowController rowPrefab;
        [SerializeField] private Transform rowContainer;

        private readonly System.Collections.Generic.List<UpgradeRowController> spawnedRows = new System.Collections.Generic.List<UpgradeRowController>();

        private void OnEnable()
        {
            EventBus.Subscribe<GoldChangedEvent>(OnGoldChanged);
            EventBus.Subscribe<PrestigeEvent>(OnPrestige);
            EventBus.Subscribe<UpgradePurchasedEvent>(OnUpgradePurchased);
        }

        // Start, not OnEnable: UpgradeSystem.Instance is only guaranteed to exist
        // by the time every object's Start has run (see HUDController for why).
        private void Start() => BuildRows();

        private void OnDisable()
        {
            EventBus.Unsubscribe<GoldChangedEvent>(OnGoldChanged);
            EventBus.Unsubscribe<PrestigeEvent>(OnPrestige);
            EventBus.Unsubscribe<UpgradePurchasedEvent>(OnUpgradePurchased);
        }

        private void BuildRows()
        {
            if (rowPrefab == null || rowContainer == null || UpgradeSystem.Instance == null) return;

            foreach (var row in spawnedRows) Destroy(row.gameObject);
            spawnedRows.Clear();

            foreach (var upgrade in UpgradeSystem.Instance.AllUpgrades)
            {
                var row = Instantiate(rowPrefab, rowContainer);
                row.Bind(upgrade);
                spawnedRows.Add(row);
            }

            // Rows instantiated at runtime under a Layout Group don't always trigger an
            // automatic rebuild -- without this they can render stacked on top of each
            // other instead of stacked vertically.
            if (rowContainer is RectTransform rt) LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
        }

        private void OnGoldChanged(GoldChangedEvent evt)
        {
            foreach (var row in spawnedRows) row.Refresh();
        }

        private void OnPrestige(PrestigeEvent evt) => BuildRows();
        private void OnUpgradePurchased(UpgradePurchasedEvent evt)
        {
            foreach (var row in spawnedRows) row.Refresh();
        }
    }
}
