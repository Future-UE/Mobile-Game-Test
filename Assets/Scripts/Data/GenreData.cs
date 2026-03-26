using UnityEngine;

namespace GameDevStudio.Data
{
    /// <summary>
    /// Defines a game genre (e.g. RPG, Action, Puzzle).
    /// Create instances via Assets → Create → GameDevStudio → Genre.
    /// </summary>
    [CreateAssetMenu(fileName = "NewGenre", menuName = "GameDevStudio/Genre")]
    public class GenreData : ScriptableObject
    {
        [Header("Identity")]
        public string GenreId;
        public string DisplayName;
        [TextArea(2, 4)]
        public string Description;

        [Header("Requirements")]
        /// <summary>Reputation needed to unlock this genre.</summary>
        public float UnlockReputation;
        /// <summary>If not empty, one of these research nodes must be completed first.</summary>
        public string[] RequiredResearchIds;
        public bool StartsUnlocked = false;

        [Header("Development Modifiers")]
        /// <summary>Multiplier on development time (1 = normal).</summary>
        [Range(0.5f, 3f)]
        public float DevTimeMultiplier = 1f;
        /// <summary>How much Art skill matters for this genre (0-1).</summary>
        [Range(0f, 1f)]
        public float ArtWeight = 0.25f;
        /// <summary>How much Programming skill matters (0-1).</summary>
        [Range(0f, 1f)]
        public float ProgrammingWeight = 0.25f;
        /// <summary>How much Design skill matters (0-1).</summary>
        [Range(0f, 1f)]
        public float DesignWeight = 0.25f;
        /// <summary>How much Testing skill matters (0-1).</summary>
        [Range(0f, 1f)]
        public float TestingWeight = 0.25f;

        [Header("Market")]
        /// <summary>Base market appeal (1-10).</summary>
        [Range(1f, 10f)]
        public float BaseMarketAppeal = 5f;
        /// <summary>Approximate price per unit sold.</summary>
        public float BasePricePerUnit = 2.99f;
    }
}
