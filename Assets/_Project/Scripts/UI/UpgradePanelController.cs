using UnityEngine;
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
        }

        // Start, not OnEnable: UpgradeSystem.Instance is only guaranteed to exist
        // by the time every object's Start has run (see HUDController for why).
        private void Start() => BuildRows();

        private void OnDisable()
        {
            EventBus.Unsubscribe<GoldChangedEvent>(OnGoldChanged);
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
        }

        private void OnGoldChanged(GoldChangedEvent evt)
        {
            foreach (var row in spawnedRows) row.Refresh();
        }
    }
}
