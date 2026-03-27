using UnityEngine;
using GameDevStudio.Events;

namespace GameDevStudio.UI
{
    /// <summary>
    /// Root UI controller.  Manages which "screen" (panel) is currently visible.
    /// Attach to a canvas root GameObject in the Main scene.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        // ── Singleton ─────────────────────────────────────────────────────────
        public static UIManager Instance { get; private set; }

        // ── Inspector references ──────────────────────────────────────────────
        [Header("Panels")]
        public GameObject MainHUDPanel;
        public GameObject ProjectsPanel;
        public GameObject StaffPanel;
        public GameObject ResearchPanel;
        public GameObject EventPanel;
        public GameObject NotificationPanel;
        public GameObject NewProjectPanel;
        public GameObject HirePanel;
        public GameObject SettingsPanel;

        [Header("Sub-controllers")]
        public MainHUDUI       MainHUD;
        public ProjectUI       ProjectsUI;
        public StaffUI         StaffPanelUI;
        public ResearchUI      ResearchPanelUI;
        public EventUI         EventPanelUI;
        public NotificationUI  NotificationPanelUI;

        // ── Unity lifecycle ───────────────────────────────────────────────────
        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            GameEventBus.Subscribe<RandomEventFiredEvent>(OnRandomEventFired);
            ShowMainHUD();
        }

        private void OnDestroy()
        {
            GameEventBus.Unsubscribe<RandomEventFiredEvent>(OnRandomEventFired);
        }

        // ── Navigation ────────────────────────────────────────────────────────
        public void ShowMainHUD()    => ShowPanel(MainHUDPanel);
        public void ShowProjects()   => ShowPanel(ProjectsPanel);
        public void ShowStaff()      => ShowPanel(StaffPanel);
        public void ShowResearch()   => ShowPanel(ResearchPanel);
        public void ShowNewProject() => ShowPanel(NewProjectPanel);
        public void ShowHireScreen() => ShowPanel(HirePanel);
        public void ShowSettings()   => ShowPanel(SettingsPanel);

        private void ShowPanel(GameObject target)
        {
            // Hide per-screen panels then show the target.
            // MainHUDPanel is a persistent navigation overlay — never hidden.
            // NotificationPanel and EventPanel are overlays — managed separately.
            GameObject[] screens =
            {
                ProjectsPanel, StaffPanel, ResearchPanel,
                NewProjectPanel, HirePanel, SettingsPanel
            };
            foreach (var p in screens)
                if (p != null) p.SetActive(false);

            if (target != null) target.SetActive(true);
        }

        public void ShowEventPanel()
        {
            if (EventPanel != null) EventPanel.SetActive(true);
            EventPanelUI?.RefreshEvent();
        }

        public void HideEventPanel()
        {
            if (EventPanel != null) EventPanel.SetActive(false);
        }

        // ── Event listeners ───────────────────────────────────────────────────
        private void OnRandomEventFired(RandomEventFiredEvent e)
        {
            // Show the event panel when a new event fires
            ShowEventPanel();
        }
    }
}
