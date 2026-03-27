/// <summary>
/// Default ScriptableObject data assets.
///
/// These files cannot be created automatically at runtime because Unity
/// ScriptableObject assets must live in the project's Assets/ folder and
/// be registered with the AssetDatabase.
///
/// HOW TO CREATE THE DEFAULT DATA ASSETS IN UNITY:
/// ─────────────────────────────────────────────────
/// 1. Open your Unity project.
/// 2. In the Project window, navigate to:
///       Assets/Resources/Data/Genres
///    (create the folder if it doesn't exist).
/// 3. Right-click → Create → GameDevStudio → Genre
///    to create a new GenreData asset.
/// 4. In the Inspector, fill in the fields as described below for each asset.
///
/// Alternatively, use the MenuItems defined in
///    Assets/Scripts/Editor/DefaultDataCreator.cs
/// to auto-generate all default assets from the Unity menu:
///    Tools → GameDevStudio → Create Default Data Assets
/// ─────────────────────────────────────────────────────────
///
/// DEFAULT GENRES
/// ──────────────
/// File: Resources/Data/Genres/Action.asset
///   GenreId:             genre_action
///   DisplayName:         Action
///   Description:         Fast-paced gameplay focused on reflexes and combat.
///   StartsUnlocked:      true
///   DevTimeMultiplier:   1.0
///   ProgrammingWeight:   0.35
///   ArtWeight:           0.30
///   DesignWeight:        0.25
///   TestingWeight:       0.10
///   BaseMarketAppeal:    7
///   BasePricePerUnit:    2.99
///
/// File: Resources/Data/Genres/RPG.asset
///   GenreId:             genre_rpg
///   DisplayName:         RPG
///   Description:         Story-driven adventures with character progression.
///   StartsUnlocked:      false
///   RequiredResearchIds: research_narrative_tools
///   UnlockReputation:    20
///   DevTimeMultiplier:   1.5
///   ProgrammingWeight:   0.25
///   ArtWeight:           0.25
///   DesignWeight:        0.40
///   TestingWeight:       0.10
///   BaseMarketAppeal:    8
///   BasePricePerUnit:    4.99
///
/// File: Resources/Data/Genres/Puzzle.asset
///   GenreId:             genre_puzzle
///   DisplayName:         Puzzle
///   Description:         Logic and brain-teaser challenges.
///   StartsUnlocked:      true
///   DevTimeMultiplier:   0.8
///   ProgrammingWeight:   0.20
///   ArtWeight:           0.20
///   DesignWeight:        0.50
///   TestingWeight:       0.10
///   BaseMarketAppeal:    6
///   BasePricePerUnit:    1.99
///
/// File: Resources/Data/Genres/Simulation.asset
///   GenreId:             genre_simulation
///   DisplayName:         Simulation
///   Description:         Realistic or abstract simulation of real-world systems.
///   StartsUnlocked:      false
///   UnlockReputation:    30
///   DevTimeMultiplier:   1.8
///   ProgrammingWeight:   0.45
///   ArtWeight:           0.15
///   DesignWeight:        0.25
///   TestingWeight:       0.15
///   BaseMarketAppeal:    7
///   BasePricePerUnit:    5.99
///
/// File: Resources/Data/Genres/Horror.asset
///   GenreId:             genre_horror
///   DisplayName:         Horror
///   Description:         Psychological terror and survival gameplay.
///   StartsUnlocked:      false
///   RequiredResearchIds: research_narrative_tools
///   UnlockReputation:    25
///   DevTimeMultiplier:   1.3
///   ProgrammingWeight:   0.25
///   ArtWeight:           0.35
///   DesignWeight:        0.30
///   TestingWeight:       0.10
///   BaseMarketAppeal:    7
///   BasePricePerUnit:    3.99
///
/// DEFAULT PLATFORMS
/// ─────────────────
/// File: Resources/Data/Platforms/Mobile.asset
///   PlatformId:          platform_mobile
///   DisplayName:         Mobile
///   Description:         iOS and Android smartphones and tablets.
///   StartsUnlocked:      true
///   DevEffortMultiplier: 1.0
///   MinTeamSize:         1
///   AudienceMultiplier:  2.0
///   PlatformCut:         0.30
///   CostMultiplier:      0.8
///
/// File: Resources/Data/Platforms/PC.asset
///   PlatformId:          platform_pc
///   DisplayName:         PC
///   Description:         Windows, Mac, and Linux desktop computers.
///   StartsUnlocked:      false
///   RequiredResearchIds: research_pc_tools
///   UnlockReputation:    15
///   DevEffortMultiplier: 1.2
///   MinTeamSize:         2
///   AudienceMultiplier:  1.5
///   PlatformCut:         0.30
///   CostMultiplier:      1.0
///
/// File: Resources/Data/Platforms/Console.asset
///   PlatformId:          platform_console
///   DisplayName:         Console
///   Description:         Home gaming consoles.
///   StartsUnlocked:      false
///   RequiredResearchIds: research_console_dev_kit
///   UnlockReputation:    40
///   DevEffortMultiplier: 2.0
///   MinTeamSize:         5
///   AudienceMultiplier:  1.8
///   PlatformCut:         0.30
///   CostMultiplier:      2.5
///
/// DEFAULT STAFF ROLES
/// ────────────────────
/// File: Resources/Data/StaffRoles/Programmer.asset
///   RoleId:                  role_programmer
///   DisplayName:             Programmer
///   ProgrammingContribution: 8
///   ArtContribution:         0
///   DesignContribution:      1
///   TestingContribution:     2
///   ManagementContribution:  0
///   BaseProgramming:         60
///   BaseArt:                 10
///   BaseDesign:              20
///   BaseTesting:             30
///   BaseManagement:          10
///   BaseWeeklySalary:        600
///   StartsUnlocked:          true
///
/// File: Resources/Data/StaffRoles/Artist.asset
///   RoleId:                  role_artist
///   DisplayName:             Artist
///   ProgrammingContribution: 0
///   ArtContribution:         8
///   DesignContribution:      2
///   TestingContribution:     0
///   ManagementContribution:  0
///   BaseProgramming:         10
///   BaseArt:                 65
///   BaseDesign:              25
///   BaseTesting:             10
///   BaseManagement:          5
///   BaseWeeklySalary:        550
///   StartsUnlocked:          true
///
/// File: Resources/Data/StaffRoles/Designer.asset
///   RoleId:                  role_designer
///   DisplayName:             Game Designer
///   ProgrammingContribution: 1
///   ArtContribution:         2
///   DesignContribution:      8
///   TestingContribution:     1
///   ManagementContribution:  1
///   BaseProgramming:         20
///   BaseArt:                 30
///   BaseDesign:              65
///   BaseTesting:             20
///   BaseManagement:          15
///   BaseWeeklySalary:        575
///   StartsUnlocked:          true
///
/// File: Resources/Data/StaffRoles/QA.asset
///   RoleId:                  role_qa
///   DisplayName:             QA Tester
///   ProgrammingContribution: 2
///   ArtContribution:         0
///   DesignContribution:      1
///   TestingContribution:     9
///   ManagementContribution:  0
///   BaseProgramming:         25
///   BaseArt:                 5
///   BaseDesign:              15
///   BaseTesting:             70
///   BaseManagement:          10
///   BaseWeeklySalary:        450
///   StartsUnlocked:          true
///
/// File: Resources/Data/StaffRoles/Manager.asset
///   RoleId:                  role_manager
///   DisplayName:             Producer
///   Description:             Boosts morale and reduces project time.
///   ProgrammingContribution: 1
///   ArtContribution:         1
///   DesignContribution:      2
///   TestingContribution:     1
///   ManagementContribution:  9
///   BaseProgramming:         20
///   BaseArt:                 10
///   BaseDesign:              30
///   BaseTesting:             20
///   BaseManagement:          70
///   BaseWeeklySalary:        700
///   StartsUnlocked:          false
///   RequiredResearchIds:     research_project_management
/// </summary>
public sealed class DataSetupGuide { }
