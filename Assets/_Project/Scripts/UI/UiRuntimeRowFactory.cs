using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GoldAndGoblins.Gameplay;
using GoldAndGoblins.Economy;

namespace GoldAndGoblins.UI
{
    // Builds readable Upgrade/Shop rows at runtime so we are not stuck with
    // broken stretch-anchored TMP text baked into older scene prefabs.
    public static class UiRuntimeRowFactory
    {
        private static readonly Color TextColor = new Color(0.2f, 0.12f, 0.05f);

        public static UpgradeRowController CreateUpgradeRow(Transform parent, UpgradeDataSO upgrade)
        {
            var row = CreateRowRoot(parent, "UpgradeRow", 130);

            var name = CreateLabel(row.transform, "Name", "Upgrade", 34, 2.2f, 260);
            var level = CreateLabel(row.transform, "Level", "Lv. 0", 30, 0.8f, 100);
            var cost = CreateLabel(row.transform, "Cost", "0", 30, 0.8f, 100);
            var buy = CreateBuyButton(row.transform);

            var controller = row.AddComponent<UpgradeRowController>();
            controller.Wire(name, level, cost, buy);
            controller.Bind(upgrade);
            return controller;
        }

        public static ShopRowController CreateShopRow(Transform parent, IAPProductSO product)
        {
            var row = CreateRowRoot(parent, "ShopRow", 150);

            var textCol = new GameObject("TextColumn", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement));
            textCol.transform.SetParent(row.transform, false);
            var vlg = textCol.GetComponent<VerticalLayoutGroup>();
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.spacing = 4;
            var colLe = textCol.GetComponent<LayoutElement>();
            colLe.flexibleWidth = 2.5f;
            colLe.minWidth = 280;

            var name = CreateLabel(textCol.transform, "Name", "Product", 32, 1f, 200, addLayoutElement: false);
            var desc = CreateLabel(textCol.transform, "Description", "Description", 24, 1f, 200, addLayoutElement: false);
            desc.color = new Color(0.35f, 0.25f, 0.15f);

            var price = CreateLabel(row.transform, "Price", "...", 28, 0.9f, 120);
            var buy = CreateBuyButton(row.transform);

            var controller = row.AddComponent<ShopRowController>();
            controller.Wire(name, desc, price, buy);
            controller.Bind(product);
            return controller;
        }

        private static GameObject CreateRowRoot(Transform parent, string name, float height)
        {
            var row = new GameObject(name, typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            row.transform.SetParent(parent, false);

            var le = row.GetComponent<LayoutElement>();
            le.preferredHeight = height;
            le.minHeight = height;
            le.flexibleWidth = 1;

            var hlg = row.GetComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(16, 16, 8, 8);
            hlg.spacing = 16;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;
            return row;
        }

        private static TMP_Text CreateLabel(Transform parent, string name, string text, float fontSize,
            float flex, float preferredWidth, bool addLayoutElement = true)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = TextColor;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            tmp.enableWordWrapping = false;
            tmp.overflowMode = TextOverflowModes.Ellipsis;

            var rt = tmp.rectTransform;
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 0.5f);
            rt.sizeDelta = new Vector2(preferredWidth, 0);

            if (addLayoutElement)
            {
                var le = go.AddComponent<LayoutElement>();
                le.flexibleWidth = flex;
                le.minWidth = Mathf.Min(80, preferredWidth);
                le.preferredWidth = preferredWidth;
            }

            return tmp;
        }

        private static Button CreateBuyButton(Transform parent)
        {
            var go = new GameObject("BuyButton", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            go.transform.SetParent(parent, false);

            var image = go.GetComponent<Image>();
            image.color = new Color(0.45f, 0.28f, 0.12f);

            var le = go.GetComponent<LayoutElement>();
            le.preferredWidth = 160;
            le.minWidth = 140;
            le.preferredHeight = 90;

            var labelGo = new GameObject("Label", typeof(RectTransform));
            labelGo.transform.SetParent(go.transform, false);
            var label = labelGo.AddComponent<TextMeshProUGUI>();
            label.text = "Buy";
            label.fontSize = 30;
            label.color = Color.white;
            label.alignment = TextAlignmentOptions.Center;
            label.enableWordWrapping = false;
            var lrt = label.rectTransform;
            lrt.anchorMin = Vector2.zero;
            lrt.anchorMax = Vector2.one;
            lrt.offsetMin = Vector2.zero;
            lrt.offsetMax = Vector2.zero;

            return go.GetComponent<Button>();
        }
    }
}
