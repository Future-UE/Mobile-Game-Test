using GameDevStudio.Events;

namespace GameDevStudio.Core
{
    /// <summary>
    /// Tracks real-time elapsed seconds and converts them into in-game weeks,
    /// firing <see cref="WeekPassedEvent"/> at each boundary.
    /// </summary>
    public class TimeManager
    {
        // ── Config ────────────────────────────────────────────────────────────
        private float _secondsPerWeek;

        // ── State ─────────────────────────────────────────────────────────────
        private float _accumulator;
        private int   _totalWeeks;

        public int CurrentWeek  { get; private set; } = 1;  // 1-4
        public int CurrentMonth { get; private set; } = 1;  // 1-12
        public int CurrentYear  { get; private set; } = 1;

        // ── Constructor ───────────────────────────────────────────────────────
        public TimeManager(float secondsPerWeek)
        {
            _secondsPerWeek = secondsPerWeek;
        }

        // ── Public API ────────────────────────────────────────────────────────
        public void SetSecondsPerWeek(float seconds) => _secondsPerWeek = seconds;

        /// <summary>
        /// Call from MonoBehaviour.Update with Time.deltaTime.
        /// </summary>
        public void Tick(float deltaTime)
        {
            _accumulator += deltaTime;
            if (_accumulator < _secondsPerWeek) return;

            _accumulator -= _secondsPerWeek;
            AdvanceWeek();
        }

        /// <summary>Manually advance one week (useful for tests / cheat button).</summary>
        public void ForceAdvanceWeek() => AdvanceWeek();

        // ── Internal ──────────────────────────────────────────────────────────
        private void AdvanceWeek()
        {
            _totalWeeks++;
            CurrentWeek++;

            if (CurrentWeek > 4)
            {
                CurrentWeek = 1;
                CurrentMonth++;

                if (CurrentMonth > 12)
                {
                    CurrentMonth = 1;
                    CurrentYear++;
                }
            }

            GameEventBus.Publish(new WeekPassedEvent
            {
                Week  = CurrentWeek,
                Month = CurrentMonth,
                Year  = CurrentYear
            });
        }

        // ── Restore from save ─────────────────────────────────────────────────
        public void RestoreState(int week, int month, int year)
        {
            CurrentWeek  = week;
            CurrentMonth = month;
            CurrentYear  = year;
        }

        public string GetDateString() =>
            $"Year {CurrentYear}, Month {CurrentMonth}, Week {CurrentWeek}";
    }
}
