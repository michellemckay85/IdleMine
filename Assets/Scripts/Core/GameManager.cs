using System;
using GoldAndGoblins.Ads;
using GoldAndGoblins.Mining;
using UnityEngine;

namespace GoldAndGoblins.Core
{
    /// <summary>
    /// Boots the game: loads the save, initializes CurrencyManager/IdleMineManager/UpgradeSystem
    /// with it, applies offline earnings, and autosaves periodically and on pause/quit.
    /// Attach to a single persistent "GameManager" GameObject in the boot scene, with
    /// CurrencyManager, IdleMineManager, UpgradeSystem and GoblinRaidManager on the same
    /// object (or referenced via the inspector fields below).
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private CurrencyManager currencyManager;
        [SerializeField] private IdleMineManager idleMineManager;
        [SerializeField] private UpgradeSystem upgradeSystem;
        [SerializeField] private GoblinRaidManager goblinRaidManager;
        [SerializeField] private AdsGateway adsGateway;

        [SerializeField] private float autosaveIntervalSeconds = 30f;

        public event Action<double, TimeSpan> OnOfflineEarningsApplied;

        private GameSaveData _saveData;
        private float _autosaveTimer;

        public CurrencyManager Currency => currencyManager;
        public IdleMineManager IdleMine => idleMineManager;
        public UpgradeSystem Upgrades => upgradeSystem;
        public GoblinRaidManager GoblinRaids => goblinRaidManager;
        public AdsGateway Ads => adsGateway;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            _saveData = SaveSystem.Load();

            currencyManager.InitializeFromSave(_saveData);
            upgradeSystem.InitializeFromSave(_saveData);
            idleMineManager.InitializeFromSave(_saveData, upgradeSystem);
            goblinRaidManager.InitializeFromSave(_saveData);
            if (adsGateway != null) adsGateway.InitializeFromSave(_saveData);

            ApplyOfflineEarnings(_saveData);
        }

        private void ApplyOfflineEarnings(GameSaveData data)
        {
            if (string.IsNullOrEmpty(data.lastSaveTimeUtc)) return;
            if (!DateTime.TryParse(data.lastSaveTimeUtc, null,
                    System.Globalization.DateTimeStyles.RoundtripKind, out DateTime lastSave))
            {
                return;
            }

            TimeSpan elapsed = DateTime.UtcNow - lastSave;
            if (elapsed.TotalSeconds <= 1) return;

            float capHours = currencyManager.VipBundlePurchased
                ? GameConstants.VipOfflineCapHours
                : GameConstants.BaseOfflineCapHours;
            TimeSpan capped = elapsed > TimeSpan.FromHours(capHours) ? TimeSpan.FromHours(capHours) : elapsed;

            double goldPerSecond = idleMineManager.CurrentGoldPerSecond * currencyManager.PermanentGoldMultiplier;
            double earned = goldPerSecond * capped.TotalSeconds;
            if (earned <= 0) return;

            currencyManager.AddGoldRaw(earned);
            OnOfflineEarningsApplied?.Invoke(earned, capped);
        }

        private void Update()
        {
            _autosaveTimer += Time.unscaledDeltaTime;
            if (_autosaveTimer >= autosaveIntervalSeconds)
            {
                _autosaveTimer = 0f;
                SaveNow();
            }
        }

        public void SaveNow()
        {
            if (_saveData == null) return;
            currencyManager.ApplyToSave(_saveData);
            upgradeSystem.ApplyToSave(_saveData);
            goblinRaidManager.ApplyToSave(_saveData);
            SaveSystem.Save(_saveData);
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus) SaveNow();
        }

        private void OnApplicationQuit()
        {
            SaveNow();
        }
    }
}
