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
        [SerializeField] private ShopRowController rowPrefab; // optional legacy
        [SerializeField] private Transform rowContainer;

        private readonly List<ShopRowController> spawnedRows = new List<ShopRowController>();

        private void OnEnable()
        {
            EventBus.Subscribe<PurchaseCompletedEvent>(OnPurchaseCompleted);
            if (Application.isPlaying && catalog != null && spawnedRows.Count == 0)
            {
                BuildRows();
            }
        }

        private void Start() => BuildRows();

        private void OnDisable()
        {
            EventBus.Unsubscribe<PurchaseCompletedEvent>(OnPurchaseCompleted);
        }

        private void BuildRows()
        {
            if (catalog == null)
            {
                Debug.LogWarning("[ShopUI] No ProductCatalog assigned on ShopUIController.");
                return;
            }

            EnsureRowContainer();
            if (rowContainer == null)
            {
                Debug.LogError("[ShopUI] No rowContainer — open Gold And Goblins → Build UI Layout once.");
                return;
            }

            foreach (var row in spawnedRows)
            {
                if (row != null) Destroy(row.gameObject);
            }
            spawnedRows.Clear();

            for (var i = rowContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(rowContainer.GetChild(i).gameObject);
            }

            if (catalog.products == null || catalog.products.Count == 0)
            {
                Debug.LogWarning("[ShopUI] Product catalog is empty.");
                return;
            }

            foreach (var product in catalog.products)
            {
                if (product == null) continue;
                var row = UiRuntimeRowFactory.CreateShopRow(rowContainer, product);
                spawnedRows.Add(row);
            }

            if (rowContainer is RectTransform rt) LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
            Debug.Log($"[ShopUI] Built {spawnedRows.Count} shop rows.");
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

        private void OnPurchaseCompleted(PurchaseCompletedEvent evt) => BuildRows();
    }
}
