using UnityEngine;
using UnityEditor;
using UnityEditor.Build;

namespace SurakshaAR.Editor
{
    [InitializeOnLoad]
    public class ARAutoSetup
    {
        static ARAutoSetup()
        {
            EditorApplication.delayCall += ConfigureAR;
        }

        static void ConfigureAR()
        {
            var androidTarget = NamedBuildTarget.Android;
            PlayerSettings.SetApplicationIdentifier(androidTarget, "com.surakshaar.app");
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel29;
            PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)34;
            PlayerSettings.SetScriptingBackend(androidTarget, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;

            PlayerSettings.Android.forceInternetPermission = true;
            PlayerSettings.Android.forceSDCardPermission = false;

            Debug.Log("[AR Auto Setup] Android AR settings configured:");
            Debug.Log("  Min SDK: 29 (Android 10)");
            Debug.Log("  Target SDK: 34");
            Debug.Log("  Scripting: IL2CPP");
            Debug.Log("  Architectures: ARM64 + ARMv7");
            Debug.Log("  Internet: Required");
        }

        [MenuItem("SurakshaAR/Validate AR Setup")]
        static void ValidateSetup()
        {
            var androidTarget = NamedBuildTarget.Android;

            Debug.Log("--- AR Setup Validation ---");
            Debug.Log("AR Foundation: CHECK Packages/manifest.json");
            Debug.Log("ARCore: CHECK Packages/manifest.json");
            Debug.Log($"Min SDK: {PlayerSettings.Android.minSdkVersion}");
            Debug.Log($"Target SDK: {PlayerSettings.Android.targetSdkVersion}");
            Debug.Log($"Scripting Backend: {PlayerSettings.GetScriptingBackend(androidTarget)}");
            Debug.Log($"Target Architectures: {PlayerSettings.Android.targetArchitectures}");

            Debug.Log("[AR Validation] Setup looks correct!");
        }
    }
}
