using System;
using System.IO;
using UnityEngine;

namespace GoldAndGoblins.Core
{
    /// <summary>Loads/saves GameSaveData as JSON under Application.persistentDataPath.</summary>
    public static class SaveSystem
    {
        private static string FilePath => Path.Combine(Application.persistentDataPath, GameConstants.SaveFileName);

        public static GameSaveData Load()
        {
            try
            {
                if (!File.Exists(FilePath))
                {
                    return GameSaveData.CreateNew();
                }

                string json = File.ReadAllText(FilePath);
                GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);
                return data ?? GameSaveData.CreateNew();
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Failed to load save, starting fresh. {e}");
                return GameSaveData.CreateNew();
            }
        }

        public static void Save(GameSaveData data)
        {
            try
            {
                data.lastSaveTimeUtc = DateTime.UtcNow.ToString("o");
                string json = JsonUtility.ToJson(data, prettyPrint: true);
                File.WriteAllText(FilePath, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Failed to save. {e}");
            }
        }
    }
}
