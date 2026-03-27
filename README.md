# 🎮 GameDev Studio — Text-Based Mobile Game

A **Game Dev Story-inspired** text-based studio simulator built for Unity (Mobile-first).
Manage your indie game studio: hire staff, develop games, research upgrades, react to events, and build your reputation.

---

## ✨ Features

| System | Description |
|---|---|
| **Studio Management** | Track money, reputation, fans, office tier |
| **Game Projects** | 5-phase dev pipeline: Concept → Pre-Production → Production → Testing → Polish |
| **Staff Hiring** | Procedurally-generated candidates per role; morale, experience, training |
| **Research Tree** | Unlockable upgrades that open new genres, platforms, roles, and passive income |
| **Random Events** | Weighted random events with player choices and consequences |
| **Post-Release Sales** | Exponential decay revenue stream after each game release |
| **Save / Load** | JSON persist save to `Application.persistentDataPath` |
| **Event Bus** | Decoupled type-safe event system — add listeners anywhere |
| **Data-Driven Design** | All content is Unity ScriptableObjects — add content with zero code changes |

---

## 🗂️ Project Structure

```
Assets/
├── Scripts/
│   ├── Core/           GameManager, TimeManager, SaveSystem, GameBootstrap
│   ├── Models/         StudioStats, Employee, GameProject, ResearchNode
│   ├── Data/           ScriptableObject definitions (GenreData, PlatformData, …)
│   ├── Managers/       StudioManager, ProjectManager, StaffManager, ResearchManager, EventManager
│   ├── Events/         GameEventBus + all event structs
│   ├── UI/             UIManager, MainHUDUI, ProjectUI, StaffUI, ResearchUI, EventUI, NotificationUI
│   ├── Utils/          Constants, ExtensionMethods
│   └── Editor/         DefaultDataCreator (Unity Editor tool)
├── Tests/              Edit-mode NUnit tests (no scene required)
├── Resources/
│   └── Data/
│       ├── Genres/     GenreData assets
│       ├── Platforms/  PlatformData assets
│       ├── StaffRoles/ StaffRoleData assets
│       ├── Research/   ResearchNodeData assets
│       └── Events/     RandomEventData assets
└── ScriptableObjects/  DataSetupGuide.cs (documentation)
```

---

## 🚀 Getting Started (Unity Setup)

### 1. Create the Unity Project

1. Open **Unity Hub** and create a new **2D (URP)** project.  
2. Set the build target to **Android** or **iOS** under *File → Build Settings*.
3. Copy the entire `Assets/` folder from this repository into your new Unity project's `Assets/` folder.

### 2. Generate Default Data Assets

Open the Unity Editor and navigate to:

```
Tools → GameDevStudio → Create Default Data Assets
```

This runs `DefaultDataCreator.cs` which auto-creates all:
- 6 genres (Action, Puzzle, RPG, Simulation, Horror, Strategy)
- 3 platforms (Mobile, PC, Console)
- 6 staff roles (Programmer, Artist, Game Designer, QA, Producer, Sound Designer)
- 10 research nodes across 3 tiers
- 7 random events

### 3. Scene Setup

#### Bootstrap Scene
1. Create a new scene called **Bootstrap**.
2. Add an empty GameObject named `GameBootstrap` and attach the `GameBootstrap` script.
3. Add this as the **first scene** in *File → Build Settings*.

#### Main Scene
1. Create a scene called **Main**.
2. **Canvas (UI Root)** — Add a canvas and attach `UIManager`.
3. **GameManager** — Add a GameObject with the `GameManager` script.
4. Create panels and wire them up via the `UIManager` Inspector fields:

| Panel Name | Script |
|---|---|
| `MainHUDPanel` | `MainHUDUI` |
| `ProjectsPanel` | `ProjectUI` |
| `StaffPanel` | `StaffUI` |
| `ResearchPanel` | `ResearchUI` |
| `EventPanel` | `EventUI` |
| `NotificationPanel` | `NotificationUI` |

### 4. UI Prefabs (Required)

Create simple prefabs with `TMP_Text` and `Button` children:

| Prefab | Purpose | Expected Children |
|---|---|---|
| `ProjectEntryPrefab` | Project list row | [0] Title, [1] Phase, [2] Progress; Button |
| `StaffEntryPrefab` | Staff list row | [0] Name, [1] Role, [2] Status, [3] Morale; Button |
| `CandidateEntryPrefab` | Hire candidate row | [0] Name, [1] Stats, [2] Salary; Button |
| `NodeEntryPrefab` | Research list row | [0] Name, [1] Category, [2] Status; Button |
| `NotificationPrefab` | Toast notification | TMP_Text, Image background, Close Button |

> **Tip**: Use `TextMeshPro - Text (UI)` for all text fields. Install the TMP Essential Resources if prompted.

---

## 🎮 Gameplay Loop

```
Week tick (every N real seconds)
  │
  ├─ Staff weekly tick    (salary deducted, morale decays, experience grows)
  ├─ Project weekly tick  (phases advance, quality/bugs accumulate)
  ├─ Research weekly tick (nodes progress toward completion)
  ├─ Released sales tick  (post-release revenue drip)
  └─ Event tick           (random event roll, reputation triggers)
```

### Game Phases

Each game project moves through **five phases** sequentially:

1. **Concept** — Design-heavy; defines the game's direction.  
2. **Pre-Production** — Planning and architecture.  
3. **Production** — Main development; quality accumulates but bugs are introduced.  
4. **Testing & QA** — Bugs are fixed; QA testers shine here.  
5. **Polishing** — Final art and design pass; quality gains continue.  

After Polishing completes, the game **auto-releases**, generating a review score and initial sales revenue.

### Review Score Formula

```
reviewScore = (qualityRatio × 8) + (reputation/100 × 2) − (bugPenalty × 2)
```
Where `qualityRatio = qualityPoints / (plannedWeeks × 20)`.  
Score is clamped to **1–10**.

### Research Tree

Research nodes form a directed acyclic graph (DAG) via `PrerequisiteNodeIds`.  
Completing a node may:
- Unlock new genres or platforms
- Unlock new staff roles
- Provide passive weekly income
- Boost quality per week

---

## 🔧 Adding New Content

All game content is stored as Unity ScriptableObject assets under `Assets/Resources/Data/`.  
**No code changes are required to add new content.**

### Adding a New Genre

1. In the Project window: right-click → **Create → GameDevStudio → Genre**
2. Place it in `Assets/Resources/Data/Genres/`
3. Fill in the Inspector fields (GenreId must be unique)

### Adding a New Research Node

1. Right-click → **Create → GameDevStudio → ResearchNode**  
2. Place it in `Assets/Resources/Data/Research/`
3. Set prerequisites via `PrerequisiteNodeIds` (referencing other NodeIds)

### Adding a New Event

1. Right-click → **Create → GameDevStudio → RandomEvent**  
2. Place it in `Assets/Resources/Data/Events/`
3. Set `Choices` array for player interaction, or leave empty for info-only

### Adding a New Staff Role

1. Right-click → **Create → GameDevStudio → StaffRole**  
2. Place it in `Assets/Resources/Data/StaffRoles/`

---

## 🧪 Running Tests

1. In Unity, open **Window → General → Test Runner**
2. Select the **EditMode** tab
3. Click **Run All** — tests in `Assets/Tests/GameLogicTests.cs` will execute

Tests cover:
- `StudioStats` (money tracking, reputation clamping, office tiers)
- `Employee` (morale multiplier, weekly tick, skill calculation)
- `GameProject` (dev progress, review labels)
- `ResearchNode` (status flags)
- `GameEventBus` (subscribe, publish, unsubscribe, multi-listener)

---

## 📐 Architecture Decisions

| Decision | Reason |
|---|---|
| **ScriptableObjects for data** | Zero-code content addition; hot-reloadable in Editor |
| **Manager pattern (plain C# classes)** | Testable without Unity; `GameManager` owns all managers |
| **GameEventBus (event structs)** | Completely decoupled systems; no direct cross-references |
| **JSON save via `JsonUtility`** | Built into Unity; no extra dependencies; mobile-friendly |
| **Procedural staff generation** | Infinite replayability without static data tables |
| **Phase-based project pipeline** | Clear progression visible to players; easy to balance |

---

## 📱 Mobile Considerations

- UI designed for portrait layout on 1080×1920 screens.
- Use Unity's **Safe Area** component to handle notches.
- `SecondsPerWeek` in `GameManager` can be adjusted for pacing (default: 5 seconds/week).
- Save path uses `Application.persistentDataPath` — correct for mobile.
- No third-party dependencies; the project uses only Unity built-ins.

---

## 🗺️ Roadmap / Expandability Ideas

- [ ] More genres (Sports, Educational, Casual, Shooter)
- [ ] More platforms (Handheld, VR, Web)
- [ ] Competitor studios with AI-driven behaviour
- [ ] Sequel / franchise mechanics
- [ ] Contract work (extra income with tight deadlines)
- [ ] Industry awards ceremony
- [ ] Localisation system
- [ ] Cloud save
- [ ] Leaderboard integration