using System;
using System.Collections.Generic;

namespace GameDevStudio.Events
{
    /// <summary>
    /// Lightweight type-safe event bus used to decouple game systems.
    /// 
    /// Usage:
    ///   Subscribe:   GameEventBus.Subscribe&lt;WeekPassedEvent&gt;(OnWeekPassed);
    ///   Unsubscribe: GameEventBus.Unsubscribe&lt;WeekPassedEvent&gt;(OnWeekPassed);
    ///   Publish:     GameEventBus.Publish(new WeekPassedEvent { Week = 5, Month = 2 });
    /// </summary>
    public static class GameEventBus
    {
        private static readonly Dictionary<Type, List<Delegate>> _listeners = new();

        public static void Subscribe<T>(Action<T> listener)
        {
            var type = typeof(T);
            if (!_listeners.ContainsKey(type))
                _listeners[type] = new List<Delegate>();
            _listeners[type].Add(listener);
        }

        public static void Unsubscribe<T>(Action<T> listener)
        {
            var type = typeof(T);
            if (_listeners.TryGetValue(type, out var list))
                list.Remove(listener);
        }

        public static void Publish<T>(T evt)
        {
            var type = typeof(T);
            if (!_listeners.TryGetValue(type, out var list)) return;

            // Iterate a copy to allow listeners to unsubscribe during the call.
            foreach (var del in list.ToArray())
                ((Action<T>)del)?.Invoke(evt);
        }

        /// <summary>Removes all subscriptions. Call on scene/save reset.</summary>
        public static void Clear() => _listeners.Clear();
    }

    // ── Event structs ─────────────────────────────────────────────────────────

    public struct WeekPassedEvent
    {
        public int Week;
        public int Month;
        public int Year;
    }

    public struct MoneyChangedEvent
    {
        public float OldAmount;
        public float NewAmount;
        public float Delta;
    }

    public struct ReputationChangedEvent
    {
        public float OldValue;
        public float NewValue;
    }

    public struct ProjectStartedEvent
    {
        public string ProjectId;
        public string ProjectTitle;
    }

    public struct ProjectPhaseChangedEvent
    {
        public string ProjectId;
        public Models.ProjectPhase OldPhase;
        public Models.ProjectPhase NewPhase;
    }

    public struct ProjectReleasedEvent
    {
        public string ProjectId;
        public string ProjectTitle;
        public float  ReviewScore;
        public int    UnitsSold;
        public float  Revenue;
    }

    public struct EmployeeHiredEvent
    {
        public string EmployeeId;
        public string Name;
        public string RoleId;
    }

    public struct EmployeeFiredEvent
    {
        public string EmployeeId;
        public string Name;
    }

    public struct ResearchCompletedEvent
    {
        public string NodeId;
        public string DisplayName;
    }

    public struct RandomEventFiredEvent
    {
        public string EventId;
        public string Title;
    }

    public struct NotificationEvent
    {
        public string Message;
        public NotificationSeverity Severity;
    }

    public enum NotificationSeverity
    {
        Info,
        Success,
        Warning,
        Danger
    }
}
