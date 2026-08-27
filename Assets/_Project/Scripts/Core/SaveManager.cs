using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace GoldAndGoblins.Core
{
    // Local save only. Obfuscated, not encrypted -- fine for casual tamper resistance,
    // not for anti-cheat. Add server-authoritative validation before relying on this
    // for anything IAP-adjacent (see IAPManager's IReceiptValidator).
    public class SaveManager : GoldAndGoblins.Utils.Singleton<SaveManager>
    {
        private const string FileName = "goldandgoblins.sav";
        private static readonly byte[] ObfuscationKey = Encoding.UTF8.GetBytes("GoldAndGoblinsSaveKeyV1");

        public SaveData Current { get; private set; }

        private string FilePath => Path.Combine(Application.persistentDataPath, FileName);

        protected override void Awake()
        {
            base.Awake();
            Load();
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause) Save();
        }

        private void OnApplicationQuit() => Save();

        public void Load()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    var raw = File.ReadAllBytes(FilePath);
                    var json = Encoding.UTF8.GetString(Xor(raw));
                    Current = JsonUtility.FromJson<SaveData>(json) ?? new SaveData();
                }
                else
                {
                    Current = new SaveData();
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveManager] Failed to load save, starting fresh: {e.Message}");
                Current = new SaveData();
            }

            if (string.IsNullOrEmpty(Current.lastSaveUtcTicks) || Current.lastSaveUtcTicks == "0")
            {
                Current.LastSaveUtc = DateTime.UtcNow;
            }
        }

        public void Save()
        {
            if (Current == null) return;
            Current.LastSaveUtc = DateTime.UtcNow;

            try
            {
                var json = JsonUtility.ToJson(Current);
                var bytes = Xor(Encoding.UTF8.GetBytes(json));
                File.WriteAllBytes(FilePath, bytes);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Failed to save: {e.Message}");
            }
        }

        private static byte[] Xor(byte[] data)
        {
            var result = new byte[data.Length];
            for (var i = 0; i < data.Length; i++)
            {
                result[i] = (byte)(data[i] ^ ObfuscationKey[i % ObfuscationKey.Length]);
            }
            return result;
        }
    }
}
