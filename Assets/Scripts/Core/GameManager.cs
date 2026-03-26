using UnityEngine;
using GameDevStudio.Data;
using GameDevStudio.Models;
using GameDevStudio.Events;

namespace GameDevStudio.Core
{
    /// <summary>
    /// Root singleton that wires together all game systems.
    /// Attach to a persistent GameObject in the Bootstrap scene.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        // ── Singleton ─────────────────────────────────────────────────────────
        public static GameManager Instance { get; private set; }

        // ── Sub-managers (accessible via GameManager.Instance.XXX) ────────────
        public TimeManager     Time     { get; private set; }
        public StudioManager   Studio   { get; private set; }
        public ProjectManager  Projects { get; private set; }
        public StaffManager    Staff    { get; private set; }
        public ResearchManager Research { get; private set; }
        public EventManager    Events   { get; private set; }
        public SaveSystem      Save     { get; private set; }

        // ── Inspector ─────────────────────────────────────────────────────────
        [Header("Settings")]
        [Tooltip("Seconds per in-game week in real time.")]
        public float SecondsPerWeek = 5f;
        [Tooltip("Studio name used when starting a new game.")]
        public string DefaultStudioName = "Indie Dreams Studio";

        // ── State ─────────────────────────────────────────────────────────────
        public  bool   IsPaused   { get; private set; }
        private bool   _isRunning;

        // ── Unity lifecycle ───────────────────────────────────────────────────
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitialiseSystems();
        }

        private void Start()
        {
            if (Save.HasSaveFile())
                Save.Load();
            else
                StartNewGame();
        }

        private void Update()
        {
            if (!_isRunning || IsPaused) return;
            Time.Tick(UnityEngine.Time.deltaTime);
        }

        // ── Initialisation ────────────────────────────────────────────────────
        private void InitialiseSystems()
        {
            GameDatabase.Instance.LoadAll();

            Save     = new SaveSystem();
            Time     = new TimeManager(SecondsPerWeek);
            Studio   = new StudioManager();
            Projects = new ProjectManager();
            Staff    = new StaffManager();
            Research = new ResearchManager();
            Events   = new EventManager();

            // Wire up cross-system subscriptions
            GameEventBus.Subscribe<WeekPassedEvent>(OnWeekPassed);
        }

        private void StartNewGame()
        {
            Studio.Initialise(DefaultStudioName);
            Research.Initialise();
            _isRunning = true;

            GameEventBus.Publish(new NotificationEvent
            {
                Message  = $"Welcome to {DefaultStudioName}! Your journey begins.",
                Severity = NotificationSeverity.Info
            });
        }

        // ── Week tick ─────────────────────────────────────────────────────────
        private void OnWeekPassed(WeekPassedEvent e)
        {
            Staff.OnWeekPassed(Studio.Stats);
            Projects.OnWeekPassed(Studio.Stats);
            Research.OnWeekPassed(Studio.Stats);
            Events.OnWeekPassed(Studio.Stats, e);
        }

        // ── Public API ────────────────────────────────────────────────────────
        public void SetPaused(bool paused)
        {
            IsPaused = paused;
        }

        public void TogglePause() => SetPaused(!IsPaused);

        private void OnDestroy()
        {
            GameEventBus.Unsubscribe<WeekPassedEvent>(OnWeekPassed);
        }
    }
}
