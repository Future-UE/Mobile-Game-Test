using System.Collections.Generic;
using UnityEngine;

namespace GameDevStudio.Data
{
    /// <summary>
    /// Central registry for all game content loaded from Resources.
    /// Provides O(1) lookup by id after initialisation.
    /// Add new content by creating ScriptableObject assets in the correct
    /// Resources sub-folder — no code changes required.
    /// </summary>
    public class GameDatabase
    {
        // ── Singleton ─────────────────────────────────────────────────────────
        private static GameDatabase _instance;
        public  static GameDatabase Instance => _instance ??= new GameDatabase();

        // ── Dictionaries ──────────────────────────────────────────────────────
        private Dictionary<string, GenreData>       _genres       = new();
        private Dictionary<string, PlatformData>    _platforms    = new();
        private Dictionary<string, StaffRoleData>   _staffRoles   = new();
        private Dictionary<string, ResearchNodeData>_researchNodes= new();
        private Dictionary<string, RandomEventData> _events       = new();

        // ── Public collections ────────────────────────────────────────────────
        public IReadOnlyDictionary<string, GenreData>        Genres        => _genres;
        public IReadOnlyDictionary<string, PlatformData>     Platforms     => _platforms;
        public IReadOnlyDictionary<string, StaffRoleData>    StaffRoles    => _staffRoles;
        public IReadOnlyDictionary<string, ResearchNodeData> ResearchNodes => _researchNodes;
        public IReadOnlyDictionary<string, RandomEventData>  Events        => _events;

        // ── Initialisation ────────────────────────────────────────────────────
        /// <summary>
        /// Loads all ScriptableObjects from Resources.
        /// Call once during startup (e.g. from GameManager.Awake).
        /// </summary>
        public void LoadAll()
        {
            LoadCategory<GenreData>      ("Data/Genres",       _genres,        d => d.GenreId);
            LoadCategory<PlatformData>   ("Data/Platforms",    _platforms,     d => d.PlatformId);
            LoadCategory<StaffRoleData>  ("Data/StaffRoles",   _staffRoles,    d => d.RoleId);
            LoadCategory<ResearchNodeData>("Data/Research",    _researchNodes, d => d.NodeId);
            LoadCategory<RandomEventData>("Data/Events",       _events,        d => d.EventId);

            Debug.Log($"[GameDatabase] Loaded: {_genres.Count} genres, {_platforms.Count} platforms, " +
                      $"{_staffRoles.Count} roles, {_researchNodes.Count} research nodes, {_events.Count} events.");
        }

        private void LoadCategory<T>(string path, Dictionary<string, T> dict,
                                     System.Func<T, string> idSelector) where T : ScriptableObject
        {
            dict.Clear();
            var assets = Resources.LoadAll<T>(path);
            foreach (var asset in assets)
            {
                string id = idSelector(asset);
                if (string.IsNullOrEmpty(id))
                {
                    Debug.LogWarning($"[GameDatabase] {typeof(T).Name} '{asset.name}' has no Id set. Skipping.");
                    continue;
                }
                if (dict.ContainsKey(id))
                {
                    Debug.LogWarning($"[GameDatabase] Duplicate id '{id}' for {typeof(T).Name}. Skipping.");
                    continue;
                }
                dict[id] = asset;
            }
        }

        // ── Convenience getters ───────────────────────────────────────────────
        public GenreData        GetGenre      (string id) { _genres.TryGetValue(id, out var v);       return v; }
        public PlatformData     GetPlatform   (string id) { _platforms.TryGetValue(id, out var v);    return v; }
        public StaffRoleData    GetStaffRole  (string id) { _staffRoles.TryGetValue(id, out var v);   return v; }
        public ResearchNodeData GetResearch   (string id) { _researchNodes.TryGetValue(id, out var v);return v; }
        public RandomEventData  GetEvent      (string id) { _events.TryGetValue(id, out var v);       return v; }
    }
}
