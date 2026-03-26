using UnityEngine;

namespace GameDevStudio.Data
{
    /// <summary>
    /// Defines a staff role (Programmer, Artist, Designer, QA, Manager, etc.).
    /// Create instances via Assets → Create → GameDevStudio → StaffRole.
    /// </summary>
    [CreateAssetMenu(fileName = "NewStaffRole", menuName = "GameDevStudio/StaffRole")]
    public class StaffRoleData : ScriptableObject
    {
        [Header("Identity")]
        public string RoleId;
        public string DisplayName;
        [TextArea(2, 4)]
        public string Description;

        [Header("Skill Contributions (per week, per employee)")]
        [Range(0f, 10f)] public float ProgrammingContribution;
        [Range(0f, 10f)] public float ArtContribution;
        [Range(0f, 10f)] public float DesignContribution;
        [Range(0f, 10f)] public float TestingContribution;
        [Range(0f, 10f)] public float ManagementContribution;

        [Header("Starting Stats")]
        [Range(0f, 100f)] public float BaseProgramming;
        [Range(0f, 100f)] public float BaseArt;
        [Range(0f, 100f)] public float BaseDesign;
        [Range(0f, 100f)] public float BaseTesting;
        [Range(0f, 100f)] public float BaseManagement;

        [Header("Hiring")]
        /// <summary>Base weekly salary for this role.</summary>
        public float BaseWeeklySalary = 500f;
        /// <summary>Variance added to salary on hire (0-1 fraction).</summary>
        [Range(0f, 0.5f)]
        public float SalaryVariance = 0.2f;
        /// <summary>How many of this role can be hired (0 = unlimited).</summary>
        public int MaxCount = 0;

        [Header("Requirements")]
        public string[] RequiredResearchIds;
        public bool StartsUnlocked = true;
    }
}
