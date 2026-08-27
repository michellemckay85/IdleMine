using GoldAndGoblins.Core;
using UnityEngine;

namespace GoldAndGoblins.Mining
{
    /// <summary>
    /// Drives the idle income loop (gold/second from upgrades, ticked every frame) and manual
    /// tap-to-mine income. Offline earnings are computed once at boot by GameManager using
    /// CurrentGoldPerSecond, so this class must be initialized before that calculation runs.
    /// </summary>
    public class IdleMineManager : MonoBehaviour
    {
        [SerializeField] private CurrencyManager currencyManager;
        [SerializeField] private double baseTapGold = 1;
        [SerializeField] private double tapGoldPerUpgradeLevel = 0.1;

        private UpgradeSystem _upgradeSystem;

        public double CurrentGoldPerSecond => _upgradeSystem != null ? _upgradeSystem.TotalGoldPerSecond() : 0;

        public void InitializeFromSave(GameSaveData data, UpgradeSystem upgradeSystem)
        {
            _upgradeSystem = upgradeSystem;
        }

        private void Update()
        {
            if (_upgradeSystem == null || currencyManager == null) return;
            double perSecond = CurrentGoldPerSecond;
            if (perSecond > 0)
            {
                currencyManager.AddGold(perSecond * Time.deltaTime);
            }
        }

        /// <summary>Call from a UI "tap the ore" button for active-play income on top of the idle rate.</summary>
        public void Tap()
        {
            if (currencyManager == null) return;
            int totalLevels = 0;
            foreach (UpgradeDefinition def in UpgradeCatalog.All)
            {
                totalLevels += _upgradeSystem != null ? _upgradeSystem.GetLevel(def.id) : 0;
            }
            double amount = baseTapGold + tapGoldPerUpgradeLevel * totalLevels;
            currencyManager.AddGold(amount);
        }
    }
}
