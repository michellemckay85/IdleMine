using System.Collections.Generic;
using UnityEngine;
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
        }

        private void OnPurchaseCompleted(PurchaseCompletedEvent evt) => BuildRows();
    }
}
