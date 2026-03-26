using System;
using System.Collections.Generic;

namespace GameDevStudio.Models
{
    /// <summary>
    /// Represents a single employee working at the studio.
    /// </summary>
    [Serializable]
    public class Employee
    {
        // ── Identity ──────────────────────────────────────────────────────────
        public string Id;
        public string Name;
        /// <summary>References a StaffRoleData.RoleId</summary>
        public string RoleId;

        // ── Stats (0-100) ─────────────────────────────────────────────────────
        public float Programming;
        public float Art;
        public float Design;
        public float Testing;
        public float Management;

        // ── Employment ────────────────────────────────────────────────────────
        public float WeeklySalary;
        /// <summary>Months employed at this studio.</summary>
        public int   Tenure;
        /// <summary>0=Idle, 1=Working on project, 2=Researching, 3=Training</summary>
        public EmployeeStatus Status = EmployeeStatus.Idle;
        /// <summary>Id of the project or research task this employee is assigned to.</summary>
        public string AssignedTaskId;

        // ── Morale & Efficiency ───────────────────────────────────────────────
        /// <summary>0-100. Low morale reduces output quality.</summary>
        public float Morale = 80f;
        /// <summary>0-100. Skill grows with experience and training.</summary>
        public float Experience;

        // ── Traits ────────────────────────────────────────────────────────────
        public List<string> Traits = new List<string>();

        // ── Computed helpers ──────────────────────────────────────────────────
        /// <summary>
        /// Returns an efficiency multiplier (0.5 – 1.5) based on morale.
        /// </summary>
        public float MoraleMultiplier =>
            0.5f + (Morale / 100f);

        /// <summary>
        /// Composite "contribution score" weighted by role and skills.
        /// </summary>
        public float OverallSkill =>
            (Programming + Art + Design + Testing + Management) / 5f;

        public void ApplyWeeklyTick()
        {
            // Slight morale decay each week — offset by bonuses, events, etc.
            Morale = Math.Max(0f, Morale - 0.5f);
            Morale = Math.Min(100f, Morale);
            Tenure++;
        }

        public string GetStatusLabel() => Status switch
        {
            EmployeeStatus.Idle       => "Idle",
            EmployeeStatus.Working    => "Working",
            EmployeeStatus.Researching=> "Researching",
            EmployeeStatus.Training   => "Training",
            _                         => "Unknown"
        };
    }

    public enum EmployeeStatus
    {
        Idle,
        Working,
        Researching,
        Training
    }
}
