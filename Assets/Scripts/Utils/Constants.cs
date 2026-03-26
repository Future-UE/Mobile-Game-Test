namespace GameDevStudio.Utils
{
    /// <summary>
    /// Shared constant values. Centralising them here makes balance tweaks easy.
    /// </summary>
    public static class Constants
    {
        // ── Time ──────────────────────────────────────────────────────────────
        public const int WeeksPerMonth = 4;
        public const int MonthsPerYear = 12;

        // ── Money ─────────────────────────────────────────────────────────────
        public const float StartingMoney       = 50_000f;
        public const float TrainingCostBase    = 2_000f;
        public const float MarketingCostBase   = 5_000f;

        // ── Reputation ────────────────────────────────────────────────────────
        public const float MaxReputation       = 100f;
        public const float InitialReputation   = 10f;

        // ── Staff ─────────────────────────────────────────────────────────────
        public const float MoraleDecayPerWeek  = 0.5f;
        public const float ExperienceGainPerWeek = 0.5f;

        // ── Projects ──────────────────────────────────────────────────────────
        public const int   MinProjectWeeks     = 4;
        public const float BugChancePerWeek    = 0.3f;
        public const int   MaxBugsPerWeek      = 3;

        // ── Research ─────────────────────────────────────────────────────────
        public const float MinResearchWeeks    = 1f;
    }
}
