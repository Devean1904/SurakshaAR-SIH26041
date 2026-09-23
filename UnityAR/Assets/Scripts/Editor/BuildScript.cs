#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

namespace SurakshaAR.Editor
{
    public class BuildScript
    {
        private static readonly string APK_OUTPUT = "Builds/SurakshaAR.apk";

        [MenuItem("SurakshaAR/Build Android APK")]
        public static void BuildAndroid()
        {
            var scenes = new[]
            {
                "Assets/Scenes/LoginScene.unity",
                "Assets/Scenes/MainScene.unity"
            };

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = APK_OUTPUT,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            BuildPipeline.BuildPlayer(options);
            Debug.Log($"[Build] APK built to: {APK_OUTPUT}");
        }

        [MenuItem("SurakshaAR/Build Android AAB")]
        public static void BuildAndroidAAB()
        {
            EditorUserBuildSettings.buildAppBundle = true;
            BuildAndroid();
            EditorUserBuildSettings.buildAppBundle = false;
        }

        [MenuItem("SurakshaAR/Open Build Folder")]
        public static void OpenBuildFolder()
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "Builds");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            EditorUtility.RevealInFinder(path);
        }
    }
}
#endif
