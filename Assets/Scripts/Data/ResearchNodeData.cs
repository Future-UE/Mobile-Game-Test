using UnityEngine;

namespace GameDevStudio.Data
{
    /// <summary>
    /// Defines a research node (upgrade) that can be unlocked during the game.
    /// Create instances via Assets → Create → GameDevStudio → ResearchNode.
    /// Nodes form a tree via <see cref="PrerequisiteNodeIds"/>.
    /// </summary>
    [CreateAssetMenu(fileName = "NewResearchNode", menuName = "GameDevStudio/ResearchNode")]
    public class ResearchNodeData : ScriptableObject
    {
        [Header("Identity")]
        public string NodeId;
        public string DisplayName;
        [TextArea(2, 4)]
        public string Description;
        /// <summary>Category tag for grouping in the UI (e.g. "Tech", "Business", "Creative").</summary>
        public string Category;

        [Header("Prerequisites")]
        public string[] PrerequisiteNodeIds;
        public float    MinReputation;
        public bool     StartsAvailable = false;

        [Header("Cost")]
        public float    MoneyCost;
        /// <summary>Total weeks of researcher time required to complete.</summary>
        public float    WeeksRequired = 4f;

        [Header("Effects")]
        /// <summary>
        /// Which genre ids this node unlocks (leave empty if none).
        /// </summary>
        public string[] UnlocksGenreIds;
        /// <summary>
        /// Which platform ids this node unlocks (leave empty if none).
        /// </summary>
        public string[] UnlocksPlatformIds;
        /// <summary>
        /// Which staff role ids this node unlocks (leave empty if none).
        /// </summary>
        public string[] UnlocksStaffRoleIds;
        /// <summary>Flat bonus added to all game quality points per week.</summary>
        public float    QualityBonus;
        /// <summary>Multiplier on studio reputation gain (1 = no change).</summary>
        [Range(1f, 3f)]
        public float    ReputationGainMultiplier = 1f;
        /// <summary>Flat weekly passive income granted after completion.</summary>
        public float    PassiveIncomePerWeek;
        /// <summary>
        /// Free-form effect description shown in the UI tooltip.
        /// </summary>
        [TextArea(2, 4)]
        public string   EffectSummary;
    }
}
