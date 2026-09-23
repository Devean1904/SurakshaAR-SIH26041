using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ThemeManager : MonoBehaviour
{
    public static ThemeManager Instance { get; private set; }

    [Header("Dark Theme (Default)")]
    public Color darkBackground = new Color(0.05f, 0.10f, 0.18f);      // Deep navy blue
    public Color darkSurface = new Color(0.08f, 0.15f, 0.25f);         // Slightly lighter navy
    public Color darkCard = new Color(0.10f, 0.18f, 0.28f);            // Card background
    public Color darkText = Color.white;
    public Color darkTextSecondary = new Color(0.7f, 0.85f, 1f);       // Light blue text
    public Color darkPrimary = new Color(0f, 0.75f, 0.85f);            // Cyan/Teal accent
    public Color darkPrimaryDark = new Color(0f, 0.55f, 0.65f);        // Darker teal
    public Color darkAccent = new Color(0f, 0.85f, 0.75f);             // Green-cyan accent
    public Color darkWarning = new Color(1f, 0.6f, 0f);                // Orange warning
    public Color darkDanger = new Color(0.9f, 0.2f, 0.2f);             // Red danger
    public Color darkSuccess = new Color(0.2f, 0.8f, 0.4f);            // Green success
    public Color darkInputBg = new Color(0.12f, 0.20f, 0.32f);         // Input field background
    public Color darkGradientTop = new Color(0.05f, 0.12f, 0.22f);     // Gradient top
    public Color darkGradientBottom = new Color(0.03f, 0.08f, 0.15f);  // Gradient bottom

    [Header("Light Theme")]
    public Color lightBackground = new Color(0.93f, 0.95f, 0.97f);     // Light gray-blue
    public Color lightSurface = Color.white;
    public Color lightCard = Color.white;
    public Color lightText = new Color(0.1f, 0.15f, 0.25f);            // Dark navy text
    public Color lightTextSecondary = new Color(0.4f, 0.5f, 0.6f);     // Gray text
    public Color lightPrimary = new Color(0f, 0.65f, 0.75f);           // Teal accent
    public Color lightPrimaryDark = new Color(0f, 0.50f, 0.60f);
    public Color lightAccent = new Color(0f, 0.70f, 0.65f);
    public Color lightWarning = new Color(0.9f, 0.55f, 0f);
    public Color lightDanger = new Color(0.85f, 0.15f, 0.15f);
    public Color lightSuccess = new Color(0.15f, 0.70f, 0.35f);
    public Color lightInputBg = new Color(0.90f, 0.92f, 0.95f);
    public Color lightGradientTop = new Color(0.95f, 0.97f, 1f);
    public Color lightGradientBottom = new Color(0.88f, 0.92f, 0.96f);

    [Header("Current Theme Colors")]
    public Color CurrentBackground { get; private set; }
    public Color CurrentSurface { get; private set; }
    public Color CurrentCard { get; private set; }
    public Color CurrentText { get; private set; }
    public Color CurrentTextSecondary { get; private set; }
    public Color CurrentPrimary { get; private set; }
    public Color CurrentPrimaryDark { get; private set; }
    public Color CurrentAccent { get; private set; }
    public Color CurrentWarning { get; private set; }
    public Color CurrentDanger { get; private set; }
    public Color CurrentSuccess { get; private set; }
    public Color CurrentInputBg { get; private set; }
    public Color CurrentGradientTop { get; private set; }
    public Color CurrentGradientBottom { get; private set; }

    public string CurrentTheme { get; private set; } = "Dark";

    public System.Action<string> OnThemeChanged;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        string saved = PlayerPrefs.GetString("Theme", "Dark");
        ApplyTheme(saved);
    }

    public void ApplyTheme(string theme)
    {
        CurrentTheme = theme;
        PlayerPrefs.SetString("Theme", theme);
        PlayerPrefs.Save();

        if (theme == "Light")
        {
            CurrentBackground = lightBackground;
            CurrentSurface = lightSurface;
            CurrentCard = lightCard;
            CurrentText = lightText;
            CurrentTextSecondary = lightTextSecondary;
            CurrentPrimary = lightPrimary;
            CurrentPrimaryDark = lightPrimaryDark;
            CurrentAccent = lightAccent;
            CurrentWarning = lightWarning;
            CurrentDanger = lightDanger;
            CurrentSuccess = lightSuccess;
            CurrentInputBg = lightInputBg;
            CurrentGradientTop = lightGradientTop;
            CurrentGradientBottom = lightGradientBottom;
        }
        else
        {
            CurrentBackground = darkBackground;
            CurrentSurface = darkSurface;
            CurrentCard = darkCard;
            CurrentText = darkText;
            CurrentTextSecondary = darkTextSecondary;
            CurrentPrimary = darkPrimary;
            CurrentPrimaryDark = darkPrimaryDark;
            CurrentAccent = darkAccent;
            CurrentWarning = darkWarning;
            CurrentDanger = darkDanger;
            CurrentSuccess = darkSuccess;
            CurrentInputBg = darkInputBg;
            CurrentGradientTop = darkGradientTop;
            CurrentGradientBottom = darkGradientBottom;
        }

        ApplyToAllUI();
        OnThemeChanged?.Invoke(theme);
    }

    public void ToggleTheme()
    {
        ApplyTheme(CurrentTheme == "Dark" ? "Light" : "Dark");
    }

    void ApplyToAllUI()
    {
        foreach (var canvas in FindObjectsByType<Canvas>(FindObjectsInactive.Exclude))
        {
            ApplyToCanvas(canvas);
        }
    }

    public void ApplyToCanvas(Canvas canvas)
    {
        if (canvas == null) return;

        foreach (var graphic in canvas.GetComponentsInChildren<Graphic>(true))
        {
            ApplyThemeToGraphic(graphic);
        }

        foreach (var text in canvas.GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            ApplyThemeToText(text);
        }
    }

    public void ApplyThemeToGraphic(Graphic graphic)
    {
        if (graphic == null) return;

        var go = graphic.gameObject;

        if (go.CompareTag("ThemeBackground"))
            graphic.color = CurrentBackground;
        else if (go.CompareTag("ThemeSurface"))
            graphic.color = CurrentSurface;
        else if (go.CompareTag("ThemeCard"))
            graphic.color = CurrentCard;
        else if (go.CompareTag("ThemePrimary"))
            graphic.color = CurrentPrimary;
        else if (go.CompareTag("ThemePrimaryDark"))
            graphic.color = CurrentPrimaryDark;
        else if (go.CompareTag("ThemeAccent"))
            graphic.color = CurrentAccent;
        else if (go.CompareTag("ThemeWarning"))
            graphic.color = CurrentWarning;
        else if (go.CompareTag("ThemeDanger"))
            graphic.color = CurrentDanger;
        else if (go.CompareTag("ThemeSuccess"))
            graphic.color = CurrentSuccess;
        else if (go.CompareTag("ThemeInputBg"))
            graphic.color = CurrentInputBg;
    }

    public void ApplyThemeToText(TextMeshProUGUI text)
    {
        if (text == null) return;

        var go = text.gameObject;

        if (go.CompareTag("ThemeText"))
            text.color = CurrentText;
        else if (go.CompareTag("ThemeTextSecondary"))
            text.color = CurrentTextSecondary;
        else if (go.CompareTag("ThemePrimary"))
            text.color = CurrentPrimary;
        else if (go.CompareTag("ThemeAccent"))
            text.color = CurrentAccent;
    }

    public Color GetColor(string colorName)
    {
        return colorName switch
        {
            "Background" => CurrentBackground,
            "Surface" => CurrentSurface,
            "Card" => CurrentCard,
            "Text" => CurrentText,
            "TextSecondary" => CurrentTextSecondary,
            "Primary" => CurrentPrimary,
            "PrimaryDark" => CurrentPrimaryDark,
            "Accent" => CurrentAccent,
            "Warning" => CurrentWarning,
            "Danger" => CurrentDanger,
            "Success" => CurrentSuccess,
            "InputBg" => CurrentInputBg,
            _ => CurrentPrimary
        };
    }
}
