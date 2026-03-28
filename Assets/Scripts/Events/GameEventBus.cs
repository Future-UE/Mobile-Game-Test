using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameDevStudio.Events
{
    /// <summary>
    /// Lightweight type-safe event bus used to decouple game systems.
    ///
    /// Usage:
    ///   Subscribe:   GameEventBus.Subscribe&lt;WeekTickEvent&gt;(OnWeekTick);
    ///   Unsubscribe: GameEventBus.Unsubscribe&lt;WeekTickEvent&gt;(OnWeekTick);
    ///   Publish:     GameEventBus.Publish(new WeekTickEvent { Week = 5, Month = 2 });
    ///
    /// Set <see cref="DebugMode"/> = true to log all subscribe/unsubscribe/publish calls.
    /// </summary>
    public static class GameEventBus
    {
        // ── Debug mode ────────────────────────────────────────────────────────
        /// <summary>
        /// When true, logs every subscribe, unsubscribe, and publish call via
        /// <c>Debug.Log</c>. Toggle at runtime from the Inspector or any script.
        /// </summary>
        public static bool DebugMode = false;

        private static readonly Dictionary<Type, List<Delegate>> _listeners = new();

        /// <summary>
        /// Subscribes <paramref name="listener"/> to events of type <typeparamref name="T"/>.
        /// Duplicate subscriptions (same delegate instance) are silently ignored.
        /// </summary>
        public static void Subscribe<T>(Action<T> listener)
        {
            var type = typeof(T);
            if (!_listeners.ContainsKey(type))
                _listeners[type] = new List<Delegate>();

            // Prevent duplicate subscriptions.
            if (_listeners[type].Contains(listener))
            {
                if (DebugMode)
                    Debug.LogWarning($"[GameEventBus] Duplicate subscription ignored: {typeof(T).Name}");
                return;
            }

            _listeners[type].Add(listener);

            if (DebugMode)
                Debug.Log($"[GameEventBus] Subscribed to {typeof(T).Name}. Total listeners: {_listeners[type].Count}");
        }

        /// <summary>Removes <paramref name="listener"/> from the <typeparamref name="T"/> channel.</summary>
        public static void Unsubscribe<T>(Action<T> listener)
        {
            var type = typeof(T);
            if (_listeners.TryGetValue(type, out var list))
            {
                list.Remove(listener);
                if (DebugMode)
                    Debug.Log($"[GameEventBus] Unsubscribed from {typeof(T).Name}. Remaining listeners: {list.Count}");
            }
        }

        /// <summary>Publishes <paramref name="evt"/> to all subscribers of <typeparamref name="T"/>.</summary>
        public static void Publish<T>(T evt)
        {
            var type = typeof(T);
            if (!_listeners.TryGetValue(type, out var list)) return;

            if (DebugMode)
                Debug.Log($"[GameEventBus] Publishing {typeof(T).Name} to {list.Count} listener(s)");

            // Iterate a snapshot to allow listeners to safely unsubscribe mid-publish.
            foreach (var del in list.ToArray())
                ((Action<T>)del)?.Invoke(evt);
        }

        /// <summary>Returns the current number of active listeners for <typeparamref name="T"/>.</summary>
        public static int GetListenerCount<T>() =>
            _listeners.TryGetValue(typeof(T), out var list) ? list.Count : 0;

        /// <summary>Removes all subscriptions. Call on scene/save reset.</summary>
        public static void Clear() => _listeners.Clear();
    }

    // ── Event structs ─────────────────────────────────────────────────────────

    /// <summary>
    /// Published by <see cref="GameDevStudio.Core.TimeManager"/> once per in-game week.
    /// All managers subscribe to this event instead of being called directly.
    /// </summary>
    public struct WeekTickEvent
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
