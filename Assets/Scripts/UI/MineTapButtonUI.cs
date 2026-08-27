using GoldAndGoblins.Mining;
using UnityEngine;
using UnityEngine.UI;

namespace GoldAndGoblins.UI
{
    /// <summary>Wires the big "tap the ore" button to IdleMineManager.Tap().</summary>
    public class MineTapButtonUI : MonoBehaviour
    {
        [SerializeField] private IdleMineManager idleMineManager;
        [SerializeField] private Button tapButton;

        private void Awake()
        {
            if (tapButton != null) tapButton.onClick.AddListener(() => idleMineManager.Tap());
        }
    }
}
