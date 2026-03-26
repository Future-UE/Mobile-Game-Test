using UnityEngine;

namespace GameDevStudio.Data
{
    public enum EventTrigger
    {
        /// <summary>Fires randomly after a configurable minimum month.</summary>
        Random,
        /// <summary>Fires when a game is released.</summary>
        OnGameRelease,
        /// <summary>Fires when the studio hires its Nth employee.</summary>
        OnHire,
        /// <summary>Fires when a specific research node is completed.</summary>
        OnResearchComplete,
        /// <summary>Fires when reputation crosses a threshold.</summary>
        OnReputationThreshold,
        /// <summary>Fires once at the start of a specific in-game year.</summary>
        OnYearStart,
    }

    /// <summary>
    /// Defines a random or triggered in-game event.
    /// Create instances via Assets → Create → GameDevStudio → RandomEvent.
    /// </summary>
    [CreateAssetMenu(fileName = "NewEvent", menuName = "GameDevStudio/RandomEvent")]
    public class RandomEventData : ScriptableObject
    {
        [Header("Identity")]
        public string EventId;
        public string Title;
        [TextArea(3, 6)]
        public string Description;
        /// <summary>Tag used to group events (e.g. "Industry", "Office", "Staff").</summary>
        public string Category;

        [Header("Trigger")]
        public EventTrigger Trigger = EventTrigger.Random;
        /// <summary>Minimum in-game month before this event can fire.</summary>
        public int MinMonth = 0;
        /// <summary>
        /// Probability weight relative to other random events (higher = more likely).
        /// </summary>
        [Range(0f, 10f)]
        public float Weight = 1f;
        /// <summary>For OnReputationThreshold events.</summary>
        public float ReputationThreshold;
        /// <summary>For OnYearStart events.</summary>
        public int TargetYear;
        /// <summary>If true this event fires only once per save.</summary>
        public bool OneTimeOnly = false;

        [Header("Choices")]
        /// <summary>
        /// If empty, the event is purely informational (one "OK" button).
        /// Otherwise present the player with these choices.
        /// </summary>
        public EventChoice[] Choices;
    }

    [System.Serializable]
    public class EventChoice
    {
        public string Label;
        [TextArea(1, 3)]
        public string ResultDescription;

        [Header("Effects")]
        public float MoneyDelta;
        public float ReputationDelta;
        public float MoraleDelta;
        /// <summary>Employee id to fire (leave blank if none).</summary>
        public string FireEmployeeId;
        /// <summary>Research node id to immediately complete (leave blank if none).</summary>
        public string CompleteResearchId;
        /// <summary>Set to true to skip this choice's effect being applied (for "ignore" options).</summary>
        public bool IsIgnore;
    }
}
