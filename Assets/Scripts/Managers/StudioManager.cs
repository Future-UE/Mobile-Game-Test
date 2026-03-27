using GameDevStudio.Data;
using GameDevStudio.Events;
using GameDevStudio.Models;

namespace GameDevStudio.Core
{
    /// <summary>
    /// Manages the studio's stats (money, reputation, office tier).
    /// Acts as the authoritative source for <see cref="StudioStats"/>.
    /// </summary>
    public class StudioManager
    {
        public StudioStats Stats { get; private set; } = new StudioStats();

        // ── Initialisation ────────────────────────────────────────────────────
        public void Initialise(string studioName)
        {
            Stats = new StudioStats { StudioName = studioName };

            // Unlock all genres, platforms, and staff roles that start unlocked
            // according to their data assets — no hard-coded ids needed.
            foreach (var kv in GameDatabase.Instance.Genres)
                if (kv.Value.StartsUnlocked)
                    Stats.UnlockedGenreIds.Add(kv.Key);

            foreach (var kv in GameDatabase.Instance.Platforms)
                if (kv.Value.StartsUnlocked)
                    Stats.UnlockedPlatformIds.Add(kv.Key);

            foreach (var kv in GameDatabase.Instance.StaffRoles)
                if (kv.Value.StartsUnlocked)
                    Stats.UnlockedStaffRoleIds.Add(kv.Key);
        }

        public void RestoreStats(StudioStats saved) => Stats = saved;

        // ── Money ─────────────────────────────────────────────────────────────
        public void AddMoney(float amount)
        {
            float old = Stats.Money;
            Stats.AddMoney(amount);
            GameEventBus.Publish(new MoneyChangedEvent
            {
                OldAmount = old,
                NewAmount = Stats.Money,
                Delta     = amount
            });
        }

        public bool TrySpend(float cost)
        {
            if (!Stats.CanAfford(cost)) return false;
            AddMoney(-cost);
            return true;
        }

        // ── Reputation ────────────────────────────────────────────────────────
        public void AddReputation(float delta)
        {
            float old = Stats.Reputation;
            Stats.Reputation += delta;
            Stats.ClampReputation();
            GameEventBus.Publish(new ReputationChangedEvent
            {
                OldValue = old,
                NewValue = Stats.Reputation
            });
        }

        // ── Office ────────────────────────────────────────────────────────────
        /// <summary>
        /// Costs to upgrade to each office tier indexed by (target tier - 1).
        /// </summary>
        private static readonly float[] UpgradeCosts = { 10_000f, 50_000f, 200_000f, 1_000_000f };

        public bool CanUpgradeOffice()
        {
            int next = Stats.OfficeTier + 1;
            if (next >= UpgradeCosts.Length + 1) return false;
            return Stats.CanAfford(UpgradeCosts[Stats.OfficeTier]);
        }

        public bool TryUpgradeOffice()
        {
            if (!CanUpgradeOffice()) return false;
            TrySpend(UpgradeCosts[Stats.OfficeTier]);
            Stats.OfficeTier++;
            GameEventBus.Publish(new NotificationEvent
            {
                Message  = $"Office upgraded to tier {Stats.OfficeTier}! Max staff: {Stats.MaxStaff}.",
                Severity = NotificationSeverity.Success
            });
            return true;
        }

        public float GetOfficeTierUpgradeCost()
        {
            int next = Stats.OfficeTier;
            return next < UpgradeCosts.Length ? UpgradeCosts[next] : float.MaxValue;
        }

        public string GetOfficeTierName() => Stats.OfficeTier switch
        {
            0 => "Garage",
            1 => "Small Office",
            2 => "Medium Office",
            3 => "Large Studio",
            4 => "Corporate Campus",
            _ => "Unknown"
        };

        // ── Genre / Platform unlocks ──────────────────────────────────────────
        public void UnlockGenre(string genreId)
        {
            if (!Stats.UnlockedGenreIds.Contains(genreId))
                Stats.UnlockedGenreIds.Add(genreId);
        }

        public void UnlockPlatform(string platformId)
        {
            if (!Stats.UnlockedPlatformIds.Contains(platformId))
                Stats.UnlockedPlatformIds.Add(platformId);
        }

        public void UnlockStaffRole(string staffRoleId)
        {
            if (!Stats.UnlockedStaffRoleIds.Contains(staffRoleId))
                Stats.UnlockedStaffRoleIds.Add(staffRoleId);
        }

        public bool IsGenreUnlocked(string genreId)       => Stats.UnlockedGenreIds.Contains(genreId);
        public bool IsPlatformUnlocked(string platformId) => Stats.UnlockedPlatformIds.Contains(platformId);
        public bool IsStaffRoleUnlocked(string roleId)    => Stats.UnlockedStaffRoleIds.Contains(roleId);

        // ── Reputation gain multiplier ────────────────────────────────────────
        /// <summary>
        /// Aggregates the ReputationGainMultiplier from all completed research nodes.
        /// Returns 1.0 when no relevant research is completed.
        /// </summary>
        public float GetReputationGainMultiplier()
        {
            float multiplier = 1f;
            foreach (var nodeId in Stats.CompletedResearchIds)
            {
                var data = GameDatabase.Instance.GetResearch(nodeId);
                if (data != null && data.ReputationGainMultiplier > 1f)
                    multiplier *= data.ReputationGainMultiplier;
            }
            return multiplier;
        }
    }
}
