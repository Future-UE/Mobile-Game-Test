using UnityEngine;

namespace GameDevStudio.Data
{
    /// <summary>
    /// Defines a platform the studio can target (Mobile, PC, Console, etc.).
    /// Create instances via Assets → Create → GameDevStudio → Platform.
    /// </summary>
    [CreateAssetMenu(fileName = "NewPlatform", menuName = "GameDevStudio/Platform")]
    public class PlatformData : ScriptableObject
    {
        [Header("Identity")]
        public string PlatformId;
        public string DisplayName;
        [TextArea(2, 4)]
        public string Description;

        [Header("Requirements")]
        public float UnlockReputation;
        public string[] RequiredResearchIds;
        public bool StartsUnlocked = false;

        [Header("Development Modifiers")]
        /// <summary>Extra multiplier on development effort.</summary>
        [Range(0.5f, 3f)]
        public float DevEffortMultiplier = 1f;
        /// <summary>Minimum team size recommended for this platform.</summary>
        public int MinTeamSize = 1;

        [Header("Market")]
        /// <summary>Estimated global audience size (affects sales ceiling).</summary>
        public float AudienceMultiplier = 1f;
        /// <summary>Standard royalty/cut taken by the platform store (0-1).</summary>
        [Range(0f, 0.5f)]
        public float PlatformCut = 0.30f;
        /// <summary>Approximate development cost modifier.</summary>
        public float CostMultiplier = 1f;
    }
}
