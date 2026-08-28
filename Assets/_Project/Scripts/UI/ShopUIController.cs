using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GoldAndGoblins.Core;
using GoldAndGoblins.Economy;

namespace GoldAndGoblins.UI
{
    public class ShopUIController : MonoBehaviour
    {
        [SerializeField] private ProductCatalogSO catalog;
        [SerializeField] private ShopRowController rowPrefab;
        [SerializeField] private Transform rowContainer;

        private readonly List<ShopRowController> spawnedRows = new List<ShopRowController>();

        private void OnEnable()
        {
            EventBus.Subscribe<PurchaseCompletedEvent>(OnPurchaseCompleted);
        }

        // Start, not OnEnable: rows bind against IAPManager.Instance, which is only
        // guaranteed to exist by the time every object's Start has run.
        private void Start() => BuildRows();

        private void OnDisable()
        {
            EventBus.Unsubscribe<PurchaseCompletedEvent>(OnPurchaseCompleted);
        }

        private void BuildRows()
        {
            if (catalog == null || rowPrefab == null || rowContainer == null) return;

            foreach (var row in spawnedRows) Destroy(row.gameObject);
            spawnedRows.Clear();

            foreach (var product in catalog.products)
            {
                var row = Instantiate(rowPrefab, rowContainer);
                row.Bind(product);
                spawnedRows.Add(row);
            }

            // Rows instantiated at runtime under a Layout Group don't always trigger an
            // automatic rebuild -- without this they can render stacked on top of each
            // other instead of stacked vertically.
            if (rowContainer is RectTransform rt) LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
        }

        private void OnPurchaseCompleted(PurchaseCompletedEvent evt) => BuildRows();
    }
}
