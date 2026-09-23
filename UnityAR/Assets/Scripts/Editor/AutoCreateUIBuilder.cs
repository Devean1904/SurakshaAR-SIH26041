using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public class AutoCreateUIBuilder
{
    static AutoCreateUIBuilder()
    {
        EditorApplication.delayCall += OnEditorLoad;
    }

    static void OnEditorLoad()
    {
        if (EditorSceneManager.GetActiveScene().name == "LoginScene")
        {
            var existing = GameObject.FindObjectOfType<BuildUIFromCode>();
            if (existing == null)
            {
                GameObject go = new GameObject("BuildUIFromCode");
                go.AddComponent<BuildUIFromCode>();
                Debug.Log("[AutoSetup] Created BuildUIFromCode in LoginScene");
            }
        }
    }
}
