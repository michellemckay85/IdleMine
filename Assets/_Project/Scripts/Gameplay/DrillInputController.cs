using UnityEngine;

namespace GoldAndGoblins.Gameplay
{
    // Handles tap/click input on block colliders and routes damage through MineGrid.
    // Auto-mining (from the AutoMinerSpeed upgrade) ticks independently below.
    public class DrillInputController : MonoBehaviour
    {
        [SerializeField] private Camera worldCamera;
        [SerializeField] private LayerMask blockLayerMask = ~0;

        private float autoMineAccumulator;

        private void Reset()
        {
            worldCamera = Camera.main;
        }

        private void Update()
        {
            HandleTapInput();
            HandleAutoMining();
        }

        private void HandleTapInput()
        {
            var tapped = false;
            Vector3 screenPos = default;

            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                tapped = true;
                screenPos = Input.GetTouch(0).position;
            }
            else if (Input.GetMouseButtonDown(0))
            {
                tapped = true;
                screenPos = Input.mousePosition;
            }

            if (!tapped || worldCamera == null) return;
            if (UpgradeSystem.Instance == null || MineGrid.Instance == null) return;

            var ray = worldCamera.ScreenPointToRay(screenPos);
            if (Physics.Raycast(ray, out var hit, 100f, blockLayerMask))
            {
                var block = hit.collider.GetComponentInParent<Block>();
                if (block != null)
                {
                    var drillPower = (float)UpgradeSystem.Instance.GetCurrentValue(UpgradeType.DrillPower);
                    var isCrit = Random.value < UpgradeSystem.Instance.GetCurrentValue(UpgradeType.CriticalChance);
                    var damage = isCrit ? drillPower * 3f : drillPower;
                    MineGrid.Instance.TapBlock(block.Row, block.Col, damage);
                }
            }
        }

        private void HandleAutoMining()
        {
            // Managers may not be ready on the first frames (or after a domain-reload quirk).
            // Never spam NullReferenceException from a missing singleton.
            if (UpgradeSystem.Instance == null || MineGrid.Instance == null) return;

            var autoSpeed = UpgradeSystem.Instance.GetCurrentValue(UpgradeType.AutoMinerSpeed);
            if (autoSpeed <= 0) return;

            autoMineAccumulator += Time.deltaTime * (float)autoSpeed;
            if (autoMineAccumulator < 1f) return;
            autoMineAccumulator = 0f;

            var drillPower = (float)UpgradeSystem.Instance.GetCurrentValue(UpgradeType.DrillPower);
            if (drillPower <= 0) drillPower = 1f;
            MineGrid.Instance.TapNextAvailableBlock(drillPower);
        }
    }
}
