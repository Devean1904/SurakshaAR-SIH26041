using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuildUIFromCode : MonoBehaviour
{
    [Header("Settings")]
    public bool buildOnStart = true;
    public TMP_FontAsset font;

    [Header("Colors")]
    public Color darkBg = new Color(0.05f, 0.10f, 0.18f);
    public Color cardBg = new Color(0.10f, 0.18f, 0.28f);
    public Color inputBg = new Color(0.12f, 0.20f, 0.32f);
    public Color cyanAccent = new Color(0f, 0.75f, 0.85f);
    public Color whiteText = Color.white;
    public Color lightBlue = new Color(0.7f, 0.85f, 1f);
    public Color placeholderColor = new Color(0.5f, 0.6f, 0.7f);
    public Color greenAccent = new Color(0f, 0.85f, 0.75f);
    public Color navInactive = new Color(0.5f, 0.6f, 0.7f);
    public Color orangeAccent = new Color(1f, 0.6f, 0f);
    public Color purpleAccent = new Color(0.5f, 0.3f, 0.8f);

    private Canvas _canvas;

    void Start()
    {
        if (buildOnStart) BuildLoginScreen();
    }

    public void ClearCanvas()
    {
        if (_canvas != null) Destroy(_canvas.gameObject);
    }

    public void BuildLoginScreen()
    {
        ClearCanvas();
        _canvas = CreateCanvas("LoginCanvas");
        GameObject bg = CreatePanel(_canvas.transform, "Background", darkBg);
        StretchFull(bg);
        CreateLoginUI(bg.transform);
    }

    public void BuildWorkerLanding()
    {
        ClearCanvas();
        _canvas = CreateCanvas("WorkerCanvas");
        GameObject bg = CreatePanel(_canvas.transform, "Background", darkBg);
        StretchFull(bg);
        CreateWorkerLandingUI(bg.transform);
    }

    public void BuildManagerLanding()
    {
        ClearCanvas();
        _canvas = CreateCanvas("ManagerCanvas");
        GameObject bg = CreatePanel(_canvas.transform, "Background", darkBg);
        StretchFull(bg);
        CreateManagerLandingUI(bg.transform);
    }

    public void BuildAdminLanding()
    {
        ClearCanvas();
        _canvas = CreateCanvas("AdminCanvas");
        GameObject bg = CreatePanel(_canvas.transform, "Background", darkBg);
        StretchFull(bg);
        CreateAdminLandingUI(bg.transform);
    }

    void CreateLoginUI(Transform parent)
    {
        GameObject iconBg = CreateCircle(parent, "WorkerIconBg", cyanAccent, 80f);
        SetPos(iconBg, 0, 350f);

        TextMeshProUGUI iconText = CreateText(parent, "Icon", "W", 40, whiteText);
        SetPos(iconText.gameObject, 0, 350f);
        SetSize(iconText.gameObject, 80, 80);

        TextMeshProUGUI title = CreateText(parent, "Title", "AR Safety Training", 36, whiteText, TextAlignmentOptions.Center, true);
        SetPos(title.gameObject, 0, 250f);
        SetSize(title.gameObject, 600, 50);

        TextMeshProUGUI subtitle = CreateText(parent, "Subtitle", "Real Hazards. Real Decisions. Safer Tomorrow.", 16, lightBlue);
        SetPos(subtitle.gameObject, 0, 200f);
        SetSize(subtitle.gameObject, 600, 40);

        TMP_InputField idInput = CreateInputField(parent, "IdInput", "Employee ID / Phone");
        SetPos(idInput.gameObject, 0, 100f);
        SetSize(idInput.gameObject, 500, 55);

        TMP_InputField passInput = CreateInputField(parent, "PassInput", "Password / PIN");
        SetPos(passInput.gameObject, 0, 30f);
        SetSize(passInput.gameObject, 500, 55);

        Button loginBtn = CreateButton(parent, "LoginBtn", "LOGIN", cyanAccent, whiteText, new Vector2(500, 55));
        SetPos(loginBtn.gameObject, 0, -50f);

        TextMeshProUGUI forgot = CreateText(parent, "Forgot", "Forgot Password?", 14, cyanAccent);
        SetPos(forgot.gameObject, 0, -100f);
        SetSize(forgot.gameObject, 300, 30);

        TextMeshProUGUI tagline = CreateText(parent, "Tagline", "Safe Workers \u2022 Safe Industry \u2022 Stronger India", 12, lightBlue);
        SetPos(tagline.gameObject, 0, -350f);
        SetSize(tagline.gameObject, 600, 30);
    }

    void CreateWorkerLandingUI(Transform parent)
    {
        GameObject header = CreatePanel(parent, "Header", cardBg);
        SetPos(header, 0, 700f);
        SetSize(header, 1000, 250);

        TextMeshProUGUI welcome = CreateText(header.transform, "Welcome", "Welcome,", 18, lightBlue, TextAlignmentOptions.Left);
        TextMeshProUGUI name = CreateText(header.transform, "Name", "Ramesh", 28, whiteText, TextAlignmentOptions.Left, true);
        SetPos(name.gameObject, -150, 30f);

        CreateBadge(header.transform, "Worker", 200, 50f, cyanAccent);

        GameObject card = CreatePanel(parent, "TrainingCard", cardBg);
        SetPos(card, 0, 350f);
        SetSize(card, 500, 280);

        CreateText(card.transform, "TTitle", "Today's Training", 18, lightBlue, TextAlignmentOptions.Left, true);
        CreateText(card.transform, "Module", "Fire & Explosion", 22, whiteText, TextAlignmentOptions.Left, true);
        TextMeshProUGUI prog = CreateText(card.transform, "Progress", "Module 1 of 5", 14, lightBlue, TextAlignmentOptions.Left);
        SetPos(prog.gameObject, 0, -20f);

        Slider slider = CreateSlider(card.transform, "Bar", cyanAccent, inputBg, new Vector2(420, 15));
        SetPos(slider.gameObject, 0, -50f);

        Button btn = CreateButton(card.transform, "Continue", "Continue", cyanAccent, whiteText, new Vector2(420, 50));
        SetPos(btn.gameObject, 0, -100f);

        CreateBottomNav(parent);
    }

    void CreateManagerLandingUI(Transform parent)
    {
        GameObject header = CreatePanel(parent, "Header", cardBg);
        SetPos(header, 0, 700f);
        SetSize(header, 1000, 250);

        CreateText(header.transform, "Welcome", "Welcome,", 18, lightBlue, TextAlignmentOptions.Left);
        TextMeshProUGUI name = CreateText(header.transform, "Name", "S. Patil", 28, whiteText, TextAlignmentOptions.Left, true);
        SetPos(name.gameObject, -150, 30f);

        CreateBadge(header.transform, "Manager", 200, 50f, cyanAccent);
        CreateText(header.transform, "Site", "| Site Supervisor", 16, lightBlue, TextAlignmentOptions.Left);

        string[] labels = { "View Worker Roster", "Site Compliance", "Certificates Verification", "Analytics" };
        Color[] colors = { cyanAccent, greenAccent, orangeAccent, purpleAccent };
        CreateDashGrid(parent, labels, colors);
        CreateBottomNav(parent);
    }

    void CreateAdminLandingUI(Transform parent)
    {
        GameObject header = CreatePanel(parent, "Header", cardBg);
        SetPos(header, 0, 700f);
        SetSize(header, 1000, 250);

        CreateText(header.transform, "Welcome", "Welcome,", 18, lightBlue, TextAlignmentOptions.Left);
        TextMeshProUGUI name = CreateText(header.transform, "Name", "Admin", 28, whiteText, TextAlignmentOptions.Left, true);
        SetPos(name.gameObject, -150, 30f);

        CreateBadge(header.transform, "Admin", 200, 50f, cyanAccent);
        CreateText(header.transform, "Site", "| System Administrator", 16, lightBlue, TextAlignmentOptions.Left);

        string[] labels = { "Manage Training Content", "User Management", "Blockchain Keys", "System Analytics" };
        Color[] colors = { cyanAccent, greenAccent, orangeAccent, purpleAccent };
        CreateDashGrid(parent, labels, colors);
        CreateBottomNav(parent);
    }

    void CreateDashGrid(Transform parent, string[] labels, Color[] iconColors)
    {
        for (int i = 0; i < 4; i++)
        {
            float x = (i % 2 == 0) ? -130f : 130f;
            float y = (i < 2) ? 250f : 80f;

            GameObject card = CreatePanel(parent, "Card" + i, cardBg);
            SetPos(card, x, y);
            SetSize(card, 240, 150);

            GameObject icon = CreateCircle(card.transform, "Icon", iconColors[i], 40f);
            SetPos(icon, 0, 20f);

            TextMeshProUGUI label = CreateText(card.transform, "Label", labels[i], 14, whiteText, TextAlignmentOptions.Center, false);
            SetPos(label.gameObject, 0, -30f);
            SetSize(label.gameObject, 220, 40);
        }
    }

    void CreateBottomNav(Transform parent)
    {
        GameObject nav = CreatePanel(parent, "BottomNav", cardBg);
        RectTransform rt = nav.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(1, 0);
        rt.pivot = new Vector2(0.5f, 0);
        rt.sizeDelta = new Vector2(0, 70);

        string[] tabs = { "Home", "Training", "Certificate", "Profile" };
        for (int i = 0; i < 4; i++)
        {
            TextMeshProUGUI tab = CreateText(nav.transform, "Tab" + i, tabs[i], 12, i == 0 ? cyanAccent : navInactive);
            RectTransform tabRt = tab.GetComponent<RectTransform>();
            tabRt.anchorMin = new Vector2((float)i / 4f, 0);
            tabRt.anchorMax = new Vector2((float)(i + 1) / 4f, 1);
            tabRt.offsetMin = Vector2.zero;
            tabRt.offsetMax = Vector2.zero;
        }
    }

    Canvas CreateCanvas(string name)
    {
        GameObject go = new GameObject(name);
        Canvas canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;
        go.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    GameObject CreatePanel(Transform parent, string name, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        go.GetComponent<Image>().color = color;
        return go;
    }

    GameObject CreateCircle(Transform parent, string name, Color color, float size)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(size, size);
        go.GetComponent<Image>().color = color;
        return go;
    }

    TextMeshProUGUI CreateText(Transform parent, string name, string text, int fontSize,
        Color color, TextAlignmentOptions align = TextAlignmentOptions.Center, bool bold = false)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = align;
        tmp.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
        tmp.raycastTarget = false;
        if (font != null) tmp.font = font;
        return tmp;
    }

    Button CreateButton(Transform parent, string name, string label, Color bg, Color textColor, Vector2 size)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        go.GetComponent<RectTransform>().sizeDelta = size;
        go.GetComponent<Image>().color = bg;

        GameObject textGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        textGo.transform.SetParent(go.transform, false);
        RectTransform textRt = textGo.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;
        TextMeshProUGUI tmp = textGo.GetComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 18;
        tmp.color = textColor;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
        if (font != null) tmp.font = font;

        return go.GetComponent<Button>();
    }

    TMP_InputField CreateInputField(Transform parent, string name, string placeholder)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(TMP_InputField));
        go.transform.SetParent(parent, false);
        go.GetComponent<Image>().color = inputBg;

        GameObject phGo = new GameObject("Placeholder", typeof(RectTransform), typeof(TextMeshProUGUI));
        phGo.transform.SetParent(go.transform, false);
        RectTransform phRt = phGo.GetComponent<RectTransform>();
        phRt.anchorMin = Vector2.zero;
        phRt.anchorMax = Vector2.one;
        phRt.offsetMin = new Vector2(10, 0);
        phRt.offsetMax = new Vector2(-10, 0);
        TextMeshProUGUI phTmp = phGo.GetComponent<TextMeshProUGUI>();
        phTmp.text = placeholder;
        phTmp.fontSize = 14;
        phTmp.color = placeholderColor;
        phTmp.alignment = TextAlignmentOptions.Left;
        if (font != null) phTmp.font = font;

        GameObject txtGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        txtGo.transform.SetParent(go.transform, false);
        RectTransform txtRt = txtGo.GetComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero;
        txtRt.anchorMax = Vector2.one;
        txtRt.offsetMin = new Vector2(10, 0);
        txtRt.offsetMax = new Vector2(-10, 0);
        TextMeshProUGUI txtTmp = txtGo.GetComponent<TextMeshProUGUI>();
        txtTmp.fontSize = 14;
        txtTmp.color = whiteText;
        txtTmp.alignment = TextAlignmentOptions.Left;
        if (font != null) txtTmp.font = font;

        TMP_InputField input = go.GetComponent<TMP_InputField>();
        input.textViewport = txtRt;
        input.textComponent = txtTmp;
        input.placeholder = phTmp;
        return input;
    }

    Slider CreateSlider(Transform parent, string name, Color fillColor, Color bgColor, Vector2 size)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Slider));
        go.transform.SetParent(parent, false);
        go.GetComponent<RectTransform>().sizeDelta = size;

        GameObject bgGo = new GameObject("BG", typeof(RectTransform), typeof(Image));
        bgGo.transform.SetParent(go.transform, false);
        RectTransform bgRt = bgGo.GetComponent<RectTransform>();
        bgRt.anchorMin = new Vector2(0, 0.3f);
        bgRt.anchorMax = new Vector2(1, 0.7f);
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;
        bgGo.GetComponent<Image>().color = bgColor;

        GameObject fillGo = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fillGo.transform.SetParent(go.transform, false);
        RectTransform fillRt = fillGo.GetComponent<RectTransform>();
        fillRt.anchorMin = new Vector2(0, 0.3f);
        fillRt.anchorMax = new Vector2(1, 0.7f);
        fillRt.offsetMin = Vector2.zero;
        fillRt.offsetMax = Vector2.zero;
        fillGo.GetComponent<Image>().color = fillColor;

        GameObject handle = new GameObject("Handle", typeof(RectTransform), typeof(Image));
        handle.transform.SetParent(go.transform, false);
        handle.GetComponent<RectTransform>().sizeDelta = new Vector2(20, 20);
        handle.GetComponent<Image>().color = whiteText;

        Slider slider = go.GetComponent<Slider>();
        slider.fillRect = fillRt;
        slider.handleRect = handle.GetComponent<RectTransform>();
        slider.targetGraphic = handle.GetComponent<Image>();
        slider.value = 0.2f;
        return slider;
    }

    void CreateBadge(Transform parent, string text, float x, float y, Color color)
    {
        GameObject badge = CreatePanel(parent, "Badge" + text, color);
        SetPos(badge, x, y);
        SetSize(badge, text.Length * 14 + 20, 30);
        TextMeshProUGUI badgeText = CreateText(badge.transform, "Text", text, 12, whiteText, TextAlignmentOptions.Center, true);
        StretchFull(badgeText.gameObject);
    }

    void StretchFull(GameObject go)
    {
        RectTransform rt = go.GetComponent<RectTransform>();
        if (rt == null) return;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    void SetPos(GameObject go, float x, float y)
    {
        RectTransform rt = go.GetComponent<RectTransform>();
        if (rt != null) rt.anchoredPosition = new Vector2(x, y);
    }

    void SetSize(GameObject go, float w, float h)
    {
        RectTransform rt = go.GetComponent<RectTransform>();
        if (rt != null) rt.sizeDelta = new Vector2(w, h);
    }
}
