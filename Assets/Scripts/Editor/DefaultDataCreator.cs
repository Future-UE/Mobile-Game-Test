#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using GameDevStudio.Data;

namespace GameDevStudio.Editor
{
    /// <summary>
    /// Unity Editor tool that generates all default data assets so you can
    /// start playing immediately without manually creating ScriptableObjects.
    ///
    /// Run via:  Tools → GameDevStudio → Create Default Data Assets
    /// </summary>
    public static class DefaultDataCreator
    {
        private const string ResourcesRoot = "Assets/Resources/Data";

        [MenuItem("Tools/GameDevStudio/Create Default Data Assets")]
        public static void CreateAll()
        {
            CreateDefaultGenres();
            CreateDefaultPlatforms();
            CreateDefaultStaffRoles();
            CreateDefaultResearchNodes();
            CreateDefaultEvents();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[DefaultDataCreator] All default data assets created.");
        }

        // ── Genres ────────────────────────────────────────────────────────────
        private static void CreateDefaultGenres()
        {
            string folder = $"{ResourcesRoot}/Genres";
            EnsureFolder(folder);

            CreateGenre(folder, "Action",     "genre_action",      true,  null, 0f,  1.0f, 0.35f, 0.30f, 0.25f, 0.10f, 7f, 2.99f);
            CreateGenre(folder, "Puzzle",     "genre_puzzle",      true,  null, 0f,  0.8f, 0.20f, 0.20f, 0.50f, 0.10f, 6f, 1.99f);
            CreateGenre(folder, "RPG",        "genre_rpg",         false, new[]{"research_narrative_tools"}, 20f, 1.5f, 0.25f, 0.25f, 0.40f, 0.10f, 8f, 4.99f);
            CreateGenre(folder, "Simulation", "genre_simulation",  false, null, 30f, 1.8f, 0.45f, 0.15f, 0.25f, 0.15f, 7f, 5.99f);
            CreateGenre(folder, "Horror",     "genre_horror",      false, new[]{"research_narrative_tools"}, 25f, 1.3f, 0.25f, 0.35f, 0.30f, 0.10f, 7f, 3.99f);
            CreateGenre(folder, "Strategy",   "genre_strategy",    false, new[]{"research_ai_systems"}, 35f, 2.0f, 0.40f, 0.10f, 0.40f, 0.10f, 8f, 6.99f);
        }

        private static void CreateGenre(string folder, string name, string id,
            bool startsUnlocked, string[] required, float unlockRep,
            float devMult, float progW, float artW, float desW, float testW,
            float appeal, float price)
        {
            string path = $"{folder}/{name}.asset";
            if (AssetExists(path)) return;

            var asset = ScriptableObject.CreateInstance<GenreData>();
            asset.GenreId              = id;
            asset.DisplayName          = name;
            asset.StartsUnlocked       = startsUnlocked;
            asset.RequiredResearchIds  = required ?? System.Array.Empty<string>();
            asset.UnlockReputation     = unlockRep;
            asset.DevTimeMultiplier    = devMult;
            asset.ProgrammingWeight    = progW;
            asset.ArtWeight            = artW;
            asset.DesignWeight         = desW;
            asset.TestingWeight        = testW;
            asset.BaseMarketAppeal     = appeal;
            asset.BasePricePerUnit     = price;

            AssetDatabase.CreateAsset(asset, path);
        }

        // ── Platforms ─────────────────────────────────────────────────────────
        private static void CreateDefaultPlatforms()
        {
            string folder = $"{ResourcesRoot}/Platforms";
            EnsureFolder(folder);

            CreatePlatform(folder, "Mobile",  "platform_mobile",  true,  null, 0f,  1.0f, 1, 2.0f, 0.30f, 0.8f);
            CreatePlatform(folder, "PC",      "platform_pc",      false, new[]{"research_pc_tools"}, 15f, 1.2f, 2, 1.5f, 0.30f, 1.0f);
            CreatePlatform(folder, "Console", "platform_console", false, new[]{"research_console_dev_kit"}, 40f, 2.0f, 5, 1.8f, 0.30f, 2.5f);
        }

        private static void CreatePlatform(string folder, string name, string id,
            bool startsUnlocked, string[] required, float unlockRep,
            float devMult, int minTeam, float audienceMult, float cut, float costMult)
        {
            string path = $"{folder}/{name}.asset";
            if (AssetExists(path)) return;

            var asset = ScriptableObject.CreateInstance<PlatformData>();
            asset.PlatformId           = id;
            asset.DisplayName          = name;
            asset.StartsUnlocked       = startsUnlocked;
            asset.RequiredResearchIds  = required ?? System.Array.Empty<string>();
            asset.UnlockReputation     = unlockRep;
            asset.DevEffortMultiplier  = devMult;
            asset.MinTeamSize          = minTeam;
            asset.AudienceMultiplier   = audienceMult;
            asset.PlatformCut          = cut;
            asset.CostMultiplier       = costMult;

            AssetDatabase.CreateAsset(asset, path);
        }

        // ── Staff Roles ───────────────────────────────────────────────────────
        private static void CreateDefaultStaffRoles()
        {
            string folder = $"{ResourcesRoot}/StaffRoles";
            EnsureFolder(folder);

            // name, id, starts, required, salary, prog-contrib, art-c, des-c, test-c, mgmt-c, base stats
            CreateStaffRole(folder, "Programmer",    "role_programmer",  true,  null,  600f,  8, 0, 1, 2, 0,  60, 10, 20, 30, 10);
            CreateStaffRole(folder, "Artist",        "role_artist",      true,  null,  550f,  0, 8, 2, 0, 0,  10, 65, 25, 10,  5);
            CreateStaffRole(folder, "GameDesigner",  "role_designer",    true,  null,  575f,  1, 2, 8, 1, 1,  20, 30, 65, 20, 15);
            CreateStaffRole(folder, "QATester",      "role_qa",          true,  null,  450f,  2, 0, 1, 9, 0,  25,  5, 15, 70, 10);
            CreateStaffRole(folder, "Producer",      "role_manager",     false, new[]{"research_project_management"}, 700f, 1, 1, 2, 1, 9, 20, 10, 30, 20, 70);
            CreateStaffRole(folder, "SoundDesigner", "role_sound",       false, new[]{"research_audio_tools"}, 500f, 1, 3, 4, 1, 0, 15, 40, 45, 15,  5);
        }

        private static void CreateStaffRole(string folder, string name, string id,
            bool startsUnlocked, string[] required, float salary,
            float progC, float artC, float desC, float testC, float mgmtC,
            float baseProg, float baseArt, float baseDes, float baseTest, float baseMgmt)
        {
            string path = $"{folder}/{name}.asset";
            if (AssetExists(path)) return;

            var asset = ScriptableObject.CreateInstance<StaffRoleData>();
            asset.RoleId                   = id;
            asset.DisplayName              = name.Replace("QATester","QA Tester").Replace("GameDesigner","Game Designer").Replace("SoundDesigner","Sound Designer");
            asset.StartsUnlocked           = startsUnlocked;
            asset.RequiredResearchIds      = required ?? System.Array.Empty<string>();
            asset.BaseWeeklySalary         = salary;
            asset.ProgrammingContribution  = progC;
            asset.ArtContribution          = artC;
            asset.DesignContribution       = desC;
            asset.TestingContribution      = testC;
            asset.ManagementContribution   = mgmtC;
            asset.BaseProgramming          = baseProg;
            asset.BaseArt                  = baseArt;
            asset.BaseDesign               = baseDes;
            asset.BaseTesting              = baseTest;
            asset.BaseManagement           = baseMgmt;

            AssetDatabase.CreateAsset(asset, path);
        }

        // ── Research Nodes ────────────────────────────────────────────────────
        private static void CreateDefaultResearchNodes()
        {
            string folder = $"{ResourcesRoot}/Research";
            EnsureFolder(folder);

            // Tier 1 — always available from the start
            CreateResearchNode(folder, "BetterEngineTools",   "research_engine_tools",
                "Tech", true,  null, 0f,  5_000f, 4f,
                null, null, null, 5f, 1.1f, 0f,
                "All games receive +5 quality points per week.");

            CreateResearchNode(folder, "MarketingBasics",     "research_marketing_basics",
                "Business", true, null, 0f, 3_000f, 3f,
                null, null, null, 0f, 1.0f, 500f,
                "Earn $500 passive income per week from brand recognition.");

            // Tier 2
            CreateResearchNode(folder, "NarrativeTools",      "research_narrative_tools",
                "Creative", false, new[]{"research_engine_tools"}, 15f, 8_000f, 6f,
                new[]{"genre_rpg","genre_horror"}, null, null, 3f, 1.15f, 0f,
                "Unlocks RPG and Horror genres. +3 quality/week.");

            CreateResearchNode(folder, "PCPortingTools",      "research_pc_tools",
                "Tech", false, new[]{"research_engine_tools"}, 15f, 10_000f, 5f,
                null, new[]{"platform_pc"}, null, 0f, 1.0f, 0f,
                "Unlocks PC platform.");

            CreateResearchNode(folder, "ProjectManagement",   "research_project_management",
                "Business", false, new[]{"research_marketing_basics"}, 20f, 12_000f, 8f,
                null, null, new[]{"role_manager"}, 0f, 1.2f, 1_000f,
                "Unlocks Producer role. +20% reputation gain. +$1000/week.");

            CreateResearchNode(folder, "AudioTools",          "research_audio_tools",
                "Creative", false, new[]{"research_engine_tools"}, 10f, 6_000f, 4f,
                null, null, new[]{"role_sound"}, 2f, 1.05f, 0f,
                "Unlocks Sound Designer role. +2 quality/week.");

            // Tier 3
            CreateResearchNode(folder, "AISystems",           "research_ai_systems",
                "Tech", false, new[]{"research_engine_tools","research_pc_tools"}, 30f, 20_000f, 10f,
                new[]{"genre_strategy"}, null, null, 10f, 1.25f, 0f,
                "Unlocks Strategy genre. +10 quality/week from smarter NPC systems.");

            CreateResearchNode(folder, "ConsoleDeveloperKit", "research_console_dev_kit",
                "Tech", false, new[]{"research_pc_tools","research_project_management"}, 40f, 50_000f, 16f,
                null, new[]{"platform_console"}, null, 0f, 1.3f, 2_000f,
                "Unlocks Console platform. +$2000/week in licensing income.");

            CreateResearchNode(folder, "ViralMarketing",      "research_viral_marketing",
                "Business", false, new[]{"research_marketing_basics","research_project_management"}, 35f, 18_000f, 8f,
                null, null, null, 0f, 1.0f, 3_000f,
                "Passive viral revenue stream of $3000/week.");

            CreateResearchNode(folder, "CrunchMode",          "research_crunch_mode",
                "Business", false, new[]{"research_project_management"}, 25f, 5_000f, 3f,
                null, null, null, 15f, 1.0f, 0f,
                "Unlocks Crunch mechanic: +15 quality/week at a morale cost.");
        }

        private static void CreateResearchNode(string folder, string name, string id,
            string category, bool startsAvailable, string[] prereqs, float minRep,
            float cost, float weeks,
            string[] unlocksGenres, string[] unlocksPlatforms, string[] unlocksRoles,
            float qualityBonus, float repMult, float passiveIncome, string effectSummary)
        {
            string path = $"{folder}/{name}.asset";
            if (AssetExists(path)) return;

            var asset = ScriptableObject.CreateInstance<ResearchNodeData>();
            asset.NodeId                     = id;
            asset.DisplayName                = ObjectNames.NicifyVariableName(name);
            asset.Category                   = category;
            asset.StartsAvailable            = startsAvailable;
            asset.PrerequisiteNodeIds        = prereqs   ?? System.Array.Empty<string>();
            asset.MinReputation              = minRep;
            asset.MoneyCost                  = cost;
            asset.WeeksRequired              = weeks;
            asset.UnlocksGenreIds            = unlocksGenres    ?? System.Array.Empty<string>();
            asset.UnlocksPlatformIds         = unlocksPlatforms ?? System.Array.Empty<string>();
            asset.UnlocksStaffRoleIds        = unlocksRoles     ?? System.Array.Empty<string>();
            asset.QualityBonus               = qualityBonus;
            asset.ReputationGainMultiplier   = repMult;
            asset.PassiveIncomePerWeek       = passiveIncome;
            asset.EffectSummary              = effectSummary;

            AssetDatabase.CreateAsset(asset, path);
        }

        // ── Events ────────────────────────────────────────────────────────────
        private static void CreateDefaultEvents()
        {
            string folder = $"{ResourcesRoot}/Events";
            EnsureFolder(folder);

            // Random events
            CreateSimpleEvent(folder, "IndustryConference", "event_conference",
                "Industry Conference",
                "A major games industry conference is happening this month. " +
                "You could attend to network and boost reputation, or skip it to save money.",
                "Industry", EventTrigger.Random, 6, 2f, true,
                new EventChoice[]
                {
                    new EventChoice { Label = "Attend ($2,000)", ResultDescription = "Great networking! Reputation +5.",
                        MoneyDelta = -2000f, ReputationDelta = 5f },
                    new EventChoice { Label = "Skip", IsIgnore = true }
                });

            CreateSimpleEvent(folder, "StaffBurnout", "event_burnout",
                "Staff Burnout",
                "The team has been working hard. Morale is suffering across the board.",
                "Staff", EventTrigger.Random, 4, 1.5f, false,
                new EventChoice[]
                {
                    new EventChoice { Label = "Team Pizza Party ($500)", ResultDescription = "The team feels appreciated. Morale +15.",
                        MoneyDelta = -500f, MoraleDelta = 15f },
                    new EventChoice { Label = "Ignore", ResultDescription = "The team grumbles. Morale -5.",
                        MoraleDelta = -5f }
                });

            CreateSimpleEvent(folder, "VCOffer", "event_vc_offer",
                "Venture Capital Offer",
                "An investor has heard of your studio and wants to inject $20,000, " +
                "but they want 10 reputation points worth of creative control.",
                "Business", EventTrigger.Random, 12, 0.5f, true,
                new EventChoice[]
                {
                    new EventChoice { Label = "Accept investment", ResultDescription = "Funds received. Some creative freedom lost.",
                        MoneyDelta = 20000f, ReputationDelta = -10f },
                    new EventChoice { Label = "Decline", IsIgnore = true }
                });

            CreateSimpleEvent(folder, "HardDriveCrash", "event_drive_crash",
                "Hard Drive Crash!",
                "A critical hard drive has failed. You've lost a week of work on your current project " +
                "and need to pay for data recovery.",
                "Tech", EventTrigger.Random, 3, 0.8f, false,
                new EventChoice[]
                {
                    new EventChoice { Label = "Pay for recovery ($3,000)", ResultDescription = "Data recovered. Crisis averted.",
                        MoneyDelta = -3000f },
                    new EventChoice { Label = "Accept the loss", ResultDescription = "A painful setback. Reputation -3.",
                        ReputationDelta = -3f }
                });

            CreateSimpleEvent(folder, "GoodReview", "event_good_review",
                "Glowing Review Article",
                "A popular gaming blog has written a glowing article about your studio's potential.",
                "Industry", EventTrigger.Random, 2, 2f, false,
                null);  // Info-only, no choices

            // Reputation threshold event
            CreateSimpleEvent(folder, "IndustryRecognition", "event_industry_rec",
                "Industry Recognition",
                "Your studio has earned a reputation for quality. " +
                "A developer forum wants to feature you as a success story!",
                "Milestone", EventTrigger.OnReputationThreshold, 0, 1f, true,
                null);

            // Year-start events
            CreateSimpleEvent(folder, "NewYear1", "event_year1",
                "Year 1 Complete!",
                "You've survived your first year in the industry. " +
                "The market is buzzing about what you'll make next.",
                "Milestone", EventTrigger.OnYearStart, 0, 1f, true,
                null);
        }

        /// <param name="repThreshold">Used when trigger == OnReputationThreshold.</param>
        /// <param name="targetYear">Used when trigger == OnYearStart (0 = not applicable).</param>
        private static void CreateSimpleEvent(string folder, string name, string id,
            string title, string description, string category,
            EventTrigger trigger, int minMonth, float weight, bool oneTime,
            EventChoice[] choices, float repThreshold = 0f, int targetYear = 0)
        {
            string path = $"{folder}/{name}.asset";
            if (AssetExists(path)) return;

            var asset = ScriptableObject.CreateInstance<RandomEventData>();
            asset.EventId             = id;
            asset.Title               = title;
            asset.Description         = description;
            asset.Category            = category;
            asset.Trigger             = trigger;
            asset.MinMonth            = minMonth;
            asset.Weight              = weight;
            asset.OneTimeOnly         = oneTime;
            asset.Choices             = choices ?? System.Array.Empty<EventChoice>();
            asset.ReputationThreshold = repThreshold;
            asset.TargetYear          = targetYear;

            AssetDatabase.CreateAsset(asset, path);
        }

        // ── Helpers ───────────────────────────────────────────────────────────
        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;

            // Create each missing directory segment
            string[] parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }

        private static bool AssetExists(string path) =>
            !string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(path));
    }
}
#endif
