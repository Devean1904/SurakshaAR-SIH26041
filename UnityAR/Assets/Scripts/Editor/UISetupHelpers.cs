#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public static class UI
{
    public static readonly Color BgDark = new Color(0.051f, 0.055f, 0.071f);
    public static readonly Color Surface = new Color(0.102f, 0.110f, 0.141f);
    public static readonly Color CardBg = new Color(0.133f, 0.141f, 0.176f);
    public static readonly Color Primary = new Color(0.231f, 0.510f, 0.965f);
    public static readonly Color Success = new Color(0.133f, 0.773f, 0.369f);
    public static readonly Color Warning = new Color(0.918f, 0.702f, 0.031f);
    public static readonly Color Danger = new Color(0.937f, 0.267f, 0.267f);
    public static readonly Color White = Color.white;
    public static readonly Color TextSec = new Color(0.580f, 0.639f, 0.722f);
    public static readonly Color TextMuted = new Color(0.369f, 0.412f, 0.478f);
    public static readonly Color InputBg = new Color(0.090f, 0.094f, 0.118f);
    public static readonly Color Border = new Color(0.176f, 0.188f, 0.224f);
    public static readonly Color Purple = new Color(0.545f, 0.361f, 0.965f);

    public static GameObject Canvas(string name)
    {
        var go = new GameObject(name);
        var c = go.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        var s = go.AddComponent<CanvasScaler>();
        s.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        s.referenceResolution = new Vector2(1080, 1920);
        s.matchWidthOrHeight = 0.5f;
        go.AddComponent<GraphicRaycaster>();
        return go;
    }

    public static void EventSys()
    {
        var go = new GameObject("EventSystem");
        go.AddComponent<UnityEngine.EventSystems.EventSystem>();
        go.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
    }

    public static GameObject P(Transform p, string n, Color c)
    {
        var go = new GameObject(n, typeof(RectTransform));
        go.transform.SetParent(p, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        go.AddComponent<Image>().color = c;
        return go;
    }

    public static GameObject PA(Transform p, string n, Color c, float x0, float y0, float x1, float y1)
    {
        var go = new GameObject(n, typeof(RectTransform));
        go.transform.SetParent(p, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(x0, y0); rt.anchorMax = new Vector2(x1, y1);
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        go.AddComponent<Image>().color = c;
        return go;
    }

    public static void SA(GameObject go, float x0, float y0, float x1, float y1)
    {
        var rt = go.GetComponent<RectTransform>();
        if (rt == null) return;
        rt.anchorMin = new Vector2(x0, y0); rt.anchorMax = new Vector2(x1, y1);
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }

    public static GameObject T(Transform p, string n, string text, int sz, Color c, float x0 = 0.05f, float y0 = 0.5f, float x1 = 0.95f, float y1 = 0.6f)
    {
        var go = new GameObject(n, typeof(RectTransform));
        go.transform.SetParent(p, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(x0, y0); rt.anchorMax = new Vector2(x1, y1);
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.fontSize = sz; tmp.color = c;
        tmp.alignment = TextAlignmentOptions.Center;
        return go;
    }

    public static GameObject TA(Transform p, string n, string text, int sz, Color c, float x0 = 0.05f, float y0 = 0.5f, float x1 = 0.95f, float y1 = 0.6f)
    {
        var go = T(p, n, text, sz, c, x0, y0, x1, y1);
        go.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.MidlineLeft;
        return go;
    }

    public static GameObject B(Transform p, string n, string label, Color bg, float x0, float y0, float x1, float y1, float fs = 16f)
    {
        var go = new GameObject(n, typeof(RectTransform));
        go.transform.SetParent(p, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(x0, y0); rt.anchorMax = new Vector2(x1, y1);
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        go.AddComponent<Image>().color = bg;
        go.AddComponent<Button>();
        var l = new GameObject("Label", typeof(RectTransform));
        l.transform.SetParent(go.transform, false);
        var lr = l.GetComponent<RectTransform>();
        lr.anchorMin = Vector2.zero; lr.anchorMax = Vector2.one;
        lr.offsetMin = Vector2.zero; lr.offsetMax = Vector2.zero;
        var lt = l.AddComponent<TextMeshProUGUI>();
        lt.text = label; lt.fontSize = fs; lt.color = White;
        lt.alignment = TextAlignmentOptions.Center;
        return go;
    }

    public static GameObject I(Transform p, string n, string ph, float x0, float y0, float x1, float y1)
    {
        var go = new GameObject(n, typeof(RectTransform));
        go.transform.SetParent(p, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(x0, y0); rt.anchorMax = new Vector2(x1, y1);
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        go.AddComponent<Image>().color = InputBg;
        var inp = go.AddComponent<TMP_InputField>();
        inp.textViewport = rt;
        var pho = new GameObject("Placeholder", typeof(RectTransform));
        pho.transform.SetParent(go.transform, false);
        var phr = pho.GetComponent<RectTransform>();
        phr.anchorMin = new Vector2(0.05f, 0f); phr.anchorMax = new Vector2(0.95f, 1f);
        phr.offsetMin = Vector2.zero; phr.offsetMax = Vector2.zero;
        var pht = pho.AddComponent<TextMeshProUGUI>();
        pht.text = ph; pht.fontSize = 16; pht.color = TextMuted;
        pht.alignment = TextAlignmentOptions.MidlineLeft;
        inp.placeholder = pht;
        var tx = new GameObject("Text", typeof(RectTransform));
        tx.transform.SetParent(go.transform, false);
        var txr = tx.GetComponent<RectTransform>();
        txr.anchorMin = new Vector2(0.05f, 0f); txr.anchorMax = new Vector2(0.95f, 1f);
        txr.offsetMin = Vector2.zero; txr.offsetMax = Vector2.zero;
        var txt = tx.AddComponent<TextMeshProUGUI>();
        txt.fontSize = 16; txt.color = White;
        txt.alignment = TextAlignmentOptions.MidlineLeft;
        inp.textComponent = txt;
        return go;
    }

    public static GameObject Tog(Transform p, string n, string label, float x0, float y0, float x1, float y1)
    {
        var go = new GameObject(n, typeof(RectTransform));
        go.transform.SetParent(p, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(x0, y0); rt.anchorMax = new Vector2(x1, y1);
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        go.AddComponent<Toggle>();
        var bg = new GameObject("Background", typeof(RectTransform));
        bg.transform.SetParent(go.transform, false);
        var brt = bg.GetComponent<RectTransform>();
        brt.anchorMin = new Vector2(0f, 0.15f); brt.anchorMax = new Vector2(0.07f, 0.85f);
        brt.offsetMin = Vector2.zero; brt.offsetMax = Vector2.zero;
        bg.AddComponent<Image>().color = Border;
        var ck = new GameObject("Checkmark", typeof(RectTransform));
        ck.transform.SetParent(bg.transform, false);
        var crt = ck.GetComponent<RectTransform>();
        crt.anchorMin = Vector2.one * 0.1f; crt.anchorMax = Vector2.one * 0.9f;
        crt.offsetMin = Vector2.zero; crt.offsetMax = Vector2.zero;
        ck.AddComponent<Image>().color = Primary;
        go.GetComponent<Toggle>().graphic = ck.GetComponent<Image>();
        var lb = new GameObject("Label", typeof(RectTransform));
        lb.transform.SetParent(go.transform, false);
        var lrt = lb.GetComponent<RectTransform>();
        lrt.anchorMin = new Vector2(0.1f, 0f); lrt.anchorMax = new Vector2(1f, 1f);
        lrt.offsetMin = Vector2.zero; lrt.offsetMax = Vector2.zero;
        var lt = lb.AddComponent<TextMeshProUGUI>();
        lt.text = label; lt.fontSize = 14; lt.color = TextSec;
        lt.alignment = TextAlignmentOptions.MidlineLeft;
        return go;
    }

    public static GameObject Card(Transform p, string n, string title, string desc, Color accent, float x0, float y0, float x1, float y1)
    {
        var go = new GameObject(n, typeof(RectTransform));
        go.transform.SetParent(p, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(x0, y0); rt.anchorMax = new Vector2(x1, y1);
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        go.AddComponent<Image>().color = CardBg;
        go.AddComponent<Button>();
        var bar = PA(go.transform, "AccentBar", accent, 0f, 0f, 0.015f, 1f);
        var icon = PA(go.transform, "Icon", accent, 0.04f, 0.25f, 0.15f, 0.75f);
        var t1 = T(go.transform, "Title", title, 18, White, 0.18f, 0.5f, 0.88f, 0.9f);
        t1.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;
        t1.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.MidlineLeft;
        var t2 = T(go.transform, "Desc", desc, 13, TextSec, 0.18f, 0.1f, 0.88f, 0.5f);
        t2.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.MidlineLeft;
        t2.GetComponent<TextMeshProUGUI>().textWrappingMode = TextWrappingModes.Normal;
        var arr = T(go.transform, "Arrow", ">", 22, TextMuted, 0.9f, 0.35f, 0.97f, 0.65f);
        return go;
    }

    public static GameObject Stat(Transform p, string n, string val, string label, Color accent, float x0, float y0, float x1, float y1)
    {
        var go = PA(p, n, CardBg, x0, y0, x1, y1);
        var v = T(go.transform, "Value", val, 28, accent, 0.05f, 0.55f, 0.95f, 0.9f);
        v.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;
        var l = T(go.transform, "Label", label, 12, TextSec, 0.05f, 0.1f, 0.95f, 0.45f);
        return go;
    }
}
#endif
