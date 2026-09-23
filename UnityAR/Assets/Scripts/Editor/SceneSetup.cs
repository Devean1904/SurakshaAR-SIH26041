using UnityEngine;
using UnityEditor;

namespace SurakshaAR.Editor
{
    public static class SceneSetup
    {
        [MenuItem("SurakshaAR/Scene Setup/Create Scenes Folder")]
        static void CreateScenesFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
                AssetDatabase.CreateFolder("Assets", "Scenes");
            Debug.Log("[SceneSetup] Assets/Scenes folder ready");
        }

        [MenuItem("SurakshaAR/Scene Setup/Create Manager Panels")]
        static void CreateManagerPanels()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs/Panels"))
                AssetDatabase.CreateFolder("Assets/Prefabs", "Panels");
            string[] panels = { "AdminPanel", "ManagerPanel", "WorkerPanel", "SettingsPanel" };
            foreach (var name in panels)
            {
                var go = new GameObject(name);
                var rect = go.AddComponent<RectTransform>();
                PrefabUtility.SaveAsPrefabAsset(go, $"Assets/Prefabs/Panels/{name}.prefab");
                Object.DestroyImmediate(go);
            }
            Debug.Log("[SceneSetup] Created panel prefabs");
        }
    }
}

