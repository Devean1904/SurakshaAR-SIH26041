using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace SurakshaAR.Editor
{
    public static class FullSceneSetup
    {
        [MenuItem("SurakshaAR/Create All Scenes")]
        public static void CreateAllScenes()
        {
            CreateLoginScene();
            CreateMainScene();
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("[SceneSetup] All scenes created and saved!");
        }

        [MenuItem("SurakshaAR/Create LoginScene")]
        public static void CreateLoginScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            var canvasObj = CreateCanvas("LoginCanvas");
            var canvas = canvasObj.GetComponent<Canvas>();

            var loginPanel = CreatePanel("LoginPanel", canvasObj.transform);
            CreateInputField("UserIdInput", loginPanel.transform, new Vector2(0, 80));
            CreateInputField("PasswordInput", loginPanel.transform, new Vector2(0, 20));
            CreateButton("LoginButton", loginPanel.transform, new Vector2(0, -60), "Login");
            CreateButton("OtpLoginButton", loginPanel.transform, new Vector2(0, -120), "Login with OTP");
            CreateText("StatusText", loginPanel.transform, new Vector2(0, -180), "");

            var otpPanel = CreatePanel("OtpPanel", canvasObj.transform);
            otpPanel.SetActive(false);
            CreateInputField("PhoneInput", otpPanel.transform, new Vector2(0, 80));
            CreateInputField("OtpInput", otpPanel.transform, new Vector2(0, 20));
            CreateButton("SendOtpButton", otpPanel.transform, new Vector2(0, -60), "Send OTP");
            CreateButton("VerifyOtpButton", otpPanel.transform, new Vector2(0, -120), "Verify OTP");

            var loginManager = new GameObject("LoginManager");
            loginManager.AddComponent<LoginManager>();

            var appBootstrap = new GameObject("AppBootstrap");
            appBootstrap.AddComponent<AppBootstrap>();

            string scenePath = "Assets/Scenes/LoginScene.unity";
            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
                AssetDatabase.CreateFolder("Assets", "Scenes");
            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"[SceneSetup] Created: {scenePath}");
        }

        [MenuItem("SurakshaAR/Create MainScene")]
        public static void CreateMainScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            var arSession = new GameObject("AR Session");
            arSession.AddComponent<UnityEngine.XR.ARFoundation.ARSession>();

            var xrOriginObj = new GameObject("XR Origin");
            xrOriginObj.transform.localPosition = Vector3.zero;
            var xrOrigin = xrOriginObj.AddComponent<Unity.XR.CoreUtils.XROrigin>();
            var arRaycast = xrOriginObj.AddComponent<UnityEngine.XR.ARFoundation.ARRaycastManager>();
            var arAnchor = xrOriginObj.AddComponent<UnityEngine.XR.ARFoundation.ARAnchorManager>();
            var arPlane = xrOriginObj.AddComponent<UnityEngine.XR.ARFoundation.ARPlaneManager>();

            var canvasObj = CreateCanvas("MainCanvas");

            var workerPanel = CreatePanel("WorkerPanel", canvasObj.transform);
            CreateText("WorkerHomeTitle", workerPanel.transform, new Vector2(0, 350), "Worker Home");
            CreateButton("StartTrainingBtn", workerPanel.transform, new Vector2(0, 200), "Start Training");
            CreateButton("ScanQRBtn", workerPanel.transform, new Vector2(0, 120), "Scan QR");
            CreateButton("CertificatesBtn", workerPanel.transform, new Vector2(0, 40), "Certificates");
            CreateButton("SettingsBtn", workerPanel.transform, new Vector2(0, -40), "Settings");

            var managerPanel = CreatePanel("ManagerPanel", canvasObj.transform);
            managerPanel.SetActive(false);
            CreateText("ManagerHomeTitle", managerPanel.transform, new Vector2(0, 350), "Manager Dashboard");
            CreateButton("WorkerRosterBtn", managerPanel.transform, new Vector2(0, 200), "Worker Roster");
            CreateButton("ComplianceBtn", managerPanel.transform, new Vector2(0, 120), "Compliance");
            CreateButton("VerifyCertBtn", managerPanel.transform, new Vector2(0, 40), "Verify Certificate");

            var adminPanel = CreatePanel("AdminPanel", canvasObj.transform);
            adminPanel.SetActive(false);
            CreateText("AdminHomeTitle", adminPanel.transform, new Vector2(0, 350), "Admin Dashboard");
            CreateButton("UserMgmtBtn", adminPanel.transform, new Vector2(0, 200), "User Management");
            CreateButton("SiteConfigBtn", adminPanel.transform, new Vector2(0, 120), "Site Config");
            CreateButton("AnalyticsBtn", adminPanel.transform, new Vector2(0, 40), "Analytics");

            var settingsPanel = CreatePanel("SettingsPanel", canvasObj.transform);
            settingsPanel.SetActive(false);
            CreateText("SettingsTitle", settingsPanel.transform, new Vector2(0, 350), "Settings");
            CreateButton("ThemeToggle", settingsPanel.transform, new Vector2(0, 200), "Toggle Theme");
            CreateButton("LanguageBtn", settingsPanel.transform, new Vector2(0, 120), "Language");
            CreateButton("VoiceToggle", settingsPanel.transform, new Vector2(0, 40), "Voice On/Off");

            var hudPanel = CreatePanel("HUDPanel", canvasObj.transform);
            hudPanel.SetActive(false);
            CreateText("HazardTypeText", hudPanel.transform, new Vector2(0, 350), "Hazard");
            CreateText("TimerText", hudPanel.transform, new Vector2(0, 300), "01:00");
            CreateText("ScoreText", hudPanel.transform, new Vector2(0, 260), "Score: 100");
            CreateButton("Action1Btn", hudPanel.transform, new Vector2(-150, -200), "Action 1");
            CreateButton("Action2Btn", hudPanel.transform, new Vector2(0, -200), "Action 2");
            CreateButton("Action3Btn", hudPanel.transform, new Vector2(150, -200), "Action 3");

            var assessmentPanel = CreatePanel("AssessmentPanel", canvasObj.transform);
            assessmentPanel.SetActive(false);
            CreateText("QuestionText", assessmentPanel.transform, new Vector2(0, 250), "Question");
            CreateButton("Option1Btn", assessmentPanel.transform, new Vector2(0, 150), "Option 1");
            CreateButton("Option2Btn", assessmentPanel.transform, new Vector2(0, 80), "Option 2");
            CreateButton("Option3Btn", assessmentPanel.transform, new Vector2(0, 10), "Option 3");
            CreateButton("Option4Btn", assessmentPanel.transform, new Vector2(0, -60), "Option 4");
            CreateButton("NextBtn", assessmentPanel.transform, new Vector2(0, -150), "Next");

            var certificatePanel = CreatePanel("CertificatePanel", canvasObj.transform);
            certificatePanel.SetActive(false);
            CreateText("CertTitle", certificatePanel.transform, new Vector2(0, 300), "Certificate");
            CreateText("CertIdText", certificatePanel.transform, new Vector2(0, 200), "CERT-XXXX");
            CreateText("CertScoreText", certificatePanel.transform, new Vector2(0, 150), "Score: XX");
            CreateButton("CertCloseBtn", certificatePanel.transform, new Vector2(0, -200), "Close");

            var qrPanel = CreatePanel("QRPanel", canvasObj.transform);
            qrPanel.SetActive(false);
            CreateText("QRStatusText", qrPanel.transform, new Vector2(0, 300), "Scan QR Code");

            SetupManagers();
            SetupARScripts();

            string scenePath = "Assets/Scenes/MainScene.unity";
            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
                AssetDatabase.CreateFolder("Assets", "Scenes");
            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"[SceneSetup] Created: {scenePath}");
        }

        static void SetupManagers()
        {
            var managers = new GameObject("Managers");
            managers.AddComponent<LanguageManager>();
            managers.AddComponent<VoiceModule>();
            managers.AddComponent<ThemeManager>();
            managers.AddComponent<LocalBlockchain>();
            managers.AddComponent<OfflineManager>();
            managers.AddComponent<EscalationManager>();
            managers.AddComponent<CitificationManager>();
            managers.AddComponent<UINavigator>();
            managers.AddComponent<HUDManager>();
            managers.AddComponent<EscalationEngine>();
            managers.AddComponent<AssessmentEngine>();
            managers.AddComponent<CertificateGenerator>();
            managers.AddComponent<QRCheckpointScanner>();
            managers.AddComponent<FireSimulation>();
            managers.AddComponent<GasLeakSimulation>();
        }

        static void SetupARScripts()
        {
            var arScripts = new GameObject("ARScripts");
            arScripts.AddComponent<ARSessionSetup>();
            arScripts.AddComponent<ARSiteMapper>();
            arScripts.AddComponent<ARScenarioPlayer>();
        }

        static GameObject CreateCanvas(string name)
        {
            var canvasObj = new GameObject(name);
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            return canvasObj;
        }

        static GameObject CreatePanel(string name, Transform parent)
        {
            var panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            var rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            var image = panel.AddComponent<UnityEngine.UI.Image>();
            image.color = new Color(0.1f, 0.1f, 0.12f, 0.95f);
            return panel;
        }

        static GameObject CreateButton(string name, Transform parent, Vector2 position, string text)
        {
            var btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);
            var rect = btnObj.AddComponent<RectTransform>();
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(250, 50);
            var image = btnObj.AddComponent<UnityEngine.UI.Image>();
            image.color = new Color(0.2f, 0.6f, 0.9f, 1f);
            btnObj.AddComponent<UnityEngine.UI.Button>();
            CreateText(name + "Text", btnObj.transform, Vector2.zero, text);
            return btnObj;
        }

        static GameObject CreateInputField(string name, Transform parent, Vector2 position)
        {
            var inputObj = new GameObject(name);
            inputObj.transform.SetParent(parent, false);
            var rect = inputObj.AddComponent<RectTransform>();
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(280, 50);
            var image = inputObj.AddComponent<UnityEngine.UI.Image>();
            image.color = new Color(0.2f, 0.2f, 0.25f, 1f);
            inputObj.AddComponent<TMPro.TMP_InputField>();
            CreateText(name + "Placeholder", inputObj.transform, Vector2.zero, name.Replace("Input", ""));
            return inputObj;
        }

        static GameObject CreateText(string name, Transform parent, Vector2 position, string text)
        {
            var textObj = new GameObject(name);
            textObj.transform.SetParent(parent, false);
            var rect = textObj.AddComponent<RectTransform>();
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(300, 40);
            var tmp = textObj.AddComponent<TMPro.TextMeshProUGUI>();
            tmp.text = text;
            tmp.alignment = TMPro.TextAlignmentOptions.Center;
            tmp.fontSize = 20;
            tmp.color = Color.white;
            return textObj;
        }
    }
}