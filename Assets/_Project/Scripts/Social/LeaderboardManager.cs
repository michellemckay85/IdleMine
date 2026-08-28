using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Models;
using GoldAndGoblins.Core;

namespace GoldAndGoblins.Social
{
    public readonly struct LeaderboardRow
    {
        public readonly int Rank;
        public readonly string PlayerName;
        public readonly long Score;
        public readonly bool IsLocalPlayer;

        public LeaderboardRow(int rank, string playerName, long score, bool isLocalPlayer)
        {
            Rank = rank;
            PlayerName = playerName;
            Score = score;
            IsLocalPlayer = isLocalPlayer;
        }
    }

    // Ranks players by SaveData.totalGoldEverEarned -- unlike lifetimeGoldEarned, that
    // stat never resets on prestige, so it climbs forever and makes a sane leaderboard
    // score for an idle game.
    //
    // Requires manual one-time setup this code can't do for you:
    // 1. Edit > Project Settings > Services -- sign in and link (or create) a Unity
    //    Gaming Services project.
    // 2. In the Unity Cloud Dashboard (cloud.unity.com) for that project, open
    //    Leaderboards and create one with the exact ID below, sort order Descending,
    //    score format Long.
    //
    // Written against the documented UGS Leaderboards API but not compiled/run here --
    // this session has no Unity Editor access. If Unity's compiler flags a mismatched
    // method/type name on first import, that's a package-version API drift, not a
    // logic bug; report the exact error and it's a quick fix.
    public class LeaderboardManager : GoldAndGoblins.Utils.Singleton<LeaderboardManager>
    {
        public const string LeaderboardId = "total_gold_earned";
        private const string DefaultNamePrefix = "Miner";

        public bool IsReady { get; private set; }

        private async void Start()
        {
            try
            {
                await UnityServices.InitializeAsync();

                if (!AuthenticationService.Instance.IsSignedIn)
                {
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                }

                await EnsurePlayerNameAsync();
                IsReady = true;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[LeaderboardManager] Unity Gaming Services failed to initialize -- " +
                                  $"leaderboard will be unavailable until this is resolved: {e.Message}");
            }
        }

        private async Task EnsurePlayerNameAsync()
        {
            if (!string.IsNullOrEmpty(AuthenticationService.Instance.PlayerName)) return;

            var name = $"{DefaultNamePrefix}{UnityEngine.Random.Range(1000, 9999)}";
            await AuthenticationService.Instance.UpdatePlayerNameAsync(name);
        }

        public async Task SubmitScoreAsync()
        {
            if (!IsReady) return;

            try
            {
                var score = (long)SaveManager.Instance.Current.totalGoldEverEarned;
                await LeaderboardsService.Instance.AddPlayerScoreAsync(LeaderboardId, score);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[LeaderboardManager] Failed to submit score: {e.Message}");
            }
        }

        public async Task<List<LeaderboardRow>> GetTopScoresAsync(int count = 20)
        {
            if (!IsReady) return new List<LeaderboardRow>();

            try
            {
                var localPlayerId = AuthenticationService.Instance.PlayerId;
                var response = await LeaderboardsService.Instance.GetScoresAsync(
                    LeaderboardId, new GetScoresOptions { Offset = 0, Limit = count });

                var rows = new List<LeaderboardRow>();
                foreach (var entry in response.Results)
                {
                    rows.Add(new LeaderboardRow(entry.Rank + 1, entry.PlayerName, (long)entry.Score, entry.PlayerId == localPlayerId));
                }
                return rows;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[LeaderboardManager] Failed to fetch scores: {e.Message}");
                return new List<LeaderboardRow>();
            }
        }
    }
}
