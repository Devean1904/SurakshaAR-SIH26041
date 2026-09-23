using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class MainSceneSetup
{
    static T Add<T>(GameObject go) where T : Component => go.AddComponent<T>();

    [MenuItem("Tools/SurakshaAR/Build Main Scene")]
    public static void Build()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var root = UI.Canvas("MainCanvas");
        UI.EventSys();

        CreateTopNavBar(root.transform);
        CreateWorkerPanel(root.transform);
        CreateAdminPanel(root.transform);
        CreateManagerPanel(root.transform);
        CreateSettingsPanel(root.transform);
        AddManagers(root);

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/MainScene.unity");
        Debug.Log("MainScene built successfully!");
    }

    private static void CreateTopNavBar(Transform p)
    {
        var navBar = UI.PA(p, "TopNavBar", UI.Surface, 0, 0, 1, 0.08f);
        UI.T(navBar.transform, "AppName", "SurakshaAR", 24, UI.White, 0, 0, 0.35f, 1);
        UI.B(navBar.transform, "NotificationBtn", "\u2764", UI.Primary, 0.72f, 0.1f, 0.82f, 0.9f, 18);
        UI.B(navBar.transform, "ProfileBtn", "\u2764", UI.Primary, 0.84f, 0.1f, 0.92f, 0.9f, 18);
        UI.B(navBar.transform, "LanguageBtn", "EN", UI.Primary, 0.93f, 0.15f, 0.98f, 0.85f, 14);
    }

    private static void CreateWorkerPanel(Transform p)
    {
        var workerPanel = UI.PA(p, "WorkerPanel", UI.BgDark, 0, 0.08f, 1, 0.88f);

        UI.TA(workerPanel.transform, "WelcomeText", "Welcome, Worker!", 22, UI.White, 0.05f, 0.92f, 0.7f, 0.98f);
        UI.T(workerPanel.transform, "RoleBadge", "\u2764 Worker", 14, UI.Primary, 0.72f, 0.92f, 0.95f, 0.98f);
        UI.T(workerPanel.transform, "StatusText", "", 12, UI.TextMuted, 0.05f, 0.88f, 0.95f, 0.92f);

        UI.Stat(workerPanel.transform, "ModulesStat", "12", "Modules", UI.Primary, 0.05f, 0.78f, 0.35f, 0.9f);
        UI.Stat(workerPanel.transform, "CertificatesStat", "5", "Certificates", UI.Success, 0.37f, 0.78f, 0.63f, 0.9f);
        UI.Stat(workerPanel.transform, "DayStreakStat", "7", "Day Streak", UI.Warning, 0.65f, 0.78f, 0.95f, 0.9f);

        UI.TA(workerPanel.transform, "TrainingDomainsHeader", "Training Domains", 18, UI.TextSec, 0.05f, 0.7f, 0.95f, 0.77f);

        var domainListObj = UI.PA(workerPanel.transform, "DomainList", Color.clear, 0.03f, 0.14f, 0.97f, 0.69f);
        var dll = domainListObj.AddComponent<VerticalLayoutGroup>();
        dll.spacing = 8;
        dll.childAlignment = TextAnchor.UpperCenter;
        dll.childForceExpandWidth = true;
        dll.childForceExpandHeight = false;
        dll.padding = new RectOffset(0, 0, 4, 4);
        domainListObj.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        UI.TA(workerPanel.transform, "QuickActionsHeader", "Quick Actions", 16, UI.TextSec, 0.05f, 0.1f, 0.95f, 0.14f);
        UI.B(workerPanel.transform, "ResumeTrainingBtn", "Resume Training", UI.Primary, 0.05f, 0.04f, 0.48f, 0.09f, 14);
        UI.B(workerPanel.transform, "ViewCertsBtn", "View Certificates", UI.Success, 0.52f, 0.04f, 0.95f, 0.09f, 14);
        UI.B(workerPanel.transform, "QRScanBtn", "QR Scan", UI.Warning, 0.05f, -0.02f, 0.48f, 0.03f, 14);
        UI.B(workerPanel.transform, "SettingsBtn", "Settings", UI.Danger, 0.52f, -0.02f, 0.95f, 0.03f, 14);

        var bottomNav = UI.PA(workerPanel.transform, "BottomNavBar", UI.Surface, 0, -0.08f, 1, 0);
        UI.B(bottomNav.transform, "HomeBtn", "Home", UI.Primary, 0, 0, 0.25f, 1, 14);
        UI.B(bottomNav.transform, "TrainingBtn", "Training", UI.Primary, 0.25f, 0, 0.5f, 1, 14);
        UI.B(bottomNav.transform, "CertsBtn", "Certs", UI.Primary, 0.5f, 0, 0.75f, 1, 14);
        UI.B(bottomNav.transform, "ProfileBtnNav", "Profile", UI.Primary, 0.75f, 0, 1, 1, 14);
    }

    private static void CreateAdminPanel(Transform p)
    {
        var adminPanel = UI.PA(p, "AdminPanel", UI.BgDark, 0, 0.08f, 1, 0.88f);
        adminPanel.SetActive(false);
    }

    private static void CreateManagerPanel(Transform p)
    {
        var managerPanel = UI.PA(p, "ManagerPanel", UI.BgDark, 0, 0.08f, 1, 0.88f);
        managerPanel.SetActive(false);
    }

    private static void CreateSettingsPanel(Transform p)
    {
        var settingsPanel = UI.PA(p, "SettingsPanel", UI.BgDark, 0, 0.08f, 1, 0.88f);
        settingsPanel.SetActive(false);
        UI.B(settingsPanel.transform, "BackBtn", "\u2190 Back", UI.Primary, 0.05f, 0.92f, 0.3f, 0.98f, 16);
        UI.TA(settingsPanel.transform, "SettingsTitle", "Settings", 24, UI.White, 0.05f, 0.82f, 0.95f, 0.9f);
    }

    private static void AddManagers(GameObject parent)
    {
        parent.AddComponent<DashboardManager>();
        parent.AddComponent<UINavigator>();
        parent.AddComponent<AppBootstrap>();
        parent.AddComponent<SceneLoader>();
        parent.AddComponent<ThemeManager>();
        parent.AddComponent<LanguageManager>();
        parent.AddComponent<LocalBlockchain>();
        parent.AddComponent<OfflineManager>();
        parent.AddComponent<VoiceModule>();
        parent.AddComponent<EscalationManager>();
        parent.AddComponent<QualityManager>();
    }
}
