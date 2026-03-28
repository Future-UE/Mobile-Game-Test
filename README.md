# 🎮 GameDev Studio — Text-Based Mobile Game

A **Game Dev Story-inspired** text-based studio simulator built for Unity (Mobile-first).
Manage your indie game studio: hire staff, develop games, research upgrades, react to events, and build your reputation.

---

## 📋 Table of Contents

1. [Features](#-features)
2. [Prerequisites](#-prerequisites)
3. [Step 1 — Install Unity Hub](#step-1--install-unity-hub)
4. [Step 2 — Install Unity 6000.3.11f1](#step-2--install-unity-600031f1)
5. [Step 3 — Add Mobile Build Support](#step-3--add-mobile-build-support)
6. [Step 4 — Create a New Unity Project](#step-4--create-a-new-unity-project)
7. [Step 5 — Import the Project Files](#step-5--import-the-project-files)
8. [Step 6 — Generate Default Data Assets](#step-6--generate-default-data-assets)
9. [Step 7 — Create the Bootstrap Scene](#step-7--create-the-bootstrap-scene)
10. [Step 8 — Create the Main Scene](#step-8--create-the-main-scene)
11. [Step 9 — Create UI Prefabs](#step-9--create-ui-prefabs)
12. [Step 10 — Configure Build Settings & Run](#step-10--configure-build-settings--run)
13. [Project Structure](#-project-structure)
14. [Gameplay Loop](#-gameplay-loop)
15. [Adding New Content](#-adding-new-content)
16. [Running Tests](#-running-tests)
17. [Architecture Decisions](#-architecture-decisions)
18. [Mobile Considerations](#-mobile-considerations)
19. [Roadmap](#-roadmap--expandability-ideas)

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

## 🧰 Prerequisites

Before you begin, make sure you have:

- A Windows, macOS, or Linux computer
- An internet connection (for downloading Unity)
- At least **8 GB of free disk space** (Unity + project files)
- A [Unity account](https://id.unity.com/) — free to create

> **No prior Unity experience is required.** Every step below is explained in detail.

---

## Step 1 — Install Unity Hub

Unity Hub is the application that manages your Unity installations and projects. Think of it as a launcher.

1. Go to **[https://unity.com/download](https://unity.com/download)**.
2. Click **Download Unity Hub** and run the installer for your operating system.
3. Once installed, open **Unity Hub**.
4. Sign in with your Unity account (or create one — it's free).
5. When prompted, choose the **Personal** licence. Click **Get started** and follow the prompts to activate it.

> **Why do you need a licence?** Unity requires a free Personal licence to use the editor. You won't be charged anything.

---

## Step 2 — Install Unity 6000.3.11f1

This project requires exactly **Unity 6000.3.11f1**. Using a different version may cause errors.

1. In Unity Hub, click **Installs** in the left sidebar.
2. Click **Install Editor** (top-right button).
3. In the search bar, type `6000.3.11f1`.
   - If it doesn't appear, click **Archive** and find it at [https://unity.com/releases/editor/archive](https://unity.com/releases/editor/archive). Download the installer directly and Unity Hub will detect it.
4. Select **Unity 6000.3.11f1** and click **Install**.
5. On the **Add modules** screen that appears:
   - ✅ **Microsoft Visual Studio Community** (Windows) *or* **Visual Studio for Mac** — this is your code editor.
   - You do **not** need to add platform modules yet; we'll do that in the next step.
6. Click **Install** and wait for the download to finish (this may take 10–30 minutes).

> **Tip:** You can continue reading and preparing the project files while Unity downloads.

---

## Step 3 — Add Mobile Build Support

If you want to build the game for Android or iOS, you need to add the platform module to your Unity installation. You can **skip this step** if you only want to run the game inside the Unity Editor on your computer.

### For Android

1. In Unity Hub, go to **Installs**.
2. Find **Unity 6000.3.11f1** and click the **gear icon ⚙** next to it.
3. Select **Add modules**.
4. Check **Android Build Support** and its two sub-items:
   - ✅ Android SDK & NDK Tools
   - ✅ OpenJDK
5. Click **Install** and wait for it to finish.

### For iOS (macOS only)

1. Follow the same steps above but check **iOS Build Support** instead.
2. You will also need **Xcode** installed from the Mac App Store.

---

## Step 4 — Create a New Unity Project

1. In Unity Hub, click **Projects** in the left sidebar.
2. Click **New project** (top-right button).
3. At the top of the template screen, make sure the editor version shows **6000.3.11f1**. If it doesn't, click the version dropdown and select it.
4. Choose the **2D (URP)** template. This is the correct template for a mobile 2D game.
5. Give your project a name — for example, `GameDevStudio`.
6. Choose a location on your computer to save the project.
7. Click **Create project**. Unity will open and set up the project (this may take a minute or two).

> **What is URP?** URP stands for Universal Render Pipeline. It's Unity's recommended renderer for mobile games — it runs well on phones and tablets.

---

## Step 5 — Import the Project Files

Once your new project is open in Unity, you need to copy the game's source files into it.

1. **Download or clone this repository** to your computer.
   - If you have Git installed, open a terminal and run:
     ```
     git clone https://github.com/Future-UE/Mobile-Game-Test.git
     ```
   - If you don't have Git, click the green **Code** button on the GitHub page and select **Download ZIP**, then extract it.

2. **Locate your Unity project's `Assets` folder** on your computer. It will be inside the folder you chose in Step 4 (e.g. `Documents/GameDevStudio/Assets/`).

3. **Copy the contents** of the repository's `Assets/` folder into your Unity project's `Assets/` folder.
   - Copy everything inside `Assets/` — the `Scripts/`, `Tests/`, `Resources/`, and `ScriptableObjects/` folders.
   - Do **not** replace the whole `Assets/` folder; merge the contents into it.

4. **Switch back to Unity Editor.** Unity will detect the new files and start importing them automatically. You'll see a progress bar at the bottom. Wait for it to finish.

5. If a popup appears asking you to **Import TMP Essentials**, click **Import TMP Essentials**. This installs the TextMeshPro text renderer that the UI uses.

> **Tip:** If you see any yellow warning messages in the **Console** window (bottom of the screen), that is normal. Red errors, however, should be addressed — check that all files were copied correctly.

---

## Step 6 — Generate Default Data Assets

The game's content (genres, platforms, staff roles, etc.) is stored as data files called **ScriptableObjects**. A built-in editor tool will create all of these for you automatically.

1. In Unity, look at the **top menu bar** and click **Tools**.
2. Hover over **GameDevStudio**, then click **Create Default Data Assets**.

Unity will create the following data files inside `Assets/Resources/Data/`:

| Folder | What was created |
|---|---|
| `Genres/` | Action, Puzzle, RPG, Simulation, Horror, Strategy |
| `Platforms/` | Mobile, PC, Console |
| `StaffRoles/` | Programmer, Artist, Game Designer, QA, Producer, Sound Designer |
| `Research/` | 10 research nodes across 3 tiers |
| `Events/` | 7 random events |

> **How do you know it worked?** In the **Project** panel (bottom-left), expand `Assets → Resources → Data`. You should see those folders filled with files. Each file has a small icon that looks like a page with a Unity logo.

---

## Step 7 — Create the Bootstrap Scene

The game uses two scenes: a **Bootstrap** scene (loads first, sets up systems) and a **Main** scene (the actual game UI). Let's create the Bootstrap scene first.

1. In the top menu, go to **File → New Scene**.
2. In the **New Scene** dialog, choose **Empty Scene** and click **Create**.
3. Save the scene immediately: **File → Save As…**, name it `Bootstrap`, and save it inside `Assets/` (or a subfolder like `Assets/Scenes/`).

4. In the **Hierarchy** panel (left side), right-click in the empty area and choose **Create Empty**. This adds a blank GameObject.
5. Rename it to `GameBootstrap`:
   - Click on it once to select it, then press **F2** (Windows/Linux) or double-click (macOS) to rename it.

6. With `GameBootstrap` selected, look at the **Inspector** panel on the right. Click **Add Component**.
7. In the search bar that appears, type `GameBootstrap` and select the **GameBootstrap** script from the results.

8. In the Inspector you'll see a field called **Main Scene Name**. Make sure it says `Main` (this is the name of the scene we'll create next).

> **What does this scene do?** When the game starts, Unity loads this scene first. The `GameBootstrap` script then automatically loads the Main scene, ensuring all game systems are ready before the UI appears.

---

## Step 8 — Create the Main Scene

The Main scene contains all the game UI and the GameManager.

### 8a. Create and Save the Scene

1. Go to **File → New Scene**, choose **Empty Scene**, click **Create**.
2. Save it: **File → Save As…**, name it `Main`, save it in the same folder as Bootstrap.

### 8b. Add the GameManager

1. In the **Hierarchy**, right-click → **Create Empty**. Name it `GameManager`.
2. With it selected, click **Add Component** in the Inspector.
3. Search for `GameManager` and select the **GameManager** script.
4. You'll see two Inspector fields you can customise:
   - **Seconds Per Week** — how many real seconds pass between each in-game week (default: `5`).
   - **Default Studio Name** — the name shown when starting a new game (default: `Indie Dreams Studio`).

### 8c. Add the UI Canvas

1. In the Hierarchy, right-click → **UI → Canvas**. Unity creates a Canvas with an EventSystem automatically.
2. Select the **Canvas** object. In the Inspector, set:
   - **Render Mode** → `Screen Space - Overlay`
   - **UI Scale Mode** → `Scale With Screen Size`
   - **Reference Resolution** → `1080 x 1920` (standard portrait mobile)
   - **Match** → `0.5` (balances width and height scaling)

3. With the Canvas selected, click **Add Component**, search for `UIManager`, and add the **UIManager** script.

### 8d. Create the UI Panels

You need to create six panels as children of the Canvas. For each panel in the table below:
- Right-click the **Canvas** in the Hierarchy → **UI → Panel**.
- Rename the panel to the name shown.
- Set it to fill the full screen: in the Inspector's **Rect Transform**, click the anchor preset icon (top-left of Rect Transform), hold **Alt**, and click the **stretch both** preset (bottom-right icon in the grid).
- Add the corresponding script via **Add Component**.

| Panel Name | Script to Attach |
|---|---|
| `MainHUDPanel` | `MainHUDUI` |
| `ProjectsPanel` | `ProjectUI` |
| `StaffPanel` | `StaffUI` |
| `ResearchPanel` | `ResearchUI` |
| `EventPanel` | `EventUI` |
| `NotificationPanel` | `NotificationUI` |

### 8e. Wire Up the UIManager

1. Select the **Canvas** object (which has the `UIManager` script).
2. In the Inspector you'll see slots for each panel. Drag each panel from the Hierarchy into its matching slot:
   - Drag `MainHUDPanel` into **Main HUD Panel**
   - Drag `ProjectsPanel` into **Projects Panel**
   - Drag `StaffPanel` into **Staff Panel**
   - Drag `ResearchPanel` into **Research Panel**
   - Drag `EventPanel` into **Event Panel**
   - Drag `NotificationPanel` into **Notification Panel**

> **How to drag and drop:** Click the panel in the Hierarchy, hold the mouse button, and drag it onto the matching field in the Inspector. The field will highlight when you hover over the correct slot.

---

## Step 9 — Create UI Prefabs

Each panel displays a scrollable list of items. The list items are built from **prefabs** — reusable GameObject templates. You need to create five prefabs.

> **What is a prefab?** A prefab is a saved template for a GameObject. Whenever the game needs to show a new item in a list (e.g. a staff member), it creates a copy of the prefab.

For **each prefab** in the table below, follow these steps:

1. In the Hierarchy, right-click → **Create Empty**, and name it as shown.
2. Add the required child objects (right-click the parent → **UI → Text - TextMeshPro** for text, or **UI → Button - TextMeshPro** for buttons).
3. Once built, drag the parent object from the Hierarchy into the `Assets/` folder in the **Project** panel. This saves it as a prefab (the icon turns blue).
4. Delete the original from the Hierarchy (the prefab file in the Project panel is what matters now).

| Prefab Name | Child Objects Needed (in order) |
|---|---|
| `ProjectEntryPrefab` | [0] TMP_Text "Title", [1] TMP_Text "Phase", [2] TMP_Text "Progress", Button |
| `StaffEntryPrefab` | [0] TMP_Text "Name", [1] TMP_Text "Role", [2] TMP_Text "Status", [3] TMP_Text "Morale", Button |
| `CandidateEntryPrefab` | [0] TMP_Text "Name", [1] TMP_Text "Stats", [2] TMP_Text "Salary", Button |
| `NodeEntryPrefab` | [0] TMP_Text "Name", [1] TMP_Text "Category", [2] TMP_Text "Status", Button |
| `NotificationPrefab` | TMP_Text, Image (background), Button (close) |

After creating the prefabs, assign them in each panel's script via the Inspector:
- Select `ProjectsPanel` → find **Project Entry Prefab** slot → drag in `ProjectEntryPrefab`.
- Repeat for each panel and its corresponding prefab.

> **Tip:** Use `TextMeshPro - Text (UI)` for all text components, not the legacy `Text` component. If Unity prompts you to **Import TMP Essential Resources**, do so.

---

## Step 10 — Configure Build Settings & Run

### Add Both Scenes to Build Settings

1. Go to **File → Build Settings**.
2. Click **Add Open Scenes** — but first make sure the correct scene is open. You need to add both scenes.
   - Open the `Bootstrap` scene (double-click it in the Project panel), then click **Add Open Scenes**.
   - Open the `Main` scene, then click **Add Open Scenes** again.
3. In the **Scenes In Build** list, make sure **Bootstrap is at index 0** (top of the list). If it isn't, drag it to the top.

### Set the Build Platform (Optional — for device builds)

1. Still in **Build Settings**, select **Android** or **iOS** from the platform list on the left.
2. Click **Switch Platform** and wait for Unity to recompile.

### Test in the Editor

1. Close Build Settings.
2. Open the `Bootstrap` scene by double-clicking it in the Project panel.
3. Press the **Play ▶** button at the top of the Unity Editor.
4. The game will start. You should see the Bootstrap scene load, then the Main scene appear with the game UI.

> **Troubleshooting:** If you see errors in the Console window (bottom of the screen), the most common fixes are:
> - Make sure all prefab slots are assigned in each panel's Inspector.
> - Make sure `UIManager` has all six panel slots filled.
> - Make sure the `GameBootstrap` script's **Main Scene Name** is exactly `Main` (capital M, matching your scene name).

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