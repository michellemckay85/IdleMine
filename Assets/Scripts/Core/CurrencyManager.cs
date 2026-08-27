using System;
using UnityEngine;

namespace GoldAndGoblins.Core
{
    /// <summary>
    /// Owns the player's Gold (soft currency) and Gems (premium currency) in memory,
    /// backed by the GameSaveData handed to it at startup. Fires events so UI can react
    /// without polling.
    /// </summary>
    public class CurrencyManager : MonoBehaviour
    {
        public static CurrencyManager Instance { get; private set; }

        public event Action<double> OnGoldChanged;
        public event Action<long> OnGemsChanged;

        public double Gold { get; private set; }
        public long Gems { get; private set; }

        public float PermanentGoldMultiplier { get; private set; } = 1f;
        public bool RemoveAdsPurchased { get; private set; }
        public bool VipBundlePurchased { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void InitializeFromSave(GameSaveData data)
        {
            Gold = data.gold;
            Gems = data.gems;
            PermanentGoldMultiplier = data.permanentGoldMultiplier <= 0f ? 1f : data.permanentGoldMultiplier;
            RemoveAdsPurchased = data.removeAdsPurchased;
            VipBundlePurchased = data.vipBundlePurchased;

            OnGoldChanged?.Invoke(Gold);
            OnGemsChanged?.Invoke(Gems);
        }

        public void ApplyToSave(GameSaveData data)
        {
            data.gold = Gold;
            data.gems = Gems;
            data.permanentGoldMultiplier = PermanentGoldMultiplier;
            data.removeAdsPurchased = RemoveAdsPurchased;
            data.vipBundlePurchased = VipBundlePurchased;
        }

        public void AddGold(double amount)
        {
            if (amount <= 0) return;
            Gold += amount * PermanentGoldMultiplier;
            OnGoldChanged?.Invoke(Gold);
        }

        /// <summary>Adds a raw amount without applying the permanent multiplier (e.g. IAP grants, raid losses are handled elsewhere).</summary>
        public void AddGoldRaw(double amount)
        {
            if (amount == 0) return;
            Gold += amount;
            if (Gold < 0) Gold = 0;
            OnGoldChanged?.Invoke(Gold);
        }

        public bool TrySpendGold(double amount)
        {
            if (amount <= 0 || Gold < amount) return false;
            Gold -= amount;
            OnGoldChanged?.Invoke(Gold);
            return true;
        }

        public void AddGems(long amount)
        {
            if (amount <= 0) return;
            Gems += amount;
            OnGemsChanged?.Invoke(Gems);
        }

        public bool TrySpendGems(long amount)
        {
            if (amount <= 0 || Gems < amount) return false;
            Gems -= amount;
            OnGemsChanged?.Invoke(Gems);
            return true;
        }

        public void SetPermanentGoldMultiplier(float multiplier)
        {
            PermanentGoldMultiplier = Mathf.Max(PermanentGoldMultiplier, multiplier);
        }

        public void MarkRemoveAdsPurchased() => RemoveAdsPurchased = true;

        public void MarkVipBundlePurchased() => VipBundlePurchased = true;
    }
}
