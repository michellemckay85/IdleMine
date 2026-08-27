using System.Collections.Generic;
using UnityEngine;

namespace GoldAndGoblins.Economy
{
    [CreateAssetMenu(fileName = "ProductCatalog", menuName = "Gold And Goblins/Product Catalog")]
    public class ProductCatalogSO : ScriptableObject
    {
        public List<IAPProductSO> products = new List<IAPProductSO>();
    }
}
