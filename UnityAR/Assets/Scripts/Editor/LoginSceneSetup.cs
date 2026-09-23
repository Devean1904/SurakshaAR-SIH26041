#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class LoginSceneSetup
{
    public static void Build()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var canvas = UI.Canvas("LoginCanvas");
        UI.EventSys();
        var root = canvas.transform;

        // Full background
        var bg = UI.P(root, "Background", UI.BgDark);

        // Center card
        var card = UI.PA(bg.transform, "LoginCard", UI.CardBg, 0.25f, 0.18f, 0.75f, 0.82f);

        // Shield icon area (colored circle placeholder)
        var shield = UI.PA(card.transform, "ShieldIcon", UI.Primary, 0.38f, 0.82f, 0.62f, 0.94f);

        // App Title
        var title = UI.T(card.transform, "Title", "SurakshaAR", 36, UI.White, 0.05f, 0.72f, 0.95f, 0.82f);
        title.GetComponent<TMPro.TextMeshProUGUI>().fontStyle = TMPro.FontStyles.Bold;

        // Subtitle
        var sub = UI.T(card.transform, "Subtitle", "Mining Safety Training Platform", 14, UI.TextSec, 0.05f, 0.65f, 0.95f, 0.72f);

        // Language selector button
        var langBtn = UI.B(card.transform, "LanguageButton", "English", UI.Surface, 0.35f, 0.60f, 0.65f, 0.65f, 13f);

        // User ID input
        var userIdInput = UI.I(card.transform, "UserIdInput", "Enter Worker / Admin ID", 0.08f, 0.46f, 0.92f, 0.54f);

        // Password input
        var passInput = UI.I(card.transform, "PasswordInput", "Enter Password", 0.08f, 0.36f, 0.92f, 0.44f);

        // Remember me toggle
        var remember = UI.Tog(card.transform, "RememberToggle", "Remember ID", 0.08f, 0.30f, 0.45f, 0.34f);

        // Login button
        var loginBtn = UI.B(card.transform, "LoginButton", "SIGN IN", UI.Primary, 0.08f, 0.18f, 0.92f, 0.27f, 20f);

        // Status text
        var status = UI.T(card.transform, "StatusText", "", 13, UI.Danger, 0.08f, 0.14f, 0.92f, 0.18f);

        // Divider
        var divider = UI.T(card.transform, "Divider", "- OR -", 12, UI.TextMuted, 0.15f, 0.11f, 0.85f, 0.14f);

        // OTP Login button
        var otpBtn = UI.B(card.transform, "OtpLoginButton", "Login with OTP", UI.Surface, 0.08f, 0.03f, 0.92f, 0.10f, 15f);

        // ═══ OTP PANEL (hidden) ═══
        var otpPanel = UI.PA(bg.transform, "OtpPanel", UI.BgDark, 0.25f, 0.18f, 0.75f, 0.82f);
        otpPanel.AddComponent<Image>().color = UI.CardBg;
        otpPanel.SetActive(false);

        UI.T(otpPanel.transform, "OtpTitle", "OTP Verification", 28, UI.White, 0.05f, 0.82f, 0.95f, 0.92f)
            .GetComponent<TMPro.TextMeshProUGUI>().fontStyle = TMPro.FontStyles.Bold;
        UI.T(otpPanel.transform, "OtpSubtitle", "Enter your registered phone number", 14, UI.TextSec, 0.05f, 0.74f, 0.95f, 0.82f);

        var phoneInput = UI.I(otpPanel.transform, "PhoneInput", "+91 XXXXX XXXXX", 0.08f, 0.62f, 0.92f, 0.70f);
        var sendOtpBtn = UI.B(otpPanel.transform, "SendOtpButton", "SEND OTP", UI.Primary, 0.08f, 0.52f, 0.92f, 0.59f, 17f);
        var otpInput = UI.I(otpPanel.transform, "OtpInput", "Enter 6-digit OTP", 0.08f, 0.40f, 0.92f, 0.48f);
        var verifyBtn = UI.B(otpPanel.transform, "VerifyOtpButton", "VERIFY & SIGN IN", UI.Success, 0.08f, 0.30f, 0.92f, 0.38f, 17f);
        var backBtn = UI.B(otpPanel.transform, "BackButton", "Back to Login", UI.Surface, 0.08f, 0.20f, 0.92f, 0.27f, 15f);

        // ═══ LANGUAGE PANEL (hidden) ═══
        var langPanel = UI.PA(bg.transform, "LanguagePanel", UI.BgDark, 0.2f, 0.2f, 0.8f, 0.8f);
        langPanel.AddComponent<Image>().color = UI.CardBg;
        langPanel.SetActive(false);

        UI.T(langPanel.transform, "LangTitle", "Select Language", 24, UI.White, 0.05f, 0.88f, 0.95f, 0.96f)
            .GetComponent<TMPro.TextMeshProUGUI>().fontStyle = TMPro.FontStyles.Bold;

        string[] langs = { "English", "Hindi", "Santali", "Marathi", "Tamil", "Telugu", "Kannada" };
        string[] codes = { "en", "hi", "sat", "mr", "ta", "te", "kn" };
        for (int i = 0; i < langs.Length; i++)
        {
            float yPos = 0.78f - i * 0.09f;
            var langOption = UI.B(langPanel.transform, "Lang_" + codes[i], langs[i], UI.Surface, 0.08f, yPos - 0.035f, 0.92f, yPos + 0.035f, 16f);
        }

        // Add LoginManager component
        new GameObject("LoginManager").AddComponent<LoginManager>();

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/LoginScene.unity");
        Debug.Log("[UISetup] LoginScene built - premium UI");
    }
}
#endif
