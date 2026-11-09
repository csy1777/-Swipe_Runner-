#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KrolStudio
{
    [InitializeOnLoad]
    public static class AutoBootstrapLoader
    {
        private const string BootstrapScenePath = "Assets/KrolStudio/SwipeGunner/StaticAssets/Scenes/Bootstrap.unity"; // Path to the starting scene

        // Add a variable to enable/disable the functionality
        private static bool enableAutoBootstrap = true; // Set to false to disable the functionality

        static AutoBootstrapLoader()
        {
            // Subscribe to mode change event
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.ExitingEditMode)
                return;

            // If auto-bootstrap is disabled, do nothing
            if (!enableAutoBootstrap)
                return;

            // If the active scene is not Bootstrap, we substitute it as the starting one
            Scene activeScene = SceneManager.GetActiveScene();

            if (!activeScene.path.Equals(BootstrapScenePath))
            {
                var bootstrapScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(BootstrapScenePath);
                if (bootstrapScene != null)
                {
                    Debug.LogWarning($"[AutoBootstrapLoader] Scene \"{activeScene.name}\" is not the starting scene. Replace with \"{bootstrapScene.name}\".");
                    EditorSceneManager.playModeStartScene = bootstrapScene;
                }
                else
                {
                    Debug.LogError($"[AutoBootstrapLoader] Could not find scene on path: {BootstrapScenePath}");
                }
            }
            else
            {
                // If you are already on the desired scene, reset the custom start
                EditorSceneManager.playModeStartScene = null;
            }
        }

        // Method for changing the enableAutoBootstrap value in the editor
        [MenuItem("Tools/Toggle Auto Bootstrap")]
        public static void ToggleAutoBootstrap()
        {
            enableAutoBootstrap = !enableAutoBootstrap;
            string status = enableAutoBootstrap ? "enabled" : "disabled";
            Debug.Log($"<color=yellow>Auto Bootstrap has been {status}.</color>");
        }
    }
}
#endif
