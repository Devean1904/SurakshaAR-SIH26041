using UnityEngine;

public class AppBootstrap : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(gameObject);

        if (ThemeManager.Instance == null)
        {
            var themeObj = new GameObject("ThemeManager");
            themeObj.AddComponent<ThemeManager>();
        }

        if (LanguageManager.Instance == null)
        {
            var langObj = new GameObject("LanguageManager");
            langObj.AddComponent<LanguageManager>();
        }

        if (LocalBlockchain.Instance == null)
        {
            var bcObj = new GameObject("LocalBlockchain");
            bcObj.AddComponent<LocalBlockchain>();
        }

        if (OfflineManager.Instance == null)
        {
            var offlineObj = new GameObject("OfflineManager");
            offlineObj.AddComponent<OfflineManager>();
        }

        if (VoiceModule.Instance == null)
        {
            var voiceObj = new GameObject("VoiceModule");
            voiceObj.AddComponent<VoiceModule>();
        }

        if (EscalationManager.Instance == null)
        {
            var escObj = new GameObject("EscalationManager");
            escObj.AddComponent<EscalationManager>();
        }

        if (ARSiteMapper.Instance == null)
        {
            var siteObj = new GameObject("ARSiteMapper");
            siteObj.AddComponent<ARSiteMapper>();
        }

        if (ARScenarioPlayer.Instance == null)
        {
            var scenObj = new GameObject("ARScenarioPlayer");
            scenObj.AddComponent<ARScenarioPlayer>();
        }

        if (CitificationManager.Instance == null)
        {
            var citiObj = new GameObject("CitificationManager");
            citiObj.AddComponent<CitificationManager>();
        }

        if (UINavigator.Instance == null)
        {
            var uiObj = new GameObject("UINavigator");
            uiObj.AddComponent<UINavigator>();
        }

        if (QualityManager.Instance == null)
        {
            var qualityObj = new GameObject("QualityManager");
            qualityObj.AddComponent<QualityManager>();
        }

        if (QualityApplier.Instance == null)
        {
            var applierObj = new GameObject("QualityApplier");
            applierObj.AddComponent<QualityApplier>();
        }

        if (SceneLoader.Instance == null)
        {
            var sceneObj = new GameObject("SceneLoader");
            sceneObj.AddComponent<SceneLoader>();
        }

        Debug.Log("[App] SurakshaAR initialized - All systems ready");
    }
}
