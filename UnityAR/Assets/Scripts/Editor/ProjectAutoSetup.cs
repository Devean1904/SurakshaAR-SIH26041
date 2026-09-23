using UnityEngine;
using UnityEditor;
using System.IO;

namespace SurakshaAR.Editor
{
    [InitializeOnLoad]
    public static class ProjectAutoSetup
    {
        private const string SETUP_DONE_KEY = "SurakshaAR_AutoSetup_Done";

        static ProjectAutoSetup()
        {
            if (!SessionState.GetBool(SETUP_DONE_KEY, false))
            {
                EditorApplication.delayCall += RunFirstTimeSetup;
            }
        }

        [MenuItem("SurakshaAR/Run Full Project Setup")]
        static void RunFirstTimeSetup()
        {
            Debug.Log("=== SurakshaAR: Running Full Project Setup ===");
            CreateDirectories();
            PrefabGenerator.GenerateAllPrefabs();
            SetupBuildSettings();
            SessionState.SetBool(SETUP_DONE_KEY, true);
            Debug.Log("=== SurakshaAR: Setup Complete! ===");
            Debug.Log("Menu items available under SurakshaAR/");
        }

        static void CreateDirectories()
        {
            string[] dirs = {
                "Assets/Scenes",
                "Assets/Prefabs",
                "Assets/Prefabs/Scenarios",
                "Assets/Prefabs/Panels",
                "Assets/Resources",
                "Assets/StreamingAssets",
                "Assets/StreamingAssets/Lang",
                "Builds"
            };
            foreach (var dir in dirs)
            {
                if (!AssetDatabase.IsValidFolder(dir))
                {
                    var parts = dir.Split("/");
                    var current = parts[0];
                    for (int i = 1; i < parts.Length; i++)
                    {
                        var next = current + "/" + parts[i];
                        if (!AssetDatabase.IsValidFolder(next))
                            AssetDatabase.CreateFolder(current, parts[i]);
                        current = next;
                    }
                }
            }
            Debug.Log("[Setup] Directories created");
        }

        static void SetupBuildSettings()
        {
            var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>();
            string[] sceneNames = { "LoginScene", "MainScene" };
            foreach (var name in sceneNames)
            {
                string path = $"Assets/Scenes/{name}.unity";
                if (File.Exists(path))
                    scenes.Add(new EditorBuildSettingsScene(path, true));
            }
            if (scenes.Count > 0)
            {
                EditorBuildSettings.scenes = scenes.ToArray();
                Debug.Log($"[Setup] Build settings: {scenes.Count} scenes added");
            }
        }
    }
}

