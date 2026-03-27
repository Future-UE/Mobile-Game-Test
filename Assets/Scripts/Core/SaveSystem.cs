using System.IO;
using UnityEngine;
using GameDevStudio.Models;

namespace GameDevStudio.Core
{
    /// <summary>
    /// Handles serialising and deserialising the complete game state to/from
    /// a JSON file in Application.persistentDataPath.
    /// </summary>
    public class SaveSystem
    {
        private const string FileName = "save.json";

        private string SavePath => Path.Combine(Application.persistentDataPath, FileName);

        // ── Save data container ───────────────────────────────────────────────
        [System.Serializable]
        private class SaveData
        {
            public StudioStats   Studio;
            public int           Week;
            public int           Month;
            public int           Year;
            public Employee[]    Employees;
            public GameProject[] Projects;
            public ResearchNode[]ResearchNodes;
        }

        // ── Public API ────────────────────────────────────────────────────────
        public bool HasSaveFile() => File.Exists(SavePath);

        public void Save()
        {
            var gm = GameManager.Instance;
            var data = new SaveData
            {
                Studio        = gm.Studio.Stats,
                Week          = gm.Time.CurrentWeek,
                Month         = gm.Time.CurrentMonth,
                Year          = gm.Time.CurrentYear,
                Employees     = gm.Staff.GetAllEmployees(),
                Projects      = gm.Projects.GetAllProjects(),
                ResearchNodes = gm.Research.GetAllNodes()
            };

            string json = JsonUtility.ToJson(data, prettyPrint: true);
            File.WriteAllText(SavePath, json);
            Debug.Log($"[SaveSystem] Game saved to {SavePath}");
        }

        public void Load()
        {
            if (!HasSaveFile())
            {
                Debug.LogWarning("[SaveSystem] No save file found.");
                return;
            }

            string json = File.ReadAllText(SavePath);
            var data = JsonUtility.FromJson<SaveData>(json);

            var gm = GameManager.Instance;
            gm.Studio.RestoreStats(data.Studio);
            gm.Time.RestoreState(data.Week, data.Month, data.Year);
            gm.Staff.RestoreEmployees(data.Employees);
            gm.Projects.RestoreProjects(data.Projects);
            gm.Research.RestoreNodes(data.ResearchNodes);

            Debug.Log("[SaveSystem] Game loaded.");
        }

        public void DeleteSave()
        {
            if (HasSaveFile())
                File.Delete(SavePath);
        }
    }
}
