using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(RectTransform))]
public class UIThemeApplier : MonoBehaviour
{
    public enum ThemeElementType
    {
        Background,
        Surface,
        Card,
        InputField,
        PrimaryButton,
        SecondaryButton,
        TextPrimary,
        TextSecondary,
        Accent,
        Warning,
        Danger,
        Success,
        NavActive,
        NavInactive
    }

    [Header("Theme Settings")]
    public ThemeElementType elementType = ThemeElementType.Card;
    public bool applyOnStart = true;
    public bool applyOnThemeChange = true;

    [Header("Custom Colors (Override Theme)")]
    public bool useCustomColor = false;
    public Color customColor = Color.white;

    private Graphic _graphic;
    private TextMeshProUGUI _text;
    private Image _image;
    private Button _button;

    void Awake()
    {
        _graphic = GetComponent<Graphic>();
        _text = GetComponent<TextMeshProUGUI>();
        _image = GetComponent<Image>();
        _button = GetComponent<Button>();
    }

    void Start()
    {
        if (applyOnStart) ApplyTheme();

        if (applyOnThemeChange && ThemeManager.Instance != null)
        {
            ThemeManager.Instance.OnThemeChanged += OnThemeChanged;
        }
    }

    void OnDestroy()
    {
        if (ThemeManager.Instance != null)
        {
            ThemeManager.Instance.OnThemeChanged -= OnThemeChanged;
        }
    }

    void OnThemeChanged(string theme)
    {
        ApplyTheme();
    }

    public void ApplyTheme()
    {
        if (useCustomColor)
        {
            if (_graphic != null) _graphic.color = customColor;
            return;
        }

        if (ThemeManager.Instance == null) return;

        Color targetColor = GetColorForType(elementType);

        if (_text != null)
        {
            _text.color = targetColor;
        }
        else if (_image != null)
        {
            _image.color = targetColor;
        }
        else if (_graphic != null)
        {
            _graphic.color = targetColor;
        }

        if (_button != null)
        {
            var colors = _button.colors;
            colors.normalColor = targetColor;
            colors.highlightedColor = new Color(targetColor.r + 0.1f, targetColor.g + 0.1f, targetColor.b + 0.1f);
            colors.pressedColor = new Color(targetColor.r - 0.1f, targetColor.g - 0.1f, targetColor.b - 0.1f);
            colors.selectedColor = targetColor;
            _button.colors = colors;
        }
    }

    Color GetColorForType(ThemeElementType type)
    {
        var tm = ThemeManager.Instance;

        return type switch
        {
            ThemeElementType.Background => tm.CurrentBackground,
            ThemeElementType.Surface => tm.CurrentSurface,
            ThemeElementType.Card => tm.CurrentCard,
            ThemeElementType.InputField => tm.CurrentInputBg,
            ThemeElementType.PrimaryButton => tm.CurrentPrimary,
            ThemeElementType.SecondaryButton => tm.CurrentPrimaryDark,
            ThemeElementType.TextPrimary => tm.CurrentText,
            ThemeElementType.TextSecondary => tm.CurrentTextSecondary,
            ThemeElementType.Accent => tm.CurrentAccent,
            ThemeElementType.Warning => tm.CurrentWarning,
            ThemeElementType.Danger => tm.CurrentDanger,
            ThemeElementType.Success => tm.CurrentSuccess,
            ThemeElementType.NavActive => tm.CurrentPrimary,
            ThemeElementType.NavInactive => new Color(0.5f, 0.6f, 0.7f),
            _ => tm.CurrentPrimary
        };
    }
}
