using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameDevStudio.Core
{
    /// <summary>
    /// Entry-point MonoBehaviour placed in the Bootstrap scene.
    /// Ensures the GameManager and UI are initialised before the main scene loads.
    /// 
    /// Scene setup (Unity Editor):
    ///   1. Create a scene called "Bootstrap".
    ///   2. Add a GameObject with this script.
    ///   3. Set mainSceneName to "Main" (or your game scene name).
    ///   4. Set Bootstrap as the first scene in Build Settings.
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        [Tooltip("Name of the scene that contains the main game UI and GameManager.")]
        public string MainSceneName = "Main";

        private void Awake()
        {
            // Load the main scene additively if not already loaded
            if (!SceneManager.GetSceneByName(MainSceneName).isLoaded)
                SceneManager.LoadScene(MainSceneName, LoadSceneMode.Additive);
        }
    }
}
