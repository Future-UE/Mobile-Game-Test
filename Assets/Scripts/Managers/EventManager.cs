using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GameDevStudio.Data;
using GameDevStudio.Events;
using GameDevStudio.Models;

namespace GameDevStudio.Core
{
    /// <summary>
    /// Drives random and triggered in-game events.
    /// Manages the event queue so the UI can present one event at a time.
    /// </summary>
    public class EventManager
    {
        // ── State ─────────────────────────────────────────────────────────────
        private readonly Queue<RandomEventData> _eventQueue   = new();
        private readonly HashSet<string>        _firedOneTime = new();

        // ── Public pending events ─────────────────────────────────────────────
        public bool              HasPendingEvent  => _eventQueue.Count > 0;
        public RandomEventData   PeekNextEvent()  => _eventQueue.Count > 0 ? _eventQueue.Peek() : null;

        public RandomEventData DequeueEvent()
        {
            return _eventQueue.Count > 0 ? _eventQueue.Dequeue() : null;
        }

        // ── Weekly tick ───────────────────────────────────────────────────────
        public void OnWeekPassed(StudioStats stats, WeekPassedEvent e)
        {
            int totalMonths = (e.Year - 1) * 12 + e.Month;

            // Tick post-release sales
            GameManager.Instance.Projects.TickReleasedSales(stats);

            // Try fire a random event once per month (week 1 of each month)
            if (e.Week == 1)
                TryFireRandomEvent(stats, totalMonths);

            // Year-start events
            if (e.Week == 1 && e.Month == 1)
                TryFireYearStartEvents(stats, e.Year);

            // Reputation threshold events
            TryFireReputationEvents(stats);
        }

        // ── Random event roll ─────────────────────────────────────────────────
        private void TryFireRandomEvent(StudioStats stats, int totalMonths)
        {
            var candidates = GameDatabase.Instance.Events.Values
                .Where(ev => ev.Trigger == EventTrigger.Random
                          && totalMonths >= ev.MinMonth
                          && (!ev.OneTimeOnly || !_firedOneTime.Contains(ev.EventId)))
                .ToList();

            if (candidates.Count == 0) return;

            float totalWeight = candidates.Sum(ev => ev.Weight);
            float roll        = Random.Range(0f, totalWeight);
            float cumulative  = 0f;

            foreach (var ev in candidates)
            {
                cumulative += ev.Weight;
                if (roll <= cumulative)
                {
                    EnqueueEvent(ev);
                    return;
                }
            }
        }

        private void TryFireYearStartEvents(StudioStats stats, int year)
        {
            foreach (var ev in GameDatabase.Instance.Events.Values
                .Where(e => e.Trigger == EventTrigger.OnYearStart
                         && e.TargetYear == year
                         && (!e.OneTimeOnly || !_firedOneTime.Contains(e.EventId))))
            {
                EnqueueEvent(ev);
            }
        }

        private void TryFireReputationEvents(StudioStats stats)
        {
            foreach (var ev in GameDatabase.Instance.Events.Values
                .Where(e => e.Trigger == EventTrigger.OnReputationThreshold
                         && stats.Reputation >= e.ReputationThreshold
                         && e.OneTimeOnly
                         && !_firedOneTime.Contains(e.EventId)))
            {
                EnqueueEvent(ev);
            }
        }

        // ── External triggers ─────────────────────────────────────────────────
        public void TriggerOnGameRelease(string projectId)
        {
            foreach (var ev in GameDatabase.Instance.Events.Values
                .Where(e => e.Trigger == EventTrigger.OnGameRelease
                         && (!e.OneTimeOnly || !_firedOneTime.Contains(e.EventId))))
            {
                EnqueueEvent(ev);
            }
        }

        public void TriggerOnHire()
        {
            int count = GameManager.Instance.Staff.AllEmployees.Count;
            foreach (var ev in GameDatabase.Instance.Events.Values
                .Where(e => e.Trigger == EventTrigger.OnHire
                         && (!e.OneTimeOnly || !_firedOneTime.Contains(e.EventId))))
            {
                EnqueueEvent(ev);
            }
        }

        public void TriggerOnResearchComplete(string nodeId)
        {
            foreach (var ev in GameDatabase.Instance.Events.Values
                .Where(e => e.Trigger == EventTrigger.OnResearchComplete
                         && (!e.OneTimeOnly || !_firedOneTime.Contains(e.EventId))))
            {
                EnqueueEvent(ev);
            }
        }

        // ── Choice resolution ─────────────────────────────────────────────────
        /// <summary>
        /// Apply the effects of a player choice from a fired event.
        /// </summary>
        public void ResolveChoice(RandomEventData ev, EventChoice choice)
        {
            if (choice.IsIgnore) return;

            var studio = GameManager.Instance.Studio;
            studio.AddMoney(choice.MoneyDelta);
            studio.AddReputation(choice.ReputationDelta);

            if (choice.MoraleDelta != 0f)
                GameManager.Instance.Staff.BoostAllMorale(choice.MoraleDelta);

            if (!string.IsNullOrEmpty(choice.FireEmployeeId))
                GameManager.Instance.Staff.FireEmployee(choice.FireEmployeeId);

            if (!string.IsNullOrEmpty(choice.CompleteResearchId))
                GameManager.Instance.Research.ForceComplete(choice.CompleteResearchId);

            if (!string.IsNullOrEmpty(choice.ResultDescription))
                GameEventBus.Publish(new NotificationEvent
                {
                    Message  = choice.ResultDescription,
                    Severity = choice.MoneyDelta >= 0f
                                   ? NotificationSeverity.Success
                                   : NotificationSeverity.Warning
                });
        }

        // ── Internal helpers ──────────────────────────────────────────────────
        private void EnqueueEvent(RandomEventData ev)
        {
            if (ev.OneTimeOnly) _firedOneTime.Add(ev.EventId);
            _eventQueue.Enqueue(ev);

            GameEventBus.Publish(new RandomEventFiredEvent
            {
                EventId = ev.EventId,
                Title   = ev.Title
            });
        }
    }
}
