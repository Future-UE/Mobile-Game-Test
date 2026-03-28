using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameDevStudio.Core;
using GameDevStudio.Events;

namespace GameDevStudio.UI
{
    /// <summary>
    /// Drives the persistent HUD shown at the top/bottom of every screen,
    /// displaying money, reputation, date, and quick-action buttons.
    /// </summary>
    public class MainHUDUI : MonoBehaviour
    {
        // ── Inspector references ──────────────────────────────────────────────
        [Header("Labels")]
        public TMP_Text StudioNameText;
        public TMP_Text MoneyText;
        public TMP_Text ReputationText;
        public TMP_Text FansText;
        public TMP_Text DateText;
        public TMP_Text OfficeTierText;

        [Header("Buttons")]
        public Button ProjectsButton;
        public Button StaffButton;
        public Button ResearchButton;
        public Button PauseButton;
        public Button UpgradeOfficeButton;

        [Header("Pause icon texts")]
        public TMP_Text PauseButtonLabel;

        // ── Unity lifecycle ───────────────────────────────────────────────────
        private void Start()
        {
            ProjectsButton?.onClick.AddListener(UIManager.Instance.ShowProjects);
            StaffButton?.onClick.AddListener(UIManager.Instance.ShowStaff);
            ResearchButton?.onClick.AddListener(UIManager.Instance.ShowResearch);
            PauseButton?.onClick.AddListener(OnPauseToggle);
            UpgradeOfficeButton?.onClick.AddListener(OnUpgradeOffice);

            GameEventBus.Subscribe<WeekTickEvent>(OnWeekTick);
            GameEventBus.Subscribe<MoneyChangedEvent>(OnMoneyChanged);
            GameEventBus.Subscribe<ReputationChangedEvent>(OnRepChanged);

            RefreshAll();
        }

        private void OnDestroy()
        {
            GameEventBus.Unsubscribe<WeekTickEvent>(OnWeekTick);
            GameEventBus.Unsubscribe<MoneyChangedEvent>(OnMoneyChanged);
            GameEventBus.Unsubscribe<ReputationChangedEvent>(OnRepChanged);
        }

        // ── Event handlers ────────────────────────────────────────────────────
        private void OnWeekTick(WeekTickEvent e)      => RefreshAll();
        private void OnMoneyChanged(MoneyChangedEvent e)=> RefreshMoney();
        private void OnRepChanged(ReputationChangedEvent e) => RefreshReputation();

        // ── Button handlers ───────────────────────────────────────────────────
        private void OnPauseToggle()
        {
            GameManager.Instance.TogglePause();
            bool paused = GameManager.Instance.IsPaused;
            if (PauseButtonLabel != null)
                PauseButtonLabel.text = paused ? "▶ Resume" : "⏸ Pause";
        }

        private void OnUpgradeOffice()
        {
            GameManager.Instance.Studio.TryUpgradeOffice();
            RefreshAll();
        }

        // ── Refresh ───────────────────────────────────────────────────────────
        private void RefreshAll()
        {
            var stats = GameManager.Instance?.Studio?.Stats;
            if (stats == null) return;

            if (StudioNameText  != null) StudioNameText.text  = stats.StudioName;
            if (OfficeTierText  != null) OfficeTierText.text  = GameManager.Instance.Studio.GetOfficeTierName();
            if (FansText        != null) FansText.text        = $"Fans: {stats.Fans:N0}";
            if (DateText        != null) DateText.text        = GameManager.Instance.Time.GetDateString();

            RefreshMoney();
            RefreshReputation();

            bool canUpgrade = GameManager.Instance.Studio.CanUpgradeOffice();
            if (UpgradeOfficeButton != null)
            {
                UpgradeOfficeButton.interactable = canUpgrade;
                var label = UpgradeOfficeButton.GetComponentInChildren<TMP_Text>();
                if (label != null)
                {
                    float cost = GameManager.Instance.Studio.GetOfficeTierUpgradeCost();
                    label.text = canUpgrade
                        ? $"Upgrade Office\n${cost:N0}"
                        : "Max Office";
                }
            }
        }

        private void RefreshMoney()
        {
            if (MoneyText != null && GameManager.Instance?.Studio != null)
                MoneyText.text = GameManager.Instance.Studio.Stats.GetFormattedMoney();
        }

        private void RefreshReputation()
        {
            if (ReputationText != null && GameManager.Instance?.Studio != null)
                ReputationText.text = $"Rep: {GameManager.Instance.Studio.Stats.Reputation:F0}";
        }
    }
}
