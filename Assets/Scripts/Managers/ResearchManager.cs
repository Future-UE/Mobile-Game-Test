using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GameDevStudio.Data;
using GameDevStudio.Events;
using GameDevStudio.Models;

namespace GameDevStudio.Core
{
    /// <summary>
    /// Manages the research tree: unlocking nodes, progressing research, and
    /// applying completed node effects to the studio.
    /// </summary>
    public class ResearchManager
    {
        private readonly Dictionary<string, ResearchNode> _nodes = new();

        // ── Initialisation ────────────────────────────────────────────────────
        public void Initialise()
        {
            _nodes.Clear();
            foreach (var kv in GameDatabase.Instance.ResearchNodes)
            {
                var nodeData = kv.Value;
                var node = new ResearchNode { NodeId = nodeData.NodeId };

                node.Status = nodeData.StartsAvailable
                    ? ResearchStatus.Available
                    : ResearchStatus.Locked;

                _nodes[nodeData.NodeId] = node;
            }
        }

        public void RestoreNodes(ResearchNode[] saved)
        {
            _nodes.Clear();
            if (saved != null)
            {
                foreach (var node in saved)
                    _nodes[node.NodeId] = node;
            }
        }

        // ── Queries ───────────────────────────────────────────────────────────
        public ResearchNode[]             GetAllNodes()                  => _nodes.Values.ToArray();
        public ResearchNode               GetNode(string nodeId)
        {
            _nodes.TryGetValue(nodeId, out var node);
            return node;
        }
        public bool                       IsCompleted(string nodeId)     => _nodes.TryGetValue(nodeId, out var n) && n.IsCompleted;
        public IEnumerable<ResearchNode>  GetAvailableNodes()            => _nodes.Values.Where(n => n.IsAvailable);
        public IEnumerable<ResearchNode>  GetCompletedNodes()            => _nodes.Values.Where(n => n.IsCompleted);

        // ── Starting research ─────────────────────────────────────────────────
        /// <summary>
        /// Assigns an employee to research a node.
        /// Returns false if requirements are not met.
        /// </summary>
        public bool StartResearch(string nodeId, string employeeId)
        {
            if (!_nodes.TryGetValue(nodeId, out var node) || !node.IsAvailable)
                return false;

            var nodeData = GameDatabase.Instance.GetResearch(nodeId);
            if (nodeData == null) return false;

            var studio = GameManager.Instance.Studio;
            if (studio.Stats.Reputation < nodeData.MinReputation)
            {
                GameEventBus.Publish(new NotificationEvent
                {
                    Message  = $"Need {nodeData.MinReputation} reputation to research '{nodeData.DisplayName}'.",
                    Severity = NotificationSeverity.Warning
                });
                return false;
            }

            if (!studio.TrySpend(nodeData.MoneyCost))
            {
                GameEventBus.Publish(new NotificationEvent
                {
                    Message  = $"Not enough money to start research: {nodeData.DisplayName}.",
                    Severity = NotificationSeverity.Warning
                });
                return false;
            }

            node.Status             = ResearchStatus.InProgress;
            node.AssignedEmployeeId = employeeId;
            node.WeeksInvested      = 0f;

            var emp = GameManager.Instance.Staff.GetEmployee(employeeId);
            if (emp != null)
            {
                emp.Status         = EmployeeStatus.Researching;
                emp.AssignedTaskId = nodeId;
            }

            GameEventBus.Publish(new NotificationEvent
            {
                Message  = $"Research started: {nodeData.DisplayName}.",
                Severity = NotificationSeverity.Info
            });
            return true;
        }

        // ── Weekly tick ───────────────────────────────────────────────────────
        public void OnWeekPassed(StudioStats stats)
        {
            foreach (var node in _nodes.Values.Where(n => n.IsInProgress).ToList())
            {
                var nodeData = GameDatabase.Instance.GetResearch(node.NodeId);
                if (nodeData == null) continue;

                node.WeeksInvested += 1f;

                if (node.WeeksInvested >= nodeData.WeeksRequired)
                    CompleteResearch(node, nodeData);
            }

            // Re-evaluate locked nodes to see if they've become available
            RefreshAvailability(stats);
        }

        // ── Completion ────────────────────────────────────────────────────────
        private void CompleteResearch(ResearchNode node, ResearchNodeData nodeData)
        {
            node.Status = ResearchStatus.Completed;

            // Free the assigned employee
            var emp = GameManager.Instance.Staff.GetEmployee(node.AssignedEmployeeId);
            if (emp != null && emp.AssignedTaskId == node.NodeId)
            {
                emp.Status         = EmployeeStatus.Idle;
                emp.AssignedTaskId = null;
            }
            node.AssignedEmployeeId = null;

            // Apply effects
            var studio = GameManager.Instance.Studio;
            foreach (var genreId in nodeData.UnlocksGenreIds)
                studio.UnlockGenre(genreId);
            foreach (var platformId in nodeData.UnlocksPlatformIds)
                studio.UnlockPlatform(platformId);
            foreach (var staffRoleId in nodeData.UnlocksStaffRoleIds)
                studio.UnlockStaffRole(staffRoleId);

            if (!studio.Stats.CompletedResearchIds.Contains(node.NodeId))
                studio.Stats.CompletedResearchIds.Add(node.NodeId);

            GameEventBus.Publish(new ResearchCompletedEvent
            {
                NodeId      = node.NodeId,
                DisplayName = nodeData.DisplayName
            });
            GameEventBus.Publish(new NotificationEvent
            {
                Message  = $"Research complete: {nodeData.DisplayName}! {nodeData.EffectSummary}",
                Severity = NotificationSeverity.Success
            });
        }

        // ── Instant-complete (via event system) ───────────────────────────────
        public void ForceComplete(string nodeId)
        {
            if (!_nodes.TryGetValue(nodeId, out var node)) return;
            if (node.IsCompleted) return;
            var nodeData = GameDatabase.Instance.GetResearch(nodeId);
            if (nodeData == null) return;
            node.Status = ResearchStatus.InProgress;
            CompleteResearch(node, nodeData);
        }

        // ── Availability refresh ──────────────────────────────────────────────
        public void RefreshAvailability(StudioStats stats)
        {
            foreach (var kv in _nodes)
            {
                var node     = kv.Value;
                var nodeData = GameDatabase.Instance.GetResearch(kv.Key);
                if (nodeData == null || node.Status != ResearchStatus.Locked) continue;

                bool prereqsMet = nodeData.PrerequisiteNodeIds == null
                               || nodeData.PrerequisiteNodeIds.All(pid => IsCompleted(pid));
                bool repMet     = stats.Reputation >= nodeData.MinReputation;

                if (prereqsMet && repMet)
                    node.Status = ResearchStatus.Available;
            }
        }

        // ── Passive income sum ────────────────────────────────────────────────
        public float GetTotalPassiveIncome()
        {
            float total = 0f;
            foreach (var node in _nodes.Values.Where(n => n.IsCompleted))
            {
                var data = GameDatabase.Instance.GetResearch(node.NodeId);
                if (data != null) total += data.PassiveIncomePerWeek;
            }
            return total;
        }

        // ── Quality bonus sum ─────────────────────────────────────────────────
        public float GetTotalQualityBonus()
        {
            float total = 0f;
            foreach (var node in _nodes.Values.Where(n => n.IsCompleted))
            {
                var data = GameDatabase.Instance.GetResearch(node.NodeId);
                if (data != null) total += data.QualityBonus;
            }
            return total;
        }
    }
}
