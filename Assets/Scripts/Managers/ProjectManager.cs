using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GameDevStudio.Data;
using GameDevStudio.Events;
using GameDevStudio.Models;
using GameDevStudio.Utils;

namespace GameDevStudio.Core
{
    /// <summary>
    /// Handles creation, development simulation, and release of game projects.
    /// </summary>
    public class ProjectManager
    {
        private readonly List<GameProject> _projects = new();

        // ── Restore from save ─────────────────────────────────────────────────
        public void RestoreProjects(GameProject[] saved)
        {
            _projects.Clear();
            if (saved != null) _projects.AddRange(saved);
        }

        // ── Queries ───────────────────────────────────────────────────────────
        public GameProject[]              GetAllProjects()    => _projects.ToArray();
        public IReadOnlyList<GameProject> AllProjects         => _projects;
        public GameProject                GetProject(string id) =>
            _projects.FirstOrDefault(p => p.Id == id);

        public IEnumerable<GameProject> ActiveProjects =>
            _projects.Where(p => !p.IsReleased);

        // ── Project creation ──────────────────────────────────────────────────
        /// <summary>
        /// Creates a new project.  Returns null if the studio can't afford it
        /// or if the genre/platform are not unlocked.
        /// </summary>
        public GameProject CreateProject(string title, string genreId, string platformId,
                                         int plannedWeeks, float budget)
        {
            var studio = GameManager.Instance.Studio;
            if (!studio.IsGenreUnlocked(genreId))
            {
                Debug.LogWarning($"[ProjectManager] Genre {genreId} not unlocked.");
                return null;
            }
            if (!studio.IsPlatformUnlocked(platformId))
            {
                Debug.LogWarning($"[ProjectManager] Platform {platformId} not unlocked.");
                return null;
            }
            if (!studio.TrySpend(budget))
            {
                GameEventBus.Publish(new NotificationEvent
                {
                    Message  = "Not enough money to start this project.",
                    Severity = NotificationSeverity.Warning
                });
                return null;
            }

            var project = new GameProject
            {
                Id           = Guid.NewGuid().ToString(),
                Title        = title,
                GenreId      = genreId,
                PlatformId   = platformId,
                PlannedWeeks = plannedWeeks,
                Budget       = budget,
                Phase        = ProjectPhase.Concept,
                PhaseProgress= 0f
            };

            _projects.Add(project);
            GameEventBus.Publish(new ProjectStartedEvent
            {
                ProjectId    = project.Id,
                ProjectTitle = title
            });
            return project;
        }

        // ── Staff assignment ──────────────────────────────────────────────────
        public void AssignEmployee(string projectId, string employeeId)
        {
            var project = GetProject(projectId);
            if (project == null || project.IsReleased) return;
            if (!project.AssignedEmployeeIds.Contains(employeeId))
                project.AssignedEmployeeIds.Add(employeeId);

            var emp = GameManager.Instance.Staff.GetEmployee(employeeId);
            if (emp != null)
            {
                emp.Status       = EmployeeStatus.Working;
                emp.AssignedTaskId = projectId;
            }
        }

        public void UnassignEmployee(string projectId, string employeeId)
        {
            var project = GetProject(projectId);
            project?.AssignedEmployeeIds.Remove(employeeId);

            var emp = GameManager.Instance.Staff.GetEmployee(employeeId);
            if (emp != null && emp.AssignedTaskId == projectId)
            {
                emp.Status       = EmployeeStatus.Idle;
                emp.AssignedTaskId = null;
            }
        }

        // ── Weekly tick ───────────────────────────────────────────────────────
        public void OnWeekPassed(StudioStats stats)
        {
            foreach (var project in _projects.Where(p => !p.IsReleased).ToList())
                TickProject(project, stats);
        }

        private void TickProject(GameProject project, StudioStats stats)
        {
            // Gather assigned staff contributions
            float programming = 0, art = 0, design = 0, testing = 0;
            foreach (var empId in project.AssignedEmployeeIds)
            {
                var emp = GameManager.Instance.Staff.GetEmployee(empId);
                if (emp == null) continue;
                float m = emp.MoraleMultiplier;
                programming += emp.Programming * m;
                art         += emp.Art         * m;
                design      += emp.Design      * m;
                testing     += emp.Testing     * m;
            }

            // Apply database modifiers
            var genreData    = GameDatabase.Instance.GetGenre(project.GenreId);
            var platformData = GameDatabase.Instance.GetPlatform(project.PlatformId);

            float devMultiplier = 1f;
            if (genreData    != null) devMultiplier *= genreData.DevTimeMultiplier;
            if (platformData != null) devMultiplier *= platformData.DevEffortMultiplier;

            // Phase-specific logic
            switch (project.Phase)
            {
                case ProjectPhase.Concept:
                    TickConceptPhase(project, design, devMultiplier);
                    break;
                case ProjectPhase.PreProd:
                    TickPreProdPhase(project, programming + design, devMultiplier);
                    break;
                case ProjectPhase.Production:
                    TickProductionPhase(project, programming, art, design, devMultiplier, genreData);
                    break;
                case ProjectPhase.Testing:
                    TickTestingPhase(project, testing, devMultiplier);
                    break;
                case ProjectPhase.Polishing:
                    TickPolishingPhase(project, design + art, devMultiplier);
                    break;
            }

            project.WeeksSpent++;

            // Passive hype decay
            project.Hype = Math.Max(0f, project.Hype - 1f);
        }

        private static void TickConceptPhase(GameProject project, float design, float mult)
        {
            float progress = (design * 0.02f) / mult;
            project.PhaseProgress += progress;
            if (project.PhaseProgress >= 1f)
                AdvancePhase(project, ProjectPhase.PreProd);
        }

        private static void TickPreProdPhase(GameProject project, float combined, float mult)
        {
            float progress = (combined * 0.015f) / mult;
            project.PhaseProgress += progress;
            if (project.PhaseProgress >= 1f)
                AdvancePhase(project, ProjectPhase.Production);
        }

        private static void TickProductionPhase(GameProject project, float prog, float art,
                                                  float design, float mult, GenreData genre)
        {
            // Quality accumulates from weighted skills
            float qGain = prog * (genre?.ProgrammingWeight ?? 0.25f)
                        + art  * (genre?.ArtWeight          ?? 0.25f)
                        + design * (genre?.DesignWeight     ?? 0.25f);
            qGain /= mult;

            project.QualityPoints += qGain;

            // Bugs introduced proportional to code volume
            if (UnityEngine.Random.value < 0.3f)
                project.Bugs += UnityEngine.Random.Range(1, 4);

            float progress = (prog * 0.01f) / mult;
            project.PhaseProgress += progress;
            if (project.PhaseProgress >= 1f)
                AdvancePhase(project, ProjectPhase.Testing);
        }

        private static void TickTestingPhase(GameProject project, float testing, float mult)
        {
            // Reduce bugs
            int bugsFixed = Mathf.RoundToInt(testing * 0.5f / mult);
            project.Bugs = Math.Max(0, project.Bugs - bugsFixed);

            float progress = (testing * 0.02f) / mult;
            project.PhaseProgress += progress;
            if (project.PhaseProgress >= 1f)
                AdvancePhase(project, ProjectPhase.Polishing);
        }

        private static void TickPolishingPhase(GameProject project, float artDesign, float mult)
        {
            project.QualityPoints += artDesign * 0.1f / mult;

            float progress = (artDesign * 0.025f) / mult;
            project.PhaseProgress += progress;
            if (project.PhaseProgress >= 1f)
                ReleaseProject(project);
        }

        // ── Phase transitions ─────────────────────────────────────────────────
        private static void AdvancePhase(GameProject project, ProjectPhase next)
        {
            var prev = project.Phase;
            project.Phase         = next;
            project.PhaseProgress = 0f;

            GameEventBus.Publish(new ProjectPhaseChangedEvent
            {
                ProjectId = project.Id,
                OldPhase  = prev,
                NewPhase  = next
            });

            GameEventBus.Publish(new NotificationEvent
            {
                Message  = $"'{project.Title}' entered {next.ToString()} phase!",
                Severity = NotificationSeverity.Info
            });
        }

        // ── Release ───────────────────────────────────────────────────────────
        private static void ReleaseProject(GameProject project)
        {
            project.Phase      = ProjectPhase.Released;
            project.IsReleased = true;

            var studio  = GameManager.Instance.Studio;
            var genre   = GameDatabase.Instance.GetGenre(project.GenreId);
            var platform= GameDatabase.Instance.GetPlatform(project.PlatformId);

            // Calculate review score (0-10) from quality and bugs
            float maxQuality  = project.PlannedWeeks * 20f;
            float qualityRatio= Mathf.Clamp01(project.QualityPoints / maxQuality);
            float bugPenalty  = Mathf.Clamp01(project.Bugs / 50f) * 2f;
            float repBonus    = studio.Stats.Reputation / 100f;

            project.ReviewScore = Mathf.Clamp(
                qualityRatio * 8f + repBonus * 2f - bugPenalty,
                1f, 10f);

            // Sales calculation
            float baseUnits   = (project.ReviewScore / 10f) * 100_000f;
            float audienceMult= platform?.AudienceMultiplier ?? 1f;
            float marketAppeal= genre?.BaseMarketAppeal       ?? 5f;
            float hypeMult    = 1f + (project.Hype / 200f);

            project.UnitsSold = Mathf.RoundToInt(baseUnits * audienceMult
                                                 * (marketAppeal / 5f) * hypeMult);

            float pricePerUnit = genre?.BasePricePerUnit ?? 2.99f;
            float platformCut  = platform?.PlatformCut    ?? 0.30f;
            project.TotalRevenue = project.UnitsSold * pricePerUnit * (1f - platformCut);
            project.WeeksOnSale  = 0;

            studio.AddMoney(project.TotalRevenue);
            studio.AddReputation((project.ReviewScore - 5f) * 2f);
            studio.Stats.GamesReleased++;

            if (project.ReviewScore >= 8f)
                studio.Stats.Fans += Mathf.RoundToInt(project.UnitsSold * 0.01f);

            GameEventBus.Publish(new ProjectReleasedEvent
            {
                ProjectId    = project.Id,
                ProjectTitle = project.Title,
                ReviewScore  = project.ReviewScore,
                UnitsSold    = project.UnitsSold,
                Revenue      = project.TotalRevenue
            });

            GameEventBus.Publish(new NotificationEvent
            {
                Message  = $"'{project.Title}' released! Score: {project.ReviewScore:F1}/10 | Revenue: {project.TotalRevenue.ToMoneyString()}",
                Severity = project.ReviewScore >= 7f ? NotificationSeverity.Success : NotificationSeverity.Warning
            });
        }

        // ── Ongoing sales ─────────────────────────────────────────────────────
        /// <summary>Called by EventManager to drip post-release sales each week.</summary>
        public void TickReleasedSales(StudioStats stats)
        {
            foreach (var project in _projects.Where(p => p.IsReleased))
            {
                project.WeeksOnSale++;
                // Sales decay over time
                if (project.WeeksOnSale <= 52)
                {
                    float weeklySales = project.UnitsSold * 0.02f
                                      * Mathf.Exp(-0.03f * project.WeeksOnSale);
                    var genre    = GameDatabase.Instance.GetGenre(project.GenreId);
                    var platform = GameDatabase.Instance.GetPlatform(project.PlatformId);
                    float price  = genre?.BasePricePerUnit  ?? 2.99f;
                    float cut    = platform?.PlatformCut     ?? 0.30f;
                    float income = weeklySales * price * (1f - cut);
                    GameManager.Instance.Studio.AddMoney(income);
                    project.TotalRevenue += income;
                }
            }
        }

        // ── Marketing ─────────────────────────────────────────────────────────
        public void RunMarketingCampaign(string projectId, float cost)
        {
            var project = GetProject(projectId);
            if (project == null || project.IsReleased) return;
            if (!GameManager.Instance.Studio.TrySpend(cost)) return;
            project.Hype += cost / 1000f * 10f;
            project.Hype  = Mathf.Min(project.Hype, 100f);

            GameEventBus.Publish(new NotificationEvent
            {
                Message  = $"Marketing campaign for '{project.Title}' boosted hype to {project.Hype:F0}!",
                Severity = NotificationSeverity.Info
            });
        }
    }
}
