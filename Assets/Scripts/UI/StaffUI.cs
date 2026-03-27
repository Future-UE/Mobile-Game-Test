using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameDevStudio.Core;
using GameDevStudio.Data;
using GameDevStudio.Events;
using GameDevStudio.Models;

namespace GameDevStudio.UI
{
    /// <summary>
    /// Manages the Staff panel: viewing employees, firing, training,
    /// and kicking off the hire flow.
    /// </summary>
    public class StaffUI : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────────────────────────────────
        [Header("Staff List")]
        public Transform  StaffListContainer;
        public GameObject StaffEntryPrefab;
        public Button     HireButton;
        public TMP_Text   StaffCountText;

        [Header("Hire Panel")]
        public Transform          CandidateListContainer;
        public GameObject         CandidateEntryPrefab;
        public TMP_Dropdown       RoleDropdown;
        public Button             RefreshCandidatesButton;
        public Button             CloseHirePanelButton;

        [Header("Employee Detail")]
        public GameObject  DetailPanel;
        public TMP_Text    DetailNameText;
        public TMP_Text    DetailRoleText;
        public TMP_Text    DetailStatsText;
        public TMP_Text    DetailMoraleText;
        public TMP_Text    DetailSalaryText;
        public TMP_Text    DetailStatusText;
        public Button      FireButton;
        public Button      TrainProgButton;
        public Button      TrainArtButton;
        public Button      TrainDesignButton;
        public Button      TrainTestButton;
        public Button      CloseDetailButton;

        // ── State ─────────────────────────────────────────────────────────────
        private Employee     _selectedEmployee;
        private List<string> _roleIds = new();

        // ── Unity lifecycle ───────────────────────────────────────────────────
        private void Start()
        {
            HireButton?.onClick.AddListener(OnHireClicked);
            RefreshCandidatesButton?.onClick.AddListener(RefreshCandidates);
            CloseHirePanelButton?.onClick.AddListener(UIManager.Instance.ShowStaff);
            CloseDetailButton?.onClick.AddListener(() => { if (DetailPanel) DetailPanel.SetActive(false); });
            FireButton?.onClick.AddListener(OnFireClicked);

            TrainProgButton?.onClick.AddListener(()   => TrainSelected(TrainingFocus.Programming));
            TrainArtButton?.onClick.AddListener(()    => TrainSelected(TrainingFocus.Art));
            TrainDesignButton?.onClick.AddListener(() => TrainSelected(TrainingFocus.Design));
            TrainTestButton?.onClick.AddListener(()   => TrainSelected(TrainingFocus.Testing));

            PopulateRoleDropdown();
        }

        private void OnEnable()
        {
            GameEventBus.Subscribe<EmployeeHiredEvent>(OnEmployeeHired);
            GameEventBus.Subscribe<EmployeeFiredEvent>(OnEmployeeFired);
            RefreshStaffList();
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<EmployeeHiredEvent>(OnEmployeeHired);
            GameEventBus.Unsubscribe<EmployeeFiredEvent>(OnEmployeeFired);
        }

        // ── Event handlers ────────────────────────────────────────────────────
        private void OnEmployeeHired(EmployeeHiredEvent e) => RefreshStaffList();
        private void OnEmployeeFired(EmployeeFiredEvent e) => RefreshStaffList();

        // ── Staff list ────────────────────────────────────────────────────────
        private void RefreshStaffList()
        {
            if (StaffListContainer == null) return;
            foreach (Transform child in StaffListContainer) Destroy(child.gameObject);

            var employees = GameManager.Instance.Staff.AllEmployees;
            var studio    = GameManager.Instance.Studio.Stats;

            if (StaffCountText != null)
                StaffCountText.text = $"Staff: {employees.Count} / {studio.MaxStaff}";

            foreach (var emp in employees)
            {
                if (StaffEntryPrefab == null) continue;
                var entry = Instantiate(StaffEntryPrefab, StaffListContainer);
                SetupStaffEntry(entry, emp);
            }
        }

        private void SetupStaffEntry(GameObject entry, Employee emp)
        {
            var texts = entry.GetComponentsInChildren<TMP_Text>();
            var role  = GameDatabase.Instance.GetStaffRole(emp.RoleId);
            if (texts.Length > 0) texts[0].text = emp.Name;
            if (texts.Length > 1) texts[1].text = role?.DisplayName ?? emp.RoleId;
            if (texts.Length > 2) texts[2].text = emp.GetStatusLabel();
            if (texts.Length > 3) texts[3].text = $"Morale: {emp.Morale:F0}";

            var btn = entry.GetComponentInChildren<Button>();
            if (btn != null)
            {
                string id = emp.Id;
                btn.onClick.AddListener(() => ShowEmployeeDetail(id));
            }
        }

        // ── Employee detail ───────────────────────────────────────────────────
        private void ShowEmployeeDetail(string employeeId)
        {
            _selectedEmployee = GameManager.Instance.Staff.GetEmployee(employeeId);
            if (_selectedEmployee == null) return;
            if (DetailPanel != null) DetailPanel.SetActive(true);
            RefreshDetail();
        }

        private void RefreshDetail()
        {
            var emp  = _selectedEmployee;
            if (emp == null) return;
            var role = GameDatabase.Instance.GetStaffRole(emp.RoleId);

            if (DetailNameText   != null) DetailNameText.text   = emp.Name;
            if (DetailRoleText   != null) DetailRoleText.text   = role?.DisplayName ?? emp.RoleId;
            if (DetailSalaryText != null) DetailSalaryText.text = $"Salary: ${emp.WeeklySalary:F0}/wk";
            if (DetailStatusText != null) DetailStatusText.text = emp.GetStatusLabel();
            if (DetailMoraleText != null) DetailMoraleText.text = $"Morale: {emp.Morale:F0}";
            if (DetailStatsText  != null)
                DetailStatsText.text =
                    $"Prog: {emp.Programming:F0}  Art: {emp.Art:F0}\n" +
                    $"Design: {emp.Design:F0}  Testing: {emp.Testing:F0}\n" +
                    $"Mgmt: {emp.Management:F0}  XP: {emp.Experience:F0}";
        }

        // ── Hire flow ─────────────────────────────────────────────────────────
        private void OnHireClicked()
        {
            PopulateRoleDropdown();
            UIManager.Instance.ShowHireScreen();
            RefreshCandidates();
        }

        private void PopulateRoleDropdown()
        {
            _roleIds.Clear();
            if (RoleDropdown == null) return;

            RoleDropdown.ClearOptions();
            var options = new List<TMP_Dropdown.OptionData>();
            foreach (var kv in GameDatabase.Instance.StaffRoles)
            {
                options.Add(new TMP_Dropdown.OptionData(kv.Value.DisplayName));
                _roleIds.Add(kv.Key);
            }
            RoleDropdown.AddOptions(options);
        }

        private void RefreshCandidates()
        {
            if (CandidateListContainer == null) return;
            foreach (Transform child in CandidateListContainer) Destroy(child.gameObject);

            int roleIdx = RoleDropdown != null ? RoleDropdown.value : 0;
            if (roleIdx >= _roleIds.Count) return;

            string roleId    = _roleIds[roleIdx];
            var    candidates = GameManager.Instance.Staff.GenerateCandidates(roleId, 3);

            foreach (var candidate in candidates)
            {
                if (CandidateEntryPrefab == null) continue;
                var entry = Instantiate(CandidateEntryPrefab, CandidateListContainer);
                SetupCandidateEntry(entry, candidate);
            }
        }

        private void SetupCandidateEntry(GameObject entry, Employee candidate)
        {
            var texts = entry.GetComponentsInChildren<TMP_Text>();
            if (texts.Length > 0) texts[0].text = candidate.Name;
            if (texts.Length > 1) texts[1].text =
                $"Prog:{candidate.Programming:F0} Art:{candidate.Art:F0} " +
                $"Des:{candidate.Design:F0} Test:{candidate.Testing:F0}";
            if (texts.Length > 2) texts[2].text = $"${candidate.WeeklySalary:F0}/wk";

            var btn = entry.GetComponentInChildren<Button>();
            if (btn != null)
            {
                var emp = candidate;
                btn.onClick.AddListener(() => OnHireCandidate(emp));
            }
        }

        private void OnHireCandidate(Employee candidate)
        {
            bool hired = GameManager.Instance.Staff.HireEmployee(candidate);
            if (hired) UIManager.Instance.ShowStaff();
        }

        // ── Fire / Train ──────────────────────────────────────────────────────
        private void OnFireClicked()
        {
            if (_selectedEmployee == null) return;
            GameManager.Instance.Staff.FireEmployee(_selectedEmployee.Id);
            if (DetailPanel) DetailPanel.SetActive(false);
            _selectedEmployee = null;
        }

        private void TrainSelected(TrainingFocus focus)
        {
            if (_selectedEmployee == null) return;
            bool ok = GameManager.Instance.Staff.TrainEmployee(_selectedEmployee.Id, focus, 2000f);
            if (ok) RefreshDetail();
        }
    }
}
