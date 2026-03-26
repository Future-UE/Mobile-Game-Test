using System;
using System.Collections.Generic;

namespace GameDevStudio.Models
{
    /// <summary>
    /// Holds all persistent stats for the player's studio.
    /// Serialised as part of the save file.
    /// </summary>
    [Serializable]
    public class StudioStats
    {
        // ── Identity ──────────────────────────────────────────────────────────
        public string StudioName = "Indie Dreams Studio";
        public int    FoundingYear = 1;

        // ── Financials ────────────────────────────────────────────────────────
        public float Money = 50000f;
        public float TotalEarned;
        public float TotalSpent;

        // ── Reputation ────────────────────────────────────────────────────────
        /// <summary>0-100. Affects review scores and game sales.</summary>
        public float Reputation = 10f;
        /// <summary>Fans accumulate over time from good releases.</summary>
        public int   Fans;

        // ── Progress ──────────────────────────────────────────────────────────
        public int GamesReleased;
        public int CurrentYear   = 1;
        public int CurrentMonth  = 1;   // 1-12
        public int CurrentWeek   = 1;   // 1-4

        // ── Office ────────────────────────────────────────────────────────────
        /// <summary>Office tier (0=Garage, 1=Small, 2=Medium, 3=Large, 4=Campus)</summary>
        public int OfficeTier;
        public int MaxStaff => OfficeTierMaxStaff[Math.Min(OfficeTier, OfficeTierMaxStaff.Length - 1)];

        private static readonly int[] OfficeTierMaxStaff = { 2, 5, 10, 20, 50 };

        // ── Unlocks ───────────────────────────────────────────────────────────
        public List<string> UnlockedGenreIds     = new List<string>();
        public List<string> UnlockedPlatformIds  = new List<string>();
        public List<string> CompletedResearchIds = new List<string>();

        // ── Helpers ───────────────────────────────────────────────────────────
        public void AddMoney(float amount)
        {
            Money += amount;
            if (amount > 0) TotalEarned += amount;
            else            TotalSpent  -= amount;
        }

        public bool CanAfford(float cost) => Money >= cost;

        public void ClampReputation() =>
            Reputation = Math.Max(0f, Math.Min(100f, Reputation));

        public string GetFormattedMoney() =>
            Money >= 1_000_000f ? $"${Money / 1_000_000f:F2}M"
          : Money >= 1_000f     ? $"${Money / 1_000f:F1}K"
          :                       $"${Money:F0}";
    }
}
