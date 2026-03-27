using System;

namespace GameDevStudio.Models
{
    /// <summary>
    /// Tracks the state of a single research node (upgrade).
    /// Mirrors a ResearchNodeData asset but carries runtime state.
    /// </summary>
    [Serializable]
    public class ResearchNode
    {
        /// <summary>Must match ResearchNodeData.NodeId</summary>
        public string NodeId;

        public ResearchStatus Status = ResearchStatus.Locked;

        /// <summary>Employee currently assigned to this research task.</summary>
        public string AssignedEmployeeId;

        /// <summary>Weeks of research work contributed so far.</summary>
        public float WeeksInvested;

        // ── Computed helpers ──────────────────────────────────────────────────
        public bool IsCompleted  => Status == ResearchStatus.Completed;
        public bool IsInProgress => Status == ResearchStatus.InProgress;
        public bool IsAvailable  => Status == ResearchStatus.Available;
    }

    public enum ResearchStatus
    {
        Locked,
        Available,
        InProgress,
        Completed
    }
}
