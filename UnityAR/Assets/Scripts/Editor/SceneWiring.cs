using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using System;

public class SceneWiring
{
    [MenuItem("SurakshaAR/Wire All Scenes")]
    public static void WireAll()
    {
        WireLogin();
        WireMain();
        WireAR();
        AssetDatabase.SaveAssets();
        Debug.Log("[SceneWiring] All scenes wired.");
    }

    [MenuItem("SurakshaAR/Wire LoginScene")]
    public static void WireLogin()
    {
        var go = GO("LoginManager");
        if (go == null) { Debug.LogWarning("[SceneWiring] LoginManager not found."); return; }
        var so = new SerializedObject(go);

        Ref(so, "loginPanel", "LoginCard");
        Ref(so, "otpPanel", "OtpPanel");
        Ref(so, "userIdInput", "UserIdInput");
        Ref(so, "passwordInput", "PasswordInput");
        Ref(so, "phoneInput", "PhoneInput");
        Ref(so, "otpInput", "OtpInput");
        Ref(so, "loginButton", "LoginButton");
        Ref(so, "sendOtpButton", "SendOtpButton");
        Ref(so, "verifyOtpButton", "VerifyOtpButton");
        Ref(so, "statusText", "StatusText");
        Ref(so, "rememberToggle", "RememberToggle");

        Click(go, "BackButton", "OnBackToLogin");
        Click(go, "LanguageButton", "OnToggleLanguage");

        so.ApplyModifiedProperties();
        Debug.Log("[SceneWiring] LoginScene wired.");
    }

    [MenuItem("SurakshaAR/Wire MainScene")]
    public static void WireMain()
    {
        var canvas = GO("MainCanvas");
        if (canvas == null) { Debug.LogWarning("[SceneWiring] MainCanvas not found."); return; }

        var dm = canvas.GetComponent<DashboardManager>();
        if (dm != null)
        {
            var so = new SerializedObject(dm);
            Ref(so, "_dashboardPanel", "WorkerPanel");
            if (!Ref(so, "_logoutButton", "LogoutButton"))
                if (!Ref(so, "_logoutButton", "ProfileBtn"))
                    Ref(so, "_logoutButton", "ProfileBtnNav");
            Ref(so, "_welcomeText", "WelcomeText");
            Ref(so, "_statusText", "StatusText");
            Ref(so, "_domainListParent", "DomainList");
            RefPrefab(so, "_domainButtonPrefab", "Assets/Prefabs/UI/DomainButton.prefab");
            so.ApplyModifiedProperties();
        }

        var ui = canvas.GetComponent<UINavigator>();
        if (ui != null)
        {
            var so2 = new SerializedObject(ui);
            Ref(so2, "homePanel", "WorkerPanel");
            Ref(so2, "settingsPanel", "SettingsPanel");
            Ref(so2, "adminPanel", "AdminPanel");
            Ref(so2, "managerPanel", "ManagerPanel");
            Ref(so2, "workerPanel", "WorkerPanel");

            if (!Ref(so2, "settingsButton", "SettingsButton"))
                if (!Ref(so2, "settingsButton", "SettingsBtn"))
                    Ref(so2, "settingsButton", "LanguageBtn");

            if (!Ref(so2, "logoutButton", "LogoutButton"))
                if (!Ref(so2, "logoutButton", "ProfileBtn"))
                    Ref(so2, "logoutButton", "ProfileBtnNav");

            so2.ApplyModifiedProperties();
        }

        Debug.Log("[SceneWiring] MainScene wired.");
    }

    [MenuItem("SurakshaAR/Wire ARScene")]
    public static void WireAR()
    {
        WireHUD();
        WireAssessment();
        WireEscalation();
        AssetDatabase.SaveAssets();
        Debug.Log("[SceneWiring] ARScene wired.");
    }

    static void WireHUD()
    {
        var training = GO("Training");
        if (training == null) { Debug.LogWarning("[SceneWiring] Training not found."); return; }
        var hm = training.GetComponent<HUDManager>();
        if (hm == null) { Debug.LogWarning("[SceneWiring] HUDManager component not found."); return; }
        var so = new SerializedObject(hm);

        Ref(so, "hudPanel", "HUDPanel");
        Ref(so, "alertPanel", "AlertPanel");
        Ref(so, "criticalFailPanel", "CriticalFailPanel");
        Ref(so, "evacuationPanel", "EvacuationPanel");
        Ref(so, "scorePanel", "ScoreResultsPanel");
        Ref(so, "timerText", "TimerText");
        Ref(so, "currentScoreText", "ScoreText");
        Ref(so, "penaltyText", "PenaltyText");
        Ref(so, "warningInstructionsText", "WarningInstructionsText");
        Ref(so, "alertText", "AlertText");
        Ref(so, "actionButtonContainer", "ActionButtonContainer");

        Ref(so, "actionButton1", "ActionButton1");
        Ref(so, "actionButton2", "ActionButton2");
        Ref(so, "actionButton3", "ActionButton3");

        var ab1 = GO("ActionButton1");
        var ab2 = GO("ActionButton2");
        var ab3 = GO("ActionButton3");
        if (ab1 != null) { var t = RefTmpChild(ab1.transform, "Label"); if (t != null) so.FindProperty("actionText1").objectReferenceValue = t; }
        if (ab2 != null) { var t = RefTmpChild(ab2.transform, "Label"); if (t != null) so.FindProperty("actionText2").objectReferenceValue = t; }
        if (ab3 != null) { var t = RefTmpChild(ab3.transform, "Label"); if (t != null) so.FindProperty("actionText3").objectReferenceValue = t; }

        Ref(so, "criticalFailTitle", "CriticalFailTitle");
        Ref(so, "criticalFailMessage", "CriticalFailMessage");
        Ref(so, "criticalFailScore", "CriticalFailScore");
        Ref(so, "retryButton", "RetryButton");
        Ref(so, "viewResultsButton", "ViewResultsButton");

        Ref(so, "evacuationText", "EvacuationText");
        Ref(so, "timerProgressBar", "TimerBarFill");

        so.ApplyModifiedProperties();
    }

    static void WireAssessment()
    {
        var training = GO("Training");
        if (training == null) { Debug.LogWarning("[SceneWiring] Training not found."); return; }
        var ae = training.GetComponent<AssessmentEngine>();
        if (ae == null) { Debug.LogWarning("[SceneWiring] AssessmentEngine component not found."); return; }
        var so = new SerializedObject(ae);

        Ref(so, "assessmentPanel", "AssessmentPanel");
        Ref(so, "questionPanel", "QuestionPanel");
        Ref(so, "resultPanel", "ScoreResultsPanel");
        Ref(so, "questionNumberText", "QuestionNumberText");
        Ref(so, "questionText", "QuestionText");
        Ref(so, "progressBar", "QProgressFill");

        so.FindProperty("optionButtons").arraySize = 4;
        so.FindProperty("optionTexts").arraySize = 4;

        for (int i = 0; i < 4; i++)
        {
            string objName = "Option" + (i + 1);
            var btnGo = GO(objName);
            if (btnGo != null)
            {
                so.FindProperty("optionButtons").GetArrayElementAtIndex(i).objectReferenceValue = btnGo.GetComponent<Button>();
                var txt = RefTmpChild(btnGo.transform, "Label");
                if (txt != null)
                    so.FindProperty("optionTexts").GetArrayElementAtIndex(i).objectReferenceValue = txt;
            }
        }

        Ref(so, "nextButton", "NextButton");
        Ref(so, "resultTitleText", "ResultTitle");
        Ref(so, "actionScoreText", "ActionScoreText");
        Ref(so, "questionScoreText", "QuestionScoreText");
        Ref(so, "totalScoreText", "TotalScoreText");
        Ref(so, "resultMessageText", "ResultMessageText");
        Ref(so, "viewCertificateButton", "ViewCertificateButton");
        Ref(so, "retryButton", "RetryButton2");
        Ref(so, "homeButton", "HomeButton");

        so.ApplyModifiedProperties();
    }

    static void WireEscalation()
    {
        var training = GO("Training");
        if (training == null) { Debug.LogWarning("[SceneWiring] Training not found."); return; }
        var ee = training.GetComponent<EscalationEngine>();
        if (ee == null) { Debug.LogWarning("[SceneWiring] EscalationEngine component not found."); return; }
        var so = new SerializedObject(ee);
        so.ApplyModifiedProperties();
    }

    static GameObject GO(string name)
    {
        var found = GameObject.Find(name);
        if (found != null) return found;

        foreach (var root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
        {
            var result = Deep(root.transform, name);
            if (result != null) return result.gameObject;
        }
        return null;
    }

    static Transform Deep(Transform t, string name)
    {
        if (t.name == name) return t;
        for (int i = 0; i < t.childCount; i++)
        {
            var r = Deep(t.GetChild(i), name);
            if (r != null) return r;
        }
        return null;
    }

    static bool Ref(SerializedObject so, string prop, string childName)
    {
        var p = so.FindProperty(prop);
        if (p == null) return false;
        var go = GO(childName);
        if (go == null) return false;

        if (p.propertyType == SerializedPropertyType.ObjectReference)
        {
            var fieldType = p.objectReferenceValue != null
                ? p.objectReferenceValue.GetType()
                : System.Type.GetType(p.type.Replace("PPtr<$", "").Replace(">", "").Replace("UnityEngine.", ""));

            if (fieldType == null || fieldType == typeof(GameObject) || fieldType == typeof(UnityEngine.Object))
            {
                p.objectReferenceValue = go;
            }
            else
            {
                var comp = go.GetComponent(fieldType);
                if (comp != null) p.objectReferenceValue = comp;
            }
        }
        return true;
    }

    static bool RefBtn(SerializedObject so, string prop, string childName)
    {
        var p = so.FindProperty(prop);
        if (p == null) return false;
        var go = GO(childName);
        if (go == null) return false;
        p.objectReferenceValue = go.GetComponent<Button>();
        return true;
    }

    static bool RefTMP(SerializedObject so, string prop, string childName)
    {
        var p = so.FindProperty(prop);
        if (p == null) return false;
        var go = GO(childName);
        if (go == null) return false;
        p.objectReferenceValue = go.GetComponent<TMP_Text>();
        return true;
    }

    static bool RefImg(SerializedObject so, string prop, string childName)
    {
        var p = so.FindProperty(prop);
        if (p == null) return false;
        var go = GO(childName);
        if (go == null) return false;
        p.objectReferenceValue = go.GetComponent<Image>();
        return true;
    }

    static bool RefToggle(SerializedObject so, string prop, string childName)
    {
        var p = so.FindProperty(prop);
        if (p == null) return false;
        var go = GO(childName);
        if (go == null) return false;
        p.objectReferenceValue = go.GetComponent<Toggle>();
        return true;
    }

    static bool RefPrefab(SerializedObject so, string prop, string assetPath)
    {
        var p = so.FindProperty(prop);
        if (p == null) return false;
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        if (prefab == null) return false;
        p.objectReferenceValue = prefab;
        return true;
    }

    static bool RefTr(SerializedObject so, string prop, string childName)
    {
        var p = so.FindProperty(prop);
        if (p == null) return false;
        var go = GO(childName);
        if (go == null) return false;
        p.objectReferenceValue = go.transform;
        return true;
    }

    static TMP_Text RefTmpChild(Transform parent, string childName)
    {
        var t = Deep(parent, childName);
        if (t == null) return null;
        return t.GetComponent<TMP_Text>();
    }

    static bool RefAlt(SerializedObject so, string prop, string childA, string childB)
    {
        if (Ref(so, prop, childA)) return true;
        return Ref(so, prop, childB);
    }

    static void Click(GameObject root, string buttonName, string methodName)
    {
        var btnGo = GO(buttonName);
        if (btnGo == null) return;
        var btn = btnGo.GetComponent<Button>();
        if (btn == null) return;

        if (btn.onClick == null)
            btn.onClick = new Button.ButtonClickedEvent();

        UnityEditor.Events.UnityEventTools.AddPersistentListener(
            btn.onClick,
            new UnityEngine.Events.UnityAction(() =>
            {
                root.SendMessage(methodName, SendMessageOptions.DontRequireReceiver);
            })
        );
    }
}
