using System.Collections.Generic;

namespace GameDevStudio.Utils
{
    /// <summary>
    /// Miscellaneous extension methods used across the codebase.
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Clamps a float between 0 and 100 (useful for stat fields).
        /// </summary>
        public static float Clamp0100(this float value) =>
            value < 0f ? 0f : value > 100f ? 100f : value;

        /// <summary>
        /// Returns a human-readable money string with K/M suffixes.
        /// </summary>
        public static string ToMoneyString(this float amount) =>
            amount >= 1_000_000f ? $"${amount / 1_000_000f:F2}M"
          : amount >= 1_000f     ? $"${amount / 1_000f:F1}K"
          :                        $"${amount:F0}";
    }
}
