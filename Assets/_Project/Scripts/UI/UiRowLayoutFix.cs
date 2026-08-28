using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GoldAndGoblins.UI
{
    // Fixes existing Upgrade/Shop rows that were built with stretch-anchored TMP text.
    // Those collapse to ~0 width under a HorizontalLayoutGroup and wrap one letter per line.
    public static class UiRowLayoutFix
    {
        public static void FixRow(Transform rowRoot)
        {
            if (rowRoot == null) return;

            var hlg = rowRoot.GetComponent<HorizontalLayoutGroup>();
            if (hlg != null)
            {
                hlg.childControlWidth = true;
                hlg.childControlHeight = true;
                hlg.childForceExpandWidth = false;
                hlg.childForceExpandHeight = true;
            }

            foreach (var tmp in rowRoot.GetComponentsInChildren<TMP_Text>(true))
            {
                FixText(tmp);
            }

            if (rowRoot is RectTransform rt)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
            }
        }

        public static void FixText(TMP_Text tmp)
        {
            if (tmp == null) return;

            tmp.enableWordWrapping = false;
            tmp.overflowMode = TextOverflowModes.Ellipsis;

            var rt = tmp.rectTransform;
            // Leave button labels alone — they should fill their button.
            if (tmp.GetComponentInParent<Button>() != null && tmp.transform.parent != null &&
                tmp.transform.parent.GetComponent<Button>() != null)
            {
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                tmp.overflowMode = TextOverflowModes.Overflow;
                return;
            }

            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 0.5f);

            var le = tmp.GetComponent<LayoutElement>();
            if (le == null) le = tmp.gameObject.AddComponent<LayoutElement>();
            if (le.minWidth < 80) le.minWidth = 80;
            if (le.preferredWidth < 120) le.preferredWidth = 200;
            if (le.flexibleWidth < 0.1f) le.flexibleWidth = 1f;
        }
    }
}
