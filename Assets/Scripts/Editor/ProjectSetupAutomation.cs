#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using GameDevStudio.Core;
using GameDevStudio.UI;

namespace GameDevStudio.Editor
{
    /// <summary>
    /// One-click scene/bootstrap/UI setup so new users can get a runnable project
    /// without missing critical camera/build wiring.
    /// </summary>
    public static class ProjectSetupAutomation
    {
        private const string ScenesFolder = "Assets/Scenes";
        private const string BootstrapScenePath = ScenesFolder + "/Bootstrap.unity";
        private const string MainScenePath = ScenesFolder + "/Main.unity";

        [MenuItem("Tools/GameDevStudio/Setup/Automate Initial Scene Setup")]
        public static void AutomateInitialSceneSetup()
        {
            EnsureFolder(ScenesFolder);
            CreateBootstrapScene();
            CreateMainScene();
            EnsureBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorSceneManager.OpenScene(BootstrapScenePath);
            EditorUtility.DisplayDialog(
                "GameDevStudio Setup Complete",
                "Bootstrap/Main scenes, camera, UI roots, and build settings were created.\n\n" +
                "Next: run Tools → GameDevStudio → Create Default Data Assets, then press Play from Bootstrap.",
                "OK");
        }

        [MenuItem("Tools/GameDevStudio/Setup/Automate Initial Setup + Default Data")]
        public static void AutomateInitialSetupWithData()
        {
            AutomateInitialSceneSetup();
            DefaultDataCreator.CreateAll();
            EditorUtility.DisplayDialog(
                "GameDevStudio Setup Complete",
                "Scene setup and default data generation both finished.",
                "OK");
        }

        [MenuItem("Tools/GameDevStudio/Setup/Validate Setup (White Screen Checks)")]
        public static void ValidateWhiteScreenRisks()
        {
            var issues = "";

            if (!File.Exists(BootstrapScenePath))
            {
                issues += "- Bootstrap scene file does not exist at Assets/Scenes/Bootstrap.unity.\n";
            }
            else
            {
                var bootstrapScene = EditorSceneManager.OpenScene(BootstrapScenePath, OpenSceneMode.Single);
                var bootstrap = Object.FindObjectOfType<GameBootstrap>();
                if (bootstrap == null)
                    issues += "- Bootstrap scene is missing a GameBootstrap component.\n";
                else if (string.IsNullOrWhiteSpace(bootstrap.MainSceneName))
                    issues += "- GameBootstrap.MainSceneName is empty.\n";

                if (string.IsNullOrEmpty(bootstrapScene.path))
                    issues += "- Bootstrap scene could not be opened correctly.\n";
            }

            if (!File.Exists(MainScenePath))
            {
                issues += "- Main scene file does not exist at Assets/Scenes/Main.unity.\n";
            }
            else
            {
                var mainScene = EditorSceneManager.OpenScene(MainScenePath, OpenSceneMode.Single);
                var mainCamera = Camera.main;
                if (mainCamera == null)
                    issues += "- Main scene is missing a camera tagged MainCamera.\n";

                var uiManager = Object.FindObjectOfType<UIManager>();
                if (uiManager == null)
                    issues += "- Main scene is missing a UIManager on a Canvas.\n";
                else
                {
                    if (uiManager.MainHUDPanel == null) issues += "- UIManager.MainHUDPanel is not assigned.\n";
                    if (uiManager.ProjectsPanel == null) issues += "- UIManager.ProjectsPanel is not assigned.\n";
                    if (uiManager.StaffPanel == null) issues += "- UIManager.StaffPanel is not assigned.\n";
                    if (uiManager.ResearchPanel == null) issues += "- UIManager.ResearchPanel is not assigned.\n";
                    if (uiManager.EventPanel == null) issues += "- UIManager.EventPanel is not assigned.\n";
                    if (uiManager.NotificationPanel == null) issues += "- UIManager.NotificationPanel is not assigned.\n";
                }

                if (string.IsNullOrEmpty(mainScene.path))
                    issues += "- Main scene could not be opened correctly.\n";
            }

            if (!SceneInBuildAtIndex(BootstrapScenePath, 0))
                issues += "- Bootstrap scene is not at build index 0.\n";
            if (!SceneInBuild(MainScenePath))
                issues += "- Main scene is not in Build Settings.\n";

            if (string.IsNullOrEmpty(issues))
                EditorSceneManager.SaveOpenScenes();
            if (File.Exists(BootstrapScenePath))
                EditorSceneManager.OpenScene(BootstrapScenePath, OpenSceneMode.Single);

            if (string.IsNullOrEmpty(issues))
            {
                EditorUtility.DisplayDialog("Setup Validation", "No white-screen setup risks detected.", "OK");
                return;
            }

            Debug.LogWarning("[ProjectSetupAutomation] Setup validation issues:\n" + issues);
            EditorUtility.DisplayDialog("Setup Validation Issues", issues, "OK");
        }

        private static void CreateBootstrapScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Bootstrap";

            var bootstrapGO = new GameObject("GameBootstrap");
            var bootstrap = bootstrapGO.AddComponent<GameBootstrap>();
            bootstrap.MainSceneName = "Main";

            EditorSceneManager.SaveScene(scene, BootstrapScenePath);
        }

        private static void CreateMainScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Main";

            // Core systems
            var gm = new GameObject("GameManager");
            gm.AddComponent<GameManager>();

            CreateMainCamera();
            var uiManager = CreateCanvasAndUIRoot();
            BuildDefaultPanels(uiManager);
            EnsureEventSystem();

            EditorSceneManager.SaveScene(scene, MainScenePath);
        }

        private static void CreateMainCamera()
        {
            var cameraGO = new GameObject("Main Camera");
            cameraGO.tag = "MainCamera";
            cameraGO.transform.position = new Vector3(0f, 0f, -10f);

            var camera = cameraGO.AddComponent<Camera>();
            camera.orthographic = true;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.08f, 0.09f, 0.12f, 1f);
            camera.nearClipPlane = 0.3f;
            camera.farClipPlane = 1000f;

            cameraGO.AddComponent<AudioListener>();
        }

        private static UIManager CreateCanvasAndUIRoot()
        {
            var canvasGO = new GameObject(
                "Canvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster),
                typeof(UIManager));

            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

            return canvasGO.GetComponent<UIManager>();
        }

        private static void BuildDefaultPanels(UIManager uiManager)
        {
            var canvas = uiManager.transform;

            var mainHudPanel = CreateFullscreenPanel<MainHUDUI>(canvas, "MainHUDPanel", true);
            var projectsPanel = CreateFullscreenPanel<ProjectUI>(canvas, "ProjectsPanel", true);
            var staffPanel = CreateFullscreenPanel<StaffUI>(canvas, "StaffPanel", false);
            var researchPanel = CreateFullscreenPanel<ResearchUI>(canvas, "ResearchPanel", false);
            var eventPanel = CreateFullscreenPanel<EventUI>(canvas, "EventPanel", false);
            var notificationPanel = CreateFullscreenPanel<NotificationUI>(canvas, "NotificationPanel", true);
            var newProjectPanel = CreateFullscreenPanel(canvas, "NewProjectPanel", false);
            var hirePanel = CreateFullscreenPanel(canvas, "HirePanel", false);
            var settingsPanel = CreateFullscreenPanel(canvas, "SettingsPanel", false);

            uiManager.MainHUDPanel = mainHudPanel.gameObject;
            uiManager.ProjectsPanel = projectsPanel.gameObject;
            uiManager.StaffPanel = staffPanel.gameObject;
            uiManager.ResearchPanel = researchPanel.gameObject;
            uiManager.EventPanel = eventPanel.gameObject;
            uiManager.NotificationPanel = notificationPanel.gameObject;
            uiManager.NewProjectPanel = newProjectPanel.gameObject;
            uiManager.HirePanel = hirePanel.gameObject;
            uiManager.SettingsPanel = settingsPanel.gameObject;

            uiManager.MainHUD = mainHudPanel;
            uiManager.ProjectsUI = projectsPanel;
            uiManager.StaffPanelUI = staffPanel;
            uiManager.ResearchPanelUI = researchPanel;
            uiManager.EventPanelUI = eventPanel;
            uiManager.NotificationPanelUI = notificationPanel;
        }

        private static RectTransform CreateFullscreenPanel(Transform parent, string name, bool active) =>
            CreateFullscreenPanel<RectTransform>(parent, name, active);

        private static T CreateFullscreenPanel<T>(Transform parent, string name, bool active) where T : Component
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var component = go.AddComponent<T>();
            go.SetActive(active);
            return component;
        }

        private static void EnsureEventSystem()
        {
            var eventSystem = Object.FindObjectOfType<EventSystem>();
            if (eventSystem != null) return;

            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
        }

        private static void EnsureBuildSettings()
        {
            var current = EditorBuildSettings.scenes;
            var map = new Dictionary<string, EditorBuildSettingsScene>();
            foreach (var s in current)
                map[s.path] = s;

            map[BootstrapScenePath] = new EditorBuildSettingsScene(BootstrapScenePath, true);
            map[MainScenePath] = new EditorBuildSettingsScene(MainScenePath, true);

            var ordered = new List<EditorBuildSettingsScene>
            {
                map[BootstrapScenePath]
            };

            foreach (var s in map.Values)
            {
                if (s.path == BootstrapScenePath) continue;
                ordered.Add(s);
            }

            EditorBuildSettings.scenes = ordered.ToArray();
        }

        private static bool SceneInBuild(string path)
        {
            foreach (var scene in EditorBuildSettings.scenes)
                if (scene.path == path) return true;
            return false;
        }

        private static bool SceneInBuildAtIndex(string path, int index)
        {
            var scenes = EditorBuildSettings.scenes;
            return index >= 0 && index < scenes.Length && scenes[index].path == path;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;

            var parts = path.Split('/');
            var current = parts[0];
            for (var i = 1; i < parts.Length; i++)
            {
                var next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
#endif
