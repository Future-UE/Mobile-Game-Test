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
    /// Manages the Research/Upgrades panel.
    /// Displays nodes grouped by category with progress indicators.
    /// </summary>
    public class ResearchUI : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────────────────────────────────
        [Header("Node List")]
        public Transform  NodeListContainer;
        public GameObject NodeEntryPrefab;
        public TMP_Dropdown CategoryFilter;

        [Header("Node Detail")]
        public GameObject DetailPanel;
        public TMP_Text   DetailNameText;
        public TMP_Text   DetailDescText;
        public TMP_Text   DetailCostText;
        public TMP_Text   DetailTimeText;
        public TMP_Text   DetailEffectText;
        public TMP_Text   DetailStatusText;
        public TMP_Dropdown AssignEmployeeDropdown;
        public Button     StartResearchButton;
        public Button     CloseDetailButton;

        // ── State ─────────────────────────────────────────────────────────────
        private ResearchNodeData _selectedNode;
        private List<string>     _assignableEmployeeIds = new();

        // ── Unity lifecycle ───────────────────────────────────────────────────
        private void Start()
        {
            CloseDetailButton?.onClick.AddListener(() => { if (DetailPanel) DetailPanel.SetActive(false); });
            StartResearchButton?.onClick.AddListener(OnStartResearch);
            CategoryFilter?.onValueChanged.AddListener(_ => RefreshNodeList());
            PopulateCategoryFilter();
        }

        private void OnEnable()
        {
            GameEventBus.Subscribe<ResearchCompletedEvent>(OnResearchCompleted);
            RefreshNodeList();
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<ResearchCompletedEvent>(OnResearchCompleted);
        }

        private void OnResearchCompleted(ResearchCompletedEvent e) => RefreshNodeList();

        // ── Category filter ───────────────────────────────────────────────────
        private void PopulateCategoryFilter()
        {
            if (CategoryFilter == null) return;
            CategoryFilter.ClearOptions();
            var categories = new List<string> { "All" };
            foreach (var kv in GameDatabase.Instance.ResearchNodes)
                if (!string.IsNullOrEmpty(kv.Value.Category) && !categories.Contains(kv.Value.Category))
                    categories.Add(kv.Value.Category);
            CategoryFilter.AddOptions(categories.Select(c => new TMP_Dropdown.OptionData(c)).ToList());
        }

        // ── Node list ─────────────────────────────────────────────────────────
        private void RefreshNodeList()
        {
            if (NodeListContainer == null) return;
            foreach (Transform child in NodeListContainer) Destroy(child.gameObject);

            string filterCategory = CategoryFilter != null && CategoryFilter.value > 0
                ? CategoryFilter.options[CategoryFilter.value].text
                : null;

            foreach (var kv in GameDatabase.Instance.ResearchNodes)
            {
                var data = kv.Value;
                if (filterCategory != null && data.Category != filterCategory) continue;

                var node = GameManager.Instance.Research.GetNode(data.NodeId);
                if (node == null) continue;

                if (NodeEntryPrefab == null) continue;
                var entry = Instantiate(NodeEntryPrefab, NodeListContainer);
                SetupNodeEntry(entry, data, node);
            }
        }

        private void SetupNodeEntry(GameObject entry, ResearchNodeData data, ResearchNode node)
        {
            var texts = entry.GetComponentsInChildren<TMP_Text>();
            if (texts.Length > 0) texts[0].text = data.DisplayName;
            if (texts.Length > 1) texts[1].text = data.Category;
            if (texts.Length > 2) texts[2].text = node.Status switch
            {
                ResearchStatus.Locked     => "🔒 Locked",
                ResearchStatus.Available  => $"Available — ${data.MoneyCost:N0}",
                ResearchStatus.InProgress => $"In Progress ({node.WeeksInvested:F0}/{data.WeeksRequired:F0} wks)",
                ResearchStatus.Completed  => "✔ Completed",
                _                         => ""
            };

            var btn = entry.GetComponentInChildren<Button>();
            if (btn != null)
            {
                btn.interactable = node.Status == ResearchStatus.Available;
                string id = data.NodeId;
                btn.onClick.AddListener(() => ShowNodeDetail(id));
            }
        }

        // ── Node detail ───────────────────────────────────────────────────────
        private void ShowNodeDetail(string nodeId)
        {
            _selectedNode = GameDatabase.Instance.GetResearch(nodeId);
            if (_selectedNode == null) return;
            if (DetailPanel != null) DetailPanel.SetActive(true);
            RefreshDetail();
        }

        private void RefreshDetail()
        {
            if (_selectedNode == null) return;
            var data = _selectedNode;
            var node = GameManager.Instance.Research.GetNode(data.NodeId);

            if (DetailNameText   != null) DetailNameText.text   = data.DisplayName;
            if (DetailDescText   != null) DetailDescText.text   = data.Description;
            if (DetailCostText   != null) DetailCostText.text   = $"Cost: ${data.MoneyCost:N0}";
            if (DetailTimeText   != null) DetailTimeText.text   = $"Duration: {data.WeeksRequired} weeks";
            if (DetailEffectText != null) DetailEffectText.text = data.EffectSummary;
            if (DetailStatusText != null) DetailStatusText.text = node?.Status.ToString() ?? "Unknown";

            // Populate assignable employees
            _assignableEmployeeIds.Clear();
            if (AssignEmployeeDropdown != null)
            {
                AssignEmployeeDropdown.ClearOptions();
                var options = new List<TMP_Dropdown.OptionData>();
                foreach (var emp in GameManager.Instance.Staff.GetIdleEmployees())
                {
                    options.Add(new TMP_Dropdown.OptionData(emp.Name));
                    _assignableEmployeeIds.Add(emp.Id);
                }
                AssignEmployeeDropdown.AddOptions(options);
            }

            bool canStart = node != null && node.IsAvailable && _assignableEmployeeIds.Count > 0;
            if (StartResearchButton != null) StartResearchButton.interactable = canStart;
        }

        private void OnStartResearch()
        {
            if (_selectedNode == null) return;
            int idx = AssignEmployeeDropdown != null ? AssignEmployeeDropdown.value : 0;
            if (idx >= _assignableEmployeeIds.Count) return;

            string empId = _assignableEmployeeIds[idx];
            bool started = GameManager.Instance.Research.StartResearch(_selectedNode.NodeId, empId);
            if (started)
            {
                if (DetailPanel) DetailPanel.SetActive(false);
                RefreshNodeList();
            }
        }
    }
}
