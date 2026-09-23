using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

[UnityEditor.InitializeOnLoad]
public static class ARSceneSetup
{
    static ARSceneSetup()
    {
        EditorApplication.delayCall += OnDelayCall;
    }

    static void OnDelayCall()
    {
        Menu.SetChecked("AR/Setup ARScene", false);
    }

    [MenuItem("AR/Setup ARScene")]
    public static void Build()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        SetupCamera();
        SetupARManagers();
        SetupTraining();
        SetupCanvas();

        string dir = "Assets/Scenes";
        if (!AssetDatabase.IsValidFolder(dir))
            AssetDatabase.CreateFolder("Assets", "Scenes");
        EditorSceneManager.SaveScene(scene, dir + "/ARScene.unity");
        Debug.Log("[AR] Scene saved to " + dir + "/ARScene.unity");
    }

    static void SetupCamera()
    {
        var cam = Camera.main;
        if (cam == null)
        {
            var go = new GameObject("Main Camera");
            go.tag = "MainCamera";
            cam = go.AddComponent<Camera>();
        }
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
    }

    static void SetupARManagers()
    {
        var go = new GameObject("ARManagers");
        TryAddComponent(go, "SurfaceDetector");
        TryAddComponent(go, "ObjectClassifier");
        TryAddComponent(go, "ContextualPlacer");
        TryAddComponent(go, "MineHazardLibrary");
        TryAddComponent(go, "MineObjectFactory");
        TryAddComponent(go, "ARScenarioPlayer");
    }

    static void SetupTraining()
    {
        var go = new GameObject("Training");
        TryAddComponent(go, "ScenarioManager");
        TryAddComponent(go, "FireSimulation");
        TryAddComponent(go, "GasLeakSimulation");
        TryAddComponent(go, "HUDManager");
        TryAddComponent(go, "AssessmentEngine");
        TryAddComponent(go, "EscalationEngine");
        TryAddComponent(go, "CertificateGenerator");
    }

    static void SetupCanvas()
    {
        var canvas = UI.Canvas("ARCanvas");
        UI.EventSys();
        var root = canvas.transform;

        BuildHUDPanel(root);
        BuildAlertPanel(root);
        BuildCriticalFailPanel(root);
        BuildEvacuationPanel(root);
        BuildScoreResultsPanel(root);
        BuildAssessmentPanel(root);
    }

    static void BuildHUDPanel(Transform root)
    {
        var panel = UI.PA(root, "HUDPanel", UI.BgDark, 0, 0, 1, 1);
        UI.SA(panel, 0, 0, 1, 1);
        panel.GetComponent<RectTransform>().offsetMin = Vector2.zero;
        panel.GetComponent<RectTransform>().offsetMax = Vector2.zero;

        var topBar = UI.PA(panel.transform, "TopBar", UI.Surface, 0, 1f, 1, 0.93f);
        UI.SA(topBar, 0, 1, 1, 1);
        topBar.GetComponent<RectTransform>().offsetMin = new Vector2(0, topBar.GetComponent<RectTransform>().offsetMin.y);
        topBar.GetComponent<RectTransform>().offsetMax = new Vector2(0, topBar.GetComponent<RectTransform>().offsetMax.y);

        UI.T(topBar.transform, "HazardTypeText", "Fire", 14, UI.Warning,
            0, 0, 0.33f, 1);
        UI.T(topBar.transform, "HazardLevelText", "Level 2", 14, UI.Danger,
            0.33f, 0, 0.66f, 1);
        UI.T(topBar.transform, "TimerText", "01:00", 28, UI.White,
            0.33f, 0, 0.66f, 1);

        var timerBg = UI.PA(panel.transform, "TimerBarBg", UI.CardBg, 0, 0.88f, 1, 0.91f);
        UI.SA(timerBg, 0, 1, 1, 1);
        timerBg.GetComponent<RectTransform>().offsetMin = new Vector2(20, timerBg.GetComponent<RectTransform>().offsetMin.y);
        timerBg.GetComponent<RectTransform>().offsetMax = new Vector2(-20, timerBg.GetComponent<RectTransform>().offsetMax.y);

        var timerFill = UI.PA(timerBg.transform, "TimerBarFill", UI.Success, 0, 0, 1, 1);
        UI.SA(timerFill, 0, 0, 1, 1);
        UI.SA(timerFill, 0, 0, 0.8f, 1);

        UI.T(panel.transform, "ScoreText", "Score: 100", 16, UI.Success,
            0, 0.7f, 0.35f, 0.85f);
        UI.T(panel.transform, "PenaltyText", "-0", 14, UI.Danger,
            0.35f, 0.7f, 0.5f, 0.85f);

        UI.T(panel.transform, "WarningInstructionsText",
            "Identify the fire hazard in the area.", 13, UI.Warning,
            0, 0.3f, 1, 0.65f);

        var actionContainer = UI.PA(panel.transform, "ActionButtonContainer", UI.CardBg, 0, 0, 1, 0.25f);
        UI.SA(actionContainer, 0, 0, 1, 1);
        actionContainer.GetComponent<RectTransform>().offsetMin = new Vector2(20, 20);
        actionContainer.GetComponent<RectTransform>().offsetMax = new Vector2(-20, 0);

        UI.B(actionContainer.transform, "ActionButton1", "Extinguish", UI.Primary,
            0, 0, 0.32f, 1, 12);
        UI.B(actionContainer.transform, "ActionButton2", "Evacuate", UI.Warning,
            0.34f, 0, 0.66f, 1, 12);
        UI.B(actionContainer.transform, "ActionButton3", "Report", UI.Danger,
            0.68f, 0, 1, 1, 12);
    }

    static void BuildAlertPanel(Transform root)
    {
        var panel = UI.PA(root, "AlertPanel", UI.Warning, 0, 0, 1, 1);
        UI.SA(panel, 0, 0, 1, 1);
        panel.SetActive(false);

        UI.T(panel.transform, "AlertText", "WARNING!", 36, UI.White,
            0, 0, 1, 1);
    }

    static void BuildCriticalFailPanel(Transform root)
    {
        var panel = UI.PA(root, "CriticalFailPanel", UI.Danger, 0, 0, 1, 1);
        UI.SA(panel, 0, 0, 1, 1);
        panel.SetActive(false);

        UI.T(panel.transform, "CriticalFailTitle", "SCENARIO FAILED", 30, UI.White,
            0, 0.65f, 1, 0.8f);
        UI.T(panel.transform, "CriticalFailMessage", "You have failed the scenario.", 16, UI.TextSec,
            0, 0.5f, 1, 0.65f);
        UI.T(panel.transform, "CriticalFailScore", "Score: 0", 20, UI.White,
            0, 0.38f, 1, 0.5f);

        UI.B(panel.transform, "RetryButton", "Retry", UI.Primary,
            0.1f, 0.15f, 0.45f, 0.28f, 14);
        UI.B(panel.transform, "ViewResultsButton", "View Results", UI.CardBg,
            0.55f, 0.15f, 0.9f, 0.28f, 14);
    }

    static void BuildEvacuationPanel(Transform root)
    {
        var panel = UI.PA(root, "EvacuationPanel", UI.Primary, 0, 0, 1, 1);
        UI.SA(panel, 0, 0, 1, 1);
        panel.SetActive(false);

        UI.T(panel.transform, "EvacuationText", "EVACUATE IMMEDIATELY!", 34, UI.White,
            0, 0, 1, 1);
    }

    static void BuildScoreResultsPanel(Transform root)
    {
        var panel = UI.PA(root, "ScoreResultsPanel", UI.BgDark, 0, 0, 1, 1);
        UI.SA(panel, 0, 0, 1, 1);
        panel.SetActive(false);

        UI.T(panel.transform, "ResultTitle", "PASSED", 36, UI.Success,
            0, 0.75f, 1, 0.9f);

        UI.T(panel.transform, "ActionScoreText", "Actions: 100", 16, UI.TextSec,
            0, 0.62f, 1, 0.72f);
        UI.T(panel.transform, "QuestionScoreText", "Questions: 100", 16, UI.TextSec,
            0, 0.52f, 1, 0.62f);
        UI.T(panel.transform, "TotalScoreText", "Total: 200", 22, UI.White,
            0, 0.4f, 1, 0.52f);
        UI.T(panel.transform, "ResultMessageText", "Excellent work!", 14, UI.TextMuted,
            0, 0.3f, 1, 0.4f);

        UI.B(panel.transform, "ViewCertificateButton", "View Certificate", UI.Success,
            0.05f, 0.1f, 0.32f, 0.22f, 12);
        UI.B(panel.transform, "RetryButton2", "Retry", UI.Primary,
            0.38f, 0.1f, 0.62f, 0.22f, 12);
        UI.B(panel.transform, "HomeButton", "Home", UI.CardBg,
            0.68f, 0.1f, 0.95f, 0.22f, 12);
    }

    static void BuildAssessmentPanel(Transform root)
    {
        var panel = UI.PA(root, "AssessmentPanel", UI.BgDark, 0, 0, 1, 1);
        UI.SA(panel, 0, 0, 1, 1);
        panel.SetActive(false);

        var qPanel = UI.PA(panel.transform, "QuestionPanel", UI.CardBg, 0, 0, 1, 1);
        UI.SA(qPanel, 0, 0, 1, 1);
        qPanel.GetComponent<RectTransform>().offsetMin = new Vector2(40, 40);
        qPanel.GetComponent<RectTransform>().offsetMax = new Vector2(-40, -40);

        UI.T(qPanel.transform, "QuestionNumberText", "Question 1 of 5", 14, UI.TextMuted,
            0, 0.9f, 1, 1f);
        UI.T(qPanel.transform, "QuestionText", "What is the hazard?", 22, UI.White,
            0, 0.7f, 1, 0.88f);

        var progBg = UI.PA(qPanel.transform, "QProgressBg", UI.Surface, 0, 0.65f, 1, 0.68f);
        UI.SA(progBg, 0, 1, 1, 1);
        progBg.GetComponent<RectTransform>().offsetMin = new Vector2(0, progBg.GetComponent<RectTransform>().offsetMin.y);
        progBg.GetComponent<RectTransform>().offsetMax = new Vector2(0, progBg.GetComponent<RectTransform>().offsetMax.y);

        var progFill = UI.PA(progBg.transform, "QProgressFill", UI.Primary, 0, 0, 0.4f, 1);
        UI.SA(progFill, 0, 0, 0.4f, 1);

        UI.B(qPanel.transform, "Option1", "A) Fire extinguisher", UI.BgDark,
            0, 0.42f, 1, 0.54f, 14);
        UI.B(qPanel.transform, "Option2", "B) Gas detector", UI.BgDark,
            0, 0.3f, 1, 0.42f, 14);
        UI.B(qPanel.transform, "Option3", "C) Emergency alarm", UI.BgDark,
            0, 0.18f, 1, 0.3f, 14);
        UI.B(qPanel.transform, "Option4", "D) Safety sign", UI.BgDark,
            0, 0.06f, 1, 0.18f, 14);

        UI.B(qPanel.transform, "NextButton", "Next", UI.Primary,
            0.75f, -0.02f, 1f, 0.05f, 14);
    }

    static void TryAddComponent(GameObject go, string typeName)
    {
        var type = FindType(typeName);
        if (type != null)
            go.AddComponent(type);
    }

    static System.Type FindType(string name)
    {
        foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
        {
            var t = asm.GetType(name);
            if (t != null) return t;
        }
        return null;
    }
}
