using UnityEngine;
using UnityEditor;
using UnityEditor.Build;

namespace SurakshaAR.Editor
{
    [InitializeOnLoad]
    public static class AndroidBuildConfig
    {
        static AndroidBuildConfig()
        {
            EditorApplication.delayCall += ConfigureAndroid;
        }

        [MenuItem("SurakshaAR/Android Build/Configure Settings")]
        static void ConfigureAndroid()
        {
            var androidTarget = NamedBuildTarget.Android;
            PlayerSettings.SetApplicationIdentifier(androidTarget, "com.surakshaar.app");
            PlayerSettings.productName = "SurakshaAR";
            PlayerSettings.bundleVersion = "1.0.0";

            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel29;
            PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)35;
            PlayerSettings.SetScriptingBackend(androidTarget, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;

            PlayerSettings.Android.forceInternetPermission = true;
            PlayerSettings.Android.forceSDCardPermission = false;

            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3 });

            PlayerSettings.Android.keyaliasName = "";
            PlayerSettings.Android.keystoreName = "";

            Debug.Log("[Android Config] Settings configured:");
            Debug.Log($"  Package: com.surakshaar.app");
            Debug.Log($"  Min SDK: 29 (Android 10)");
            Debug.Log($"  Target SDK: 35");
            Debug.Log($"  Scripting: IL2CPP");
            Debug.Log($"  Architecture: ARM64");
            Debug.Log($"  Graphics: OpenGLES3");
        }

        [MenuItem("SurakshaAR/Android Build/Validate Permissions")]
        static void ValidatePermissions()
        {
            Debug.Log("--- Android Permission Check ---");

            bool hasInternet = PlayerSettings.Android.forceInternetPermission;
            Debug.Log($"Internet Permission: {(hasInternet ? "OK" : "MISSING - Required for API calls")}");

            Debug.Log("Required Android Manifest Permissions:");
            Debug.Log("  android.permission.INTERNET");
            Debug.Log("  android.permission.CAMERA");
            Debug.Log("  android.permission.ACCESS_FINE_LOCATION");
            Debug.Log("  android.permission.ACCESS_COARSE_LOCATION");
            Debug.Log("  android.permission.READ_PHONE_STATE");
            Debug.Log("  android.permission.SEND_SMS");
            Debug.Log("  android.permission.RECEIVE_SMS");

            Debug.Log("--- Validation Complete ---");
        }

        [MenuItem("SurakshaAR/Android Build/Open Build Settings")]
        static void OpenBuildSettings()
        {
            EditorWindow.GetWindow(System.Type.GetType("UnityEditor.BuildPlayerWindow, UnityEditor"));
        }

        [MenuItem("SurakshaAR/Android Build/Build APK")]
        static void BuildAPK()
        {
            var scenes = new[]
            {
                "Assets/Scenes/LoginScene.unity",
                "Assets/Scenes/MainScene.unity"
            };

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = "Builds/SurakshaAR.apk",
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            BuildPipeline.BuildPlayer(options);
            Debug.Log("[Build] APK build started");
        }

        [MenuItem("SurakshaAR/Android Build/Build AAB (Play Store)")]
        static void BuildAAB()
        {
            var scenes = new[]
            {
                "Assets/Scenes/LoginScene.unity",
                "Assets/Scenes/MainScene.unity"
            };

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = "Builds/SurakshaAR.aab",
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            BuildPipeline.BuildPlayer(options);
            Debug.Log("[Build] AAB build started");
        }
    }
}
