using System.Collections.Generic;
using System.Linq;
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
    /// Manages the Projects panel: listing active/completed projects,
    /// launching new project creation, and showing project detail.
    /// </summary>
    public class ProjectUI : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────────────────────────────────
        [Header("Project List")]
        public Transform  ProjectListContainer;
        public GameObject ProjectEntryPrefab;
        public Button     NewProjectButton;

        [Header("New Project Form")]
        public TMP_InputField ProjectTitleInput;
        public TMP_Dropdown   GenreDropdown;
        public TMP_Dropdown   PlatformDropdown;
        public TMP_InputField PlannedWeeksInput;
        public TMP_InputField BudgetInput;
        public Button         ConfirmNewProjectButton;
        public Button         CancelNewProjectButton;
        public TMP_Text       NewProjectErrorText;

        [Header("Project Detail")]
        public GameObject   DetailPanel;
        public TMP_Text     DetailTitleText;
        public TMP_Text     DetailGenrePlatformText;
        public TMP_Text     DetailPhaseText;
        public TMP_Text     DetailProgressText;
        public TMP_Text     DetailQualityText;
        public TMP_Text     DetailBugsText;
        public TMP_Text     DetailHypeText;
        public TMP_Text     DetailRevenueText;
        public TMP_Text     DetailReviewText;
        public Button       AssignStaffButton;
        public Button       MarketingButton;
        public Button       CloseDetailButton;

        // ── State ─────────────────────────────────────────────────────────────
        private GameProject _selectedProject;
        private List<string> _genreIds    = new();
        private List<string> _platformIds = new();

        // ── Unity lifecycle ───────────────────────────────────────────────────
        private void OnEnable()
        {
            GameEventBus.Subscribe<ProjectStartedEvent>(OnProjectStarted);
            GameEventBus.Subscribe<ProjectPhaseChangedEvent>(OnPhaseChanged);
            GameEventBus.Subscribe<ProjectReleasedEvent>(OnProjectReleased);
            RefreshProjectList();
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<ProjectStartedEvent>(OnProjectStarted);
            GameEventBus.Unsubscribe<ProjectPhaseChangedEvent>(OnPhaseChanged);
            GameEventBus.Unsubscribe<ProjectReleasedEvent>(OnProjectReleased);
        }

        private void Start()
        {
            NewProjectButton?.onClick.AddListener(OnNewProjectClicked);
            ConfirmNewProjectButton?.onClick.AddListener(OnConfirmNewProject);
            CancelNewProjectButton?.onClick.AddListener(OnCancelNewProject);
            CloseDetailButton?.onClick.AddListener(() => { if (DetailPanel) DetailPanel.SetActive(false); });
            MarketingButton?.onClick.AddListener(OnMarketingClicked);

            PopulateDropdowns();
        }

        // ── Event handlers ────────────────────────────────────────────────────
        private void OnProjectStarted(ProjectStartedEvent e)   => RefreshProjectList();
        private void OnPhaseChanged(ProjectPhaseChangedEvent e) => RefreshDetailIfSelected(e.ProjectId);
        private void OnProjectReleased(ProjectReleasedEvent e)  => RefreshProjectList();

        // ── Project list ──────────────────────────────────────────────────────
        private void RefreshProjectList()
        {
            if (ProjectListContainer == null) return;

            // Clear existing entries
            foreach (Transform child in ProjectListContainer)
                Destroy(child.gameObject);

            var projects = GameManager.Instance.Projects.AllProjects;
            foreach (var project in projects)
            {
                if (ProjectEntryPrefab == null) continue;
                var entry = Instantiate(ProjectEntryPrefab, ProjectListContainer);
                SetupProjectEntry(entry, project);
            }
        }

        private void SetupProjectEntry(GameObject entry, GameProject project)
        {
            // Assumes the prefab has TMP_Text children in specific order:
            // [0]=title, [1]=phase, [2]=progress
            var texts = entry.GetComponentsInChildren<TMP_Text>();
            if (texts.Length > 0) texts[0].text = project.Title;
            if (texts.Length > 1) texts[1].text = project.GetPhaseLabel();
            if (texts.Length > 2)
            {
                texts[2].text = project.IsReleased
                    ? $"★ {project.ReviewScore:F1}/10"
                    : $"{project.DevelopmentProgress:P0}";
            }

            var btn = entry.GetComponentInChildren<Button>();
            if (btn != null)
            {
                string id = project.Id;
                btn.onClick.AddListener(() => ShowProjectDetail(id));
            }
        }

        // ── Project detail ────────────────────────────────────────────────────
        public void ShowProjectDetail(string projectId)
        {
            _selectedProject = GameManager.Instance.Projects.GetProject(projectId);
            if (_selectedProject == null) return;
            if (DetailPanel != null) DetailPanel.SetActive(true);
            RefreshDetail();
        }

        private void RefreshDetailIfSelected(string projectId)
        {
            if (_selectedProject?.Id == projectId) RefreshDetail();
        }

        private void RefreshDetail()
        {
            if (_selectedProject == null) return;
            var p = _selectedProject;

            if (DetailTitleText         != null) DetailTitleText.text          = p.Title;
            if (DetailGenrePlatformText != null)
            {
                var genre    = GameDatabase.Instance.GetGenre(p.GenreId);
                var platform = GameDatabase.Instance.GetPlatform(p.PlatformId);
                DetailGenrePlatformText.text = $"{genre?.DisplayName ?? p.GenreId} | {platform?.DisplayName ?? p.PlatformId}";
            }
            if (DetailPhaseText    != null) DetailPhaseText.text    = p.GetPhaseLabel();
            if (DetailProgressText != null) DetailProgressText.text = p.IsReleased
                ? "Complete" : $"Phase: {p.PhaseProgress:P0}  |  Overall: {p.DevelopmentProgress:P0}";
            if (DetailQualityText  != null) DetailQualityText.text  = $"Quality: {p.QualityPoints:F0}";
            if (DetailBugsText     != null) DetailBugsText.text     = $"Bugs: {p.Bugs}";
            if (DetailHypeText     != null) DetailHypeText.text     = $"Hype: {p.Hype:F0}";
            if (DetailRevenueText  != null) DetailRevenueText.text  = $"Revenue: ${p.TotalRevenue:N0}";
            if (DetailReviewText   != null)
                DetailReviewText.text = p.IsReleased
                    ? $"Score: {p.ReviewScore:F1}/10 — {p.GetReviewLabel()}"
                    : "Not released yet";

            if (AssignStaffButton != null) AssignStaffButton.interactable = !p.IsReleased;
            if (MarketingButton   != null) MarketingButton.interactable   = !p.IsReleased;
        }

        // ── New project form ──────────────────────────────────────────────────
        private void PopulateDropdowns()
        {
            _genreIds.Clear();
            _platformIds.Clear();

            if (GenreDropdown != null)
            {
                GenreDropdown.ClearOptions();
                var options = new List<TMP_Dropdown.OptionData>();
                foreach (var kv in GameDatabase.Instance.Genres)
                {
                    if (GameManager.Instance.Studio.IsGenreUnlocked(kv.Key))
                    {
                        options.Add(new TMP_Dropdown.OptionData(kv.Value.DisplayName));
                        _genreIds.Add(kv.Key);
                    }
                }
                GenreDropdown.AddOptions(options);
            }

            if (PlatformDropdown != null)
            {
                PlatformDropdown.ClearOptions();
                var options = new List<TMP_Dropdown.OptionData>();
                foreach (var kv in GameDatabase.Instance.Platforms)
                {
                    if (GameManager.Instance.Studio.IsPlatformUnlocked(kv.Key))
                    {
                        options.Add(new TMP_Dropdown.OptionData(kv.Value.DisplayName));
                        _platformIds.Add(kv.Key);
                    }
                }
                PlatformDropdown.AddOptions(options);
            }
        }

        private void OnNewProjectClicked()
        {
            PopulateDropdowns();
            UIManager.Instance.ShowNewProject();
        }

        private void OnConfirmNewProject()
        {
            if (NewProjectErrorText != null) NewProjectErrorText.text = "";

            string title = ProjectTitleInput != null ? ProjectTitleInput.text.Trim() : "Untitled";
            if (string.IsNullOrEmpty(title))
            {
                if (NewProjectErrorText != null) NewProjectErrorText.text = "Please enter a project title.";
                return;
            }

            if (!int.TryParse(PlannedWeeksInput != null ? PlannedWeeksInput.text : "12", out int weeks) || weeks < 4)
            {
                if (NewProjectErrorText != null) NewProjectErrorText.text = "Planned weeks must be at least 4.";
                return;
            }

            if (!float.TryParse(BudgetInput != null ? BudgetInput.text : "5000", out float budget) || budget < 0)
            {
                if (NewProjectErrorText != null) NewProjectErrorText.text = "Invalid budget amount.";
                return;
            }

            int genreIdx    = GenreDropdown    != null ? GenreDropdown.value    : 0;
            int platformIdx = PlatformDropdown != null ? PlatformDropdown.value : 0;

            if (genreIdx >= _genreIds.Count || platformIdx >= _platformIds.Count)
            {
                if (NewProjectErrorText != null) NewProjectErrorText.text = "Select a genre and platform.";
                return;
            }

            var project = GameManager.Instance.Projects.CreateProject(
                title, _genreIds[genreIdx], _platformIds[platformIdx], weeks, budget);

            if (project != null)
                UIManager.Instance.ShowProjects();
        }

        private void OnCancelNewProject() => UIManager.Instance.ShowProjects();

        private void OnMarketingClicked()
        {
            if (_selectedProject == null) return;
            // Run a $5,000 campaign
            GameManager.Instance.Projects.RunMarketingCampaign(_selectedProject.Id, 5000f);
            RefreshDetail();
        }
    }
}
