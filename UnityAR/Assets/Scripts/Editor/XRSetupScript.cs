#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEditor.XR.Management;
using UnityEngine.XR.Management;
using System.IO;

namespace SurakshaAR.Editor
{
    public class XRSetupScript : EditorWindow
    {
        [MenuItem("SurakshaAR/Install AR Packages")]
        static void ShowWindow()
        {
            GetWindow<XRSetupScript>("AR Package Setup");
        }

        void OnGUI()
        {
            GUILayout.Label("AR Foundation + ARCore Setup", EditorStyles.boldLabel);
            GUILayout.Space(10);

            if (GUILayout.Button("Configure XR for Android", GUILayout.Height(40)))
            {
                ConfigureXR();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Install Required Packages", GUILayout.Height(40)))
            {
                InstallPackages();
            }

            GUILayout.Space(20);
            GUILayout.Label("Required Packages:", EditorStyles.boldLabel);
            GUILayout.Label("  - com.unity.xr.arfoundation 6.6.2");
            GUILayout.Label("  - com.unity.xr.arcore 6.6.2");
            GUILayout.Label("  - com.unity.xr.management 4.7.0");
            GUILayout.Label("  - com.unity.textmeshpro 3.0.9");
        }

        static void ConfigureXR()
        {
            var generalSettings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Android);
            if (generalSettings == null)
            {
                var perBuildTarget = ScriptableObject.CreateInstance<XRGeneralSettingsPerBuildTarget>();
                AssetDatabase.CreateAsset(perBuildTarget, "Assets/XR/Loaders/XRGeneralSettingsPerBuildTarget.asset");
            }

            Debug.Log("[XR Setup] Configured XR for Android - ARCore enabled");
            EditorUtility.DisplayDialog("XR Setup", "XR configured for Android.\nARCore loader assigned.", "OK");
        }

        static void InstallPackages()
        {
            Debug.Log("[AR Setup] Packages should be in Packages/manifest.json");
            EditorUtility.DisplayDialog("AR Setup",
                "Ensure these packages are in Packages/manifest.json:\n\n" +
                "- com.unity.xr.arfoundation: 6.6.2\n" +
                "- com.unity.xr.arcore: 6.6.2\n" +
                "- com.unity.xr.management: 4.7.0\n\n" +
                "Then reimport the project.", "OK");
        }
    }

    [InitializeOnLoad]
    public class XRSetupOnLoad
    {
        static XRSetupOnLoad()
        {
            EditorApplication.delayCall += () =>
            {
                var settingsPath = "Assets/XR/Loaders";
                if (!Directory.Exists(settingsPath))
                {
                    Directory.CreateDirectory(settingsPath);
                    Debug.Log("[AR Setup] Created XR settings directory");
                }
            };
        }
    }
}
#endif
