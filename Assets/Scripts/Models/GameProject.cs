using System;
using System.Collections.Generic;

namespace GameDevStudio.Models
{
    /// <summary>
    /// Represents a game project being developed (or already released) by the studio.
    /// </summary>
    [Serializable]
    public class GameProject
    {
        // ── Identity ──────────────────────────────────────────────────────────
        public string Id;
        public string Title;
        /// <summary>References GenreData.GenreId</summary>
        public string GenreId;
        /// <summary>References PlatformData.PlatformId</summary>
        public string PlatformId;

        // ── Development ───────────────────────────────────────────────────────
        public ProjectPhase Phase = ProjectPhase.Concept;
        /// <summary>Total weeks allocated to development.</summary>
        public int    PlannedWeeks;
        public int    WeeksSpent;
        /// <summary>Aggregate quality points accumulated during dev (0–1000+).</summary>
        public float  QualityPoints;
        /// <summary>Aggregate bug count; must be reduced before release.</summary>
        public int    Bugs;
        /// <summary>
        /// How much of the current phase is complete (0-1).
        /// Advances each week based on assigned staff output.
        /// </summary>
        public float  PhaseProgress;

        // ── Budget ────────────────────────────────────────────────────────────
        public float  Budget;
        public float  SpentBudget;

        // ── Release ───────────────────────────────────────────────────────────
        public bool   IsReleased;
        public float  ReviewScore;      // 0-10
        public int    UnitsSold;
        public float  TotalRevenue;
        public int    WeeksOnSale;
        /// <summary>Hype built during dev phase through marketing actions.</summary>
        public float  Hype;

        // ── Staff assigned to this project ────────────────────────────────────
        public List<string> AssignedEmployeeIds = new List<string>();

        // ── Computed helpers ──────────────────────────────────────────────────
        public float DevelopmentProgress =>
            PlannedWeeks == 0 ? 0 : (float)WeeksSpent / PlannedWeeks;

        public bool IsComplete => WeeksSpent >= PlannedWeeks && Phase == ProjectPhase.Polishing && PhaseProgress >= 1f;

        public string GetPhaseLabel() => Phase switch
        {
            ProjectPhase.Concept    => "Concept",
            ProjectPhase.PreProd    => "Pre-Production",
            ProjectPhase.Production => "Production",
            ProjectPhase.Testing    => "Testing & QA",
            ProjectPhase.Polishing  => "Polishing",
            ProjectPhase.Released   => "Released",
            _                       => "Unknown"
        };

        public string GetReviewLabel() =>
            ReviewScore >= 9.0f ? "Masterpiece" :
            ReviewScore >= 8.0f ? "Great" :
            ReviewScore >= 7.0f ? "Good" :
            ReviewScore >= 6.0f ? "Average" :
            ReviewScore >= 5.0f ? "Mediocre" :
                                  "Terrible";
    }

    public enum ProjectPhase
    {
        Concept,
        PreProd,
        Production,
        Testing,
        Polishing,
        Released
    }
}
