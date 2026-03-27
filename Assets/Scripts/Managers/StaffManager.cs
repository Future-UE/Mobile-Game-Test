using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GameDevStudio.Data;
using GameDevStudio.Events;
using GameDevStudio.Models;

namespace GameDevStudio.Core
{
    /// <summary>
    /// Manages hiring, firing, training, and weekly processing of all employees.
    /// </summary>
    public class StaffManager
    {
        private readonly List<Employee> _employees = new();

        // ── Name pools for procedural generation ──────────────────────────────
        private static readonly string[] FirstNames =
        {
            "Alex","Jordan","Casey","Morgan","Taylor","Sam","Riley","Drew","Parker","Quinn",
            "Blake","Cameron","Devon","Emery","Finley","Hayden","Jamie","Kendall","Logan","Micah"
        };
        private static readonly string[] LastNames =
        {
            "Smith","Jones","Lee","Brown","Davis","Wilson","Miller","Moore","Taylor","Anderson",
            "Thomas","Jackson","White","Harris","Martin","Thompson","Garcia","Martinez","Robinson","Clark"
        };

        // ── Restore from save ─────────────────────────────────────────────────
        public void RestoreEmployees(Employee[] saved)
        {
            _employees.Clear();
            if (saved != null) _employees.AddRange(saved);
        }

        // ── Queries ───────────────────────────────────────────────────────────
        public Employee[]             GetAllEmployees()      => _employees.ToArray();
        public IReadOnlyList<Employee>AllEmployees           => _employees;
        public Employee               GetEmployee(string id) =>
            _employees.FirstOrDefault(e => e.Id == id);

        public IEnumerable<Employee> GetIdleEmployees() =>
            _employees.Where(e => e.Status == EmployeeStatus.Idle);

        // ── Hiring ────────────────────────────────────────────────────────────
        /// <summary>
        /// Generates a pool of candidate employees for the player to choose from.
        /// </summary>
        public List<Employee> GenerateCandidates(string roleId, int count = 3)
        {
            var roleData = GameDatabase.Instance.GetStaffRole(roleId);
            if (roleData == null)
            {
                Debug.LogWarning($"[StaffManager] Unknown role id: {roleId}");
                return new List<Employee>();
            }

            var rng = new System.Random();
            var candidates = new List<Employee>(count);
            for (int i = 0; i < count; i++)
                candidates.Add(GenerateEmployee(roleData, rng));

            return candidates;
        }

        private static Employee GenerateEmployee(StaffRoleData roleData, System.Random rng)
        {
            float SkillRoll(float @base) =>
                Mathf.Clamp(@base + (float)(rng.NextDouble() * 30 - 10), 0f, 100f);

            float salaryVariance = roleData.BaseWeeklySalary * roleData.SalaryVariance;
            float salary = roleData.BaseWeeklySalary
                         + (float)(rng.NextDouble() * 2 - 1) * salaryVariance;

            string firstName = FirstNames[rng.Next(FirstNames.Length)];
            string lastName  = LastNames[rng.Next(LastNames.Length)];

            return new Employee
            {
                Id           = Guid.NewGuid().ToString(),
                Name         = $"{firstName} {lastName}",
                RoleId       = roleData.RoleId,
                Programming  = SkillRoll(roleData.BaseProgramming),
                Art          = SkillRoll(roleData.BaseArt),
                Design       = SkillRoll(roleData.BaseDesign),
                Testing      = SkillRoll(roleData.BaseTesting),
                Management   = SkillRoll(roleData.BaseManagement),
                WeeklySalary = Mathf.Max(100f, salary),
                Morale       = 80f + (float)(rng.NextDouble() * 20 - 10),
                Experience   = 0f
            };
        }

        /// <summary>
        /// Hires a candidate.  Returns false if the studio is at capacity or
        /// cannot afford the first week's salary.
        /// </summary>
        public bool HireEmployee(Employee candidate)
        {
            var studio = GameManager.Instance.Studio;

            if (_employees.Count >= studio.Stats.MaxStaff)
            {
                GameEventBus.Publish(new NotificationEvent
                {
                    Message  = "Studio is at full capacity. Upgrade your office to hire more staff.",
                    Severity = NotificationSeverity.Warning
                });
                return false;
            }

            if (!studio.TrySpend(candidate.WeeklySalary))
            {
                GameEventBus.Publish(new NotificationEvent
                {
                    Message  = $"Not enough money to hire {candidate.Name}.",
                    Severity = NotificationSeverity.Warning
                });
                return false;
            }

            _employees.Add(candidate);
            GameEventBus.Publish(new EmployeeHiredEvent
            {
                EmployeeId = candidate.Id,
                Name       = candidate.Name,
                RoleId     = candidate.RoleId
            });
            GameEventBus.Publish(new NotificationEvent
            {
                Message  = $"{candidate.Name} has joined the studio!",
                Severity = NotificationSeverity.Success
            });
            return true;
        }

        // ── Firing ────────────────────────────────────────────────────────────
        public void FireEmployee(string employeeId)
        {
            var emp = GetEmployee(employeeId);
            if (emp == null) return;

            // Remove from any assigned project
            if (emp.AssignedTaskId != null)
                GameManager.Instance.Projects.UnassignEmployee(emp.AssignedTaskId, employeeId);

            _employees.Remove(emp);
            GameEventBus.Publish(new EmployeeFiredEvent
            {
                EmployeeId = employeeId,
                Name       = emp.Name
            });
            GameEventBus.Publish(new NotificationEvent
            {
                Message  = $"{emp.Name} has left the studio.",
                Severity = NotificationSeverity.Info
            });
        }

        // ── Training ──────────────────────────────────────────────────────────
        /// <summary>
        /// Sends an employee to a training session costing <paramref name="cost"/>.
        /// Returns false if the studio cannot afford it.
        /// </summary>
        public bool TrainEmployee(string employeeId, TrainingFocus focus, float cost)
        {
            var emp = GetEmployee(employeeId);
            if (emp == null) return false;
            if (!GameManager.Instance.Studio.TrySpend(cost)) return false;

            float gain = 10f + UnityEngine.Random.Range(-2f, 5f);
            switch (focus)
            {
                case TrainingFocus.Programming: emp.Programming = Mathf.Min(100f, emp.Programming + gain); break;
                case TrainingFocus.Art:         emp.Art         = Mathf.Min(100f, emp.Art         + gain); break;
                case TrainingFocus.Design:      emp.Design      = Mathf.Min(100f, emp.Design      + gain); break;
                case TrainingFocus.Testing:     emp.Testing     = Mathf.Min(100f, emp.Testing     + gain); break;
                case TrainingFocus.Management:  emp.Management  = Mathf.Min(100f, emp.Management  + gain); break;
            }
            emp.Morale = Mathf.Min(100f, emp.Morale + 5f);

            GameEventBus.Publish(new NotificationEvent
            {
                Message  = $"{emp.Name} completed {focus} training (+{gain:F0} {focus}).",
                Severity = NotificationSeverity.Success
            });
            return true;
        }

        // ── Weekly tick ───────────────────────────────────────────────────────
        public void OnWeekPassed(StudioStats stats)
        {
            float totalSalary = 0f;
            foreach (var emp in _employees)
            {
                emp.ApplyWeeklyTick();
                emp.Experience = Math.Min(100f, emp.Experience + 0.5f);
                totalSalary += emp.WeeklySalary;
            }

            // Pay weekly salaries
            GameManager.Instance.Studio.AddMoney(-totalSalary);

            // Passive income from completed research
            float passiveIncome = GameManager.Instance.Research.GetTotalPassiveIncome();
            if (passiveIncome > 0f)
                GameManager.Instance.Studio.AddMoney(passiveIncome);
        }

        // ── Morale boost ─────────────────────────────────────────────────────
        public void BoostAllMorale(float amount)
        {
            foreach (var emp in _employees)
                emp.Morale = Mathf.Min(100f, emp.Morale + amount);
        }
    }

    public enum TrainingFocus
    {
        Programming,
        Art,
        Design,
        Testing,
        Management
    }
}
