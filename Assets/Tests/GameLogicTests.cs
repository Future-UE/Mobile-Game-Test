using NUnit.Framework;
using GameDevStudio.Models;
using GameDevStudio.Events;

namespace GameDevStudio.Tests
{
    /// <summary>
    /// Edit-mode unit tests for pure-logic model classes.
    /// Run via Unity Test Runner (Window → General → Test Runner → EditMode).
    /// These tests have no dependencies on MonoBehaviour or scene objects.
    /// </summary>
    public class StudioStatsTests
    {
        [Test]
        public void AddMoney_IncreasesBalance()
        {
            var stats = new StudioStats { Money = 1000f };
            stats.AddMoney(500f);
            Assert.AreEqual(1500f, stats.Money);
        }

        [Test]
        public void AddMoney_NegativeAmount_DecreasesBalance()
        {
            var stats = new StudioStats { Money = 1000f };
            stats.AddMoney(-300f);
            Assert.AreEqual(700f, stats.Money);
        }

        [Test]
        public void CanAfford_ReturnsFalseWhenInsufficientFunds()
        {
            var stats = new StudioStats { Money = 100f };
            Assert.IsFalse(stats.CanAfford(500f));
        }

        [Test]
        public void CanAfford_ReturnsTrueWhenSufficientFunds()
        {
            var stats = new StudioStats { Money = 1000f };
            Assert.IsTrue(stats.CanAfford(999f));
        }

        [Test]
        public void ClampReputation_ClampsAbove100()
        {
            var stats = new StudioStats { Reputation = 150f };
            stats.ClampReputation();
            Assert.AreEqual(100f, stats.Reputation);
        }

        [Test]
        public void ClampReputation_ClampsBelow0()
        {
            var stats = new StudioStats { Reputation = -10f };
            stats.ClampReputation();
            Assert.AreEqual(0f, stats.Reputation);
        }

        [Test]
        public void GetFormattedMoney_ForMillions()
        {
            var stats = new StudioStats { Money = 2_500_000f };
            string result = stats.GetFormattedMoney();
            StringAssert.StartsWith("$", result);
            StringAssert.EndsWith("M", result);
        }

        [Test]
        public void GetFormattedMoney_ForThousands()
        {
            var stats = new StudioStats { Money = 15_000f };
            string result = stats.GetFormattedMoney();
            StringAssert.EndsWith("K", result);
        }

        [Test]
        public void MaxStaff_IncreasesWithOfficeTier()
        {
            var stats = new StudioStats { OfficeTier = 0 };
            int tier0 = stats.MaxStaff;
            stats.OfficeTier = 1;
            int tier1 = stats.MaxStaff;
            Assert.Greater(tier1, tier0);
        }

        [Test]
        public void TotalEarned_TracksPositiveDeltas()
        {
            var stats = new StudioStats { Money = 0f };
            stats.AddMoney(1000f);
            stats.AddMoney(500f);
            Assert.AreEqual(1500f, stats.TotalEarned);
        }

        [Test]
        public void TotalSpent_TracksNegativeDeltas()
        {
            var stats = new StudioStats { Money = 5000f };
            stats.AddMoney(-1000f);
            Assert.AreEqual(1000f, stats.TotalSpent);
        }
    }

    public class EmployeeTests
    {
        [Test]
        public void MoraleMultiplier_Is1_5_AtFullMorale()
        {
            var emp = new Employee { Morale = 100f };
            Assert.AreEqual(1.5f, emp.MoraleMultiplier, 0.001f);
        }

        [Test]
        public void MoraleMultiplier_IsHalf_AtZeroMorale()
        {
            var emp = new Employee { Morale = 0f };
            Assert.AreEqual(0.5f, emp.MoraleMultiplier, 0.001f);
        }

        [Test]
        public void ApplyWeeklyTick_DecaysMorale()
        {
            var emp = new Employee { Morale = 80f };
            emp.ApplyWeeklyTick();
            Assert.Less(emp.Morale, 80f);
        }

        [Test]
        public void ApplyWeeklyTick_MoraleNeverGoesNegative()
        {
            var emp = new Employee { Morale = 0f };
            emp.ApplyWeeklyTick();
            Assert.GreaterOrEqual(emp.Morale, 0f);
        }

        [Test]
        public void ApplyWeeklyTick_IncrementsTenure()
        {
            var emp = new Employee { Tenure = 5 };
            emp.ApplyWeeklyTick();
            Assert.AreEqual(6, emp.Tenure);
        }

        [Test]
        public void OverallSkill_IsAverageOfFiveStats()
        {
            var emp = new Employee
            {
                Programming = 80f,
                Art         = 60f,
                Design      = 70f,
                Testing     = 50f,
                Management  = 40f
            };
            float expected = (80 + 60 + 70 + 50 + 40) / 5f;
            Assert.AreEqual(expected, emp.OverallSkill, 0.001f);
        }
    }

    public class GameProjectTests
    {
        [Test]
        public void DevelopmentProgress_IsZero_WhenNotStarted()
        {
            var project = new GameProject { PlannedWeeks = 12, WeeksSpent = 0 };
            Assert.AreEqual(0f, project.DevelopmentProgress, 0.001f);
        }

        [Test]
        public void DevelopmentProgress_IsOne_WhenComplete()
        {
            var project = new GameProject { PlannedWeeks = 12, WeeksSpent = 12 };
            Assert.AreEqual(1f, project.DevelopmentProgress, 0.001f);
        }

        [Test]
        public void GetReviewLabel_MasterpieceFor9Plus()
        {
            var project = new GameProject { ReviewScore = 9.5f };
            Assert.AreEqual("Masterpiece", project.GetReviewLabel());
        }

        [Test]
        public void GetReviewLabel_TerribleBelow5()
        {
            var project = new GameProject { ReviewScore = 3f };
            Assert.AreEqual("Terrible", project.GetReviewLabel());
        }
    }

    public class ResearchNodeTests
    {
        [Test]
        public void IsCompleted_IsTrueWhenStatusIsCompleted()
        {
            var node = new ResearchNode { Status = ResearchStatus.Completed };
            Assert.IsTrue(node.IsCompleted);
        }

        [Test]
        public void IsAvailable_IsTrueWhenStatusIsAvailable()
        {
            var node = new ResearchNode { Status = ResearchStatus.Available };
            Assert.IsTrue(node.IsAvailable);
        }

        [Test]
        public void IsInProgress_IsTrueWhenStatusIsInProgress()
        {
            var node = new ResearchNode { Status = ResearchStatus.InProgress };
            Assert.IsTrue(node.IsInProgress);
        }
    }

    public class GameEventBusTests
    {
        [Test]
        public void Subscribe_And_Publish_CallsListener()
        {
            bool called = false;
            GameEventBus.Subscribe<WeekTickEvent>(e => called = true);
            GameEventBus.Publish(new WeekTickEvent { Week = 1, Month = 1, Year = 1 });
            GameEventBus.Clear();
            Assert.IsTrue(called);
        }

        [Test]
        public void Unsubscribe_PreventsListenerFromBeingCalled()
        {
            bool called = false;
            void Handler(WeekTickEvent e) => called = true;

            GameEventBus.Subscribe<WeekTickEvent>(Handler);
            GameEventBus.Unsubscribe<WeekTickEvent>(Handler);
            GameEventBus.Publish(new WeekTickEvent { Week = 1, Month = 1, Year = 1 });
            GameEventBus.Clear();
            Assert.IsFalse(called);
        }

        [Test]
        public void Publish_PassesCorrectEventData()
        {
            int receivedYear = 0;
            GameEventBus.Subscribe<WeekTickEvent>(e => receivedYear = e.Year);
            GameEventBus.Publish(new WeekTickEvent { Week = 3, Month = 6, Year = 5 });
            GameEventBus.Clear();
            Assert.AreEqual(5, receivedYear);
        }

        [Test]
        public void MultipleListeners_AllReceiveEvent()
        {
            int count = 0;
            GameEventBus.Subscribe<MoneyChangedEvent>(_ => count++);
            GameEventBus.Subscribe<MoneyChangedEvent>(_ => count++);
            GameEventBus.Publish(new MoneyChangedEvent { Delta = 100f });
            GameEventBus.Clear();
            Assert.AreEqual(2, count);
        }

        [Test]
        public void Clear_RemovesAllListeners()
        {
            bool called = false;
            GameEventBus.Subscribe<WeekTickEvent>(_ => called = true);
            GameEventBus.Clear();
            GameEventBus.Publish(new WeekTickEvent { Week = 1 });
            Assert.IsFalse(called);
        }

        [Test]
        public void Subscribe_DuplicateDelegate_IsIgnored()
        {
            int callCount = 0;
            void Handler(WeekTickEvent e) => callCount++;

            GameEventBus.Subscribe<WeekTickEvent>(Handler);
            GameEventBus.Subscribe<WeekTickEvent>(Handler); // duplicate — should be ignored
            GameEventBus.Publish(new WeekTickEvent { Week = 1 });
            GameEventBus.Clear();

            // Listener should only be called once despite two Subscribe calls.
            Assert.AreEqual(1, callCount);
        }

        [Test]
        public void GetListenerCount_ReturnsCorrectCount()
        {
            GameEventBus.Subscribe<MoneyChangedEvent>(_ => { });
            GameEventBus.Subscribe<MoneyChangedEvent>(_ => { });
            int count = GameEventBus.GetListenerCount<MoneyChangedEvent>();
            GameEventBus.Clear();
            Assert.AreEqual(2, count);
        }

        [Test]
        public void GetListenerCount_ReturnsZero_WhenNoSubscribers()
        {
            // Use a distinct event type to avoid interference from other tests.
            Assert.AreEqual(0, GameEventBus.GetListenerCount<ReputationChangedEvent>());
        }

        [Test]
        public void DebugMode_CanBeToggled()
        {
            GameEventBus.DebugMode = true;
            Assert.IsTrue(GameEventBus.DebugMode);
            GameEventBus.DebugMode = false;
            Assert.IsFalse(GameEventBus.DebugMode);
        }
    }
}
