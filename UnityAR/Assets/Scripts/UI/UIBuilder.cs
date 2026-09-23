using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIBuilder : MonoBehaviour
{
    public static UIBuilder Instance { get; private set; }

    [Header("Font Settings")]
    public TMP_FontAsset mainFont;
    public TMP_FontAsset iconFont;

    [Header("Colors - Dark Theme")]
    public Color darkBg = new Color(0.05f, 0.10f, 0.18f);
    public Color darkCard = new Color(0.10f, 0.18f, 0.28f);
    public Color darkInput = new Color(0.12f, 0.20f, 0.32f);
    public Color cyanAccent = new Color(0f, 0.75f, 0.85f);
    public Color whiteText = Color.white;
    public Color lightBlueText = new Color(0.7f, 0.85f, 1f);

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public Canvas CreateCanvas(string name, int sortOrder = 0)
    {
        var go = new GameObject(name);
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortOrder;

        var scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;

        go.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    public GameObject CreatePanel(Transform parent, string name, Color color, bool fullScreen = true)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);

        var rt = go.GetComponent<RectTransform>();
        if (fullScreen)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        var img = go.GetComponent<Image>();
        img.color = color;
        img.raycastTarget = true;

        return go;
    }

    public TextMeshProUGUI CreateText(Transform parent, string name, string text, int fontSize,
        Color color, TextAlignmentOptions alignment = TextAlignmentOptions.Center,
        bool isBold = false, TMP_FontAsset font = null)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);

        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = alignment;
        tmp.font = font ?? mainFont;
        tmp.fontStyle = isBold ? FontStyles.Bold : FontStyles.Normal;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        tmp.raycastTarget = false;

        return tmp;
    }

    public Button CreateButton(Transform parent, string name, string label, Color bgColor,
        Color textColor, Vector2 size, int fontSize = 18, bool rounded = true)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);

        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = size;

        var img = go.GetComponent<Image>();
        img.color = bgColor;

        if (rounded)
        {
            var outline = go.AddComponent<Outline>();
            outline.effectColor = new Color(0, 0, 0, 0.1f);
        }

        var btn = go.GetComponent<Button>();
        var colors = btn.colors;
        colors.highlightedColor = new Color(bgColor.r + 0.1f, bgColor.g + 0.1f, bgColor.b + 0.1f);
        colors.pressedColor = new Color(bgColor.r - 0.1f, bgColor.g - 0.1f, bgColor.b - 0.1f);
        btn.colors = colors;

        var textGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        textGo.transform.SetParent(go.transform, false);

        var textRt = textGo.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;

        var tmp = textGo.GetComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = fontSize;
        tmp.color = textColor;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.font = mainFont;
        tmp.fontStyle = FontStyles.Bold;

        return btn;
    }

    public TMP_InputField CreateInputField(Transform parent, string name, string placeholder,
        Color bgColor, Color textColor, Color placeholderColor, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(TMP_InputField));
        go.transform.SetParent(parent, false);

        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = size;

        var img = go.GetComponent<Image>();
        img.color = bgColor;

        var placeholderGo = new GameObject("Placeholder", typeof(RectTransform), typeof(TextMeshProUGUI));
        placeholderGo.transform.SetParent(go.transform, false);

        var phRt = placeholderGo.GetComponent<RectTransform>();
        phRt.anchorMin = Vector2.zero;
        phRt.anchorMax = Vector2.one;
        phRt.offsetMin = new Vector2(10, 0);
        phRt.offsetMax = new Vector2(-10, 0);

        var phTmp = placeholderGo.GetComponent<TextMeshProUGUI>();
        phTmp.text = placeholder;
        phTmp.fontSize = 14;
        phTmp.color = placeholderColor;
        phTmp.alignment = TextAlignmentOptions.Left;
        phTmp.font = mainFont;

        var textGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textGo.transform.SetParent(go.transform, false);

        var textRt = textGo.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = new Vector2(10, 0);
        textRt.offsetMax = new Vector2(-10, 0);

        var textTmp = textGo.GetComponent<TextMeshProUGUI>();
        textTmp.fontSize = 14;
        textTmp.color = textColor;
        textTmp.alignment = TextAlignmentOptions.Left;
        textTmp.font = mainFont;

        var input = go.GetComponent<TMP_InputField>();
        input.textViewport = textRt;
        input.textComponent = textTmp;
        input.placeholder = phTmp;

        return input;
    }

    public Image CreateCard(Transform parent, string name, Color color, Vector2 size,
        Vector2? position = null, float cornerRadius = 12f)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);

        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = size;
        if (position.HasValue)
        {
            rt.anchoredPosition = position.Value;
        }

        var img = go.GetComponent<Image>();
        img.color = color;

        return img;
    }

    public Image CreateIcon(Transform parent, string name, Sprite sprite, Color color, float size)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);

        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(size, size);

        var img = go.GetComponent<Image>();
        img.sprite = sprite;
        img.color = color;
        img.preserveAspect = true;

        return img;
    }

    public Slider CreateSlider(Transform parent, string name, Color fillColor, Color bgColor,
        Vector2 size, float initialValue = 0f)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Slider));
        go.transform.SetParent(parent, false);

        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = size;

        var bgGo = new GameObject("Background", typeof(RectTransform), typeof(Image));
        bgGo.transform.SetParent(go.transform, false);

        var bgRt = bgGo.GetComponent<RectTransform>();
        bgRt.anchorMin = new Vector2(0, 0.25f);
        bgRt.anchorMax = new Vector2(1, 0.75f);
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;

        var bgImg = bgGo.GetComponent<Image>();
        bgImg.color = bgColor;

        var fillGo = new GameObject("Fill Area", typeof(RectTransform));
        fillGo.transform.SetParent(go.transform, false);

        var fillRt = fillGo.GetComponent<RectTransform>();
        fillRt.anchorMin = new Vector2(0, 0.25f);
        fillRt.anchorMax = new Vector2(1, 0.75f);
        fillRt.offsetMin = Vector2.zero;
        fillRt.offsetMax = Vector2.zero;

        var fillBarGo = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fillBarGo.transform.SetParent(fillGo.transform, false);

        var fillBarRt = fillBarGo.GetComponent<RectTransform>();
        fillBarRt.anchorMin = Vector2.zero;
        fillBarRt.anchorMax = new Vector2(0, 1);
        fillBarRt.offsetMin = Vector2.zero;
        fillBarRt.offsetMax = Vector2.zero;

        var fillImg = fillBarGo.GetComponent<Image>();
        fillImg.color = fillColor;

        var handleGo = new GameObject("Handle", typeof(RectTransform), typeof(Image));
        handleGo.transform.SetParent(go.transform, false);

        var handleRt = handleGo.GetComponent<RectTransform>();
        handleRt.sizeDelta = new Vector2(20, 20);

        var handleImg = handleGo.GetComponent<Image>();
        handleImg.color = Color.white;

        var slider = go.GetComponent<Slider>();
        slider.fillRect = fillBarRt;
        slider.handleRect = handleRt;
        slider.targetGraphic = handleImg;
        slider.value = initialValue;

        return slider;
    }

    public ScrollRect CreateScrollRect(Transform parent, string name, Vector2 size, bool horizontal = false)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(ScrollRect));
        go.transform.SetParent(parent, false);

        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = size;

        var img = go.GetComponent<Image>();
        img.color = new Color(0, 0, 0, 0.01f);

        var contentGo = new GameObject("Content", typeof(RectTransform));
        contentGo.transform.SetParent(go.transform, false);

        var contentRt = contentGo.GetComponent<RectTransform>();
        contentRt.anchorMin = new Vector2(0, 1);
        contentRt.anchorMax = new Vector2(1, 1);
        contentRt.pivot = new Vector2(0.5f, 1);

        var scroll = go.GetComponent<ScrollRect>();
        scroll.content = contentRt;
        scroll.horizontal = horizontal;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Elastic;

        return scroll;
    }

    public Image CreateBottomNav(Transform parent, Color bgColor, float height = 70f)
    {
        var nav = CreatePanel(parent, "BottomNav", bgColor, false);
        var rt = nav.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(1, 0);
        rt.pivot = new Vector2(0.5f, 0);
        rt.sizeDelta = new Vector2(0, height);

        return nav.GetComponent<Image>();
    }

    public (Button home, Button training, Button cert, Button profile) CreateNavButtons(
        Transform navParent, Color activeColor, Color inactiveColor, TMP_FontAsset iconFont = null)
    {
        float btnWidth = Screen.width / 4f;

        var homeBtn = CreateNavButton(navParent, "HomeNav", "Home", 0, btnWidth, activeColor);
        var trainBtn = CreateNavButton(navParent, "TrainingNav", "Training", 1, btnWidth, inactiveColor);
        var certBtn = CreateNavButton(navParent, "CertNav", "Certificate", 2, btnWidth, inactiveColor);
        var profBtn = CreateNavButton(navParent, "ProfileNav", "Profile", 3, btnWidth, inactiveColor);

        return (homeBtn, trainBtn, certBtn, profBtn);
    }

    Button CreateNavButton(Transform parent, string name, string label, int index, float width, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Button));
        go.transform.SetParent(parent, false);

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2((float)index / 4f, 0);
        rt.anchorMax = new Vector2((float)(index + 1) / 4f, 1);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        var textGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        textGo.transform.SetParent(go.transform, false);

        var textRt = textGo.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;

        var tmp = textGo.GetComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 12;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.font = mainFont;

        var btn = go.GetComponent<Button>();
        btn.targetGraphic = tmp;

        return btn;
    }
}
