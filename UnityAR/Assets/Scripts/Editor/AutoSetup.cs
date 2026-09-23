using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class AutoSetup : EditorWindow
{
    [MenuItem("SurakshaAR/Complete Auto Setup")]
    static void Open()
    {
        GetWindow<AutoSetup>("Auto Setup");
    }

    void OnGUI()
    {
        GUILayout.Label("Unity AR Auto Setup", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        if (GUILayout.Button("BUILD ALL", GUILayout.Height(40)))
        {
            RunAll();
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.LabelField("Individual Scene Setup:", EditorStyles.boldLabel);

        if (GUILayout.Button("Build Login Scene"))
        {
            LoginSceneSetup.Build();
        }
        if (GUILayout.Button("Build Main Scene"))
        {
            MainSceneSetup.Build();
        }
        if (GUILayout.Button("Build AR Scene"))
        {
            ARSceneSetup.Build();
        }
    }

    public static void RunAll()
    {
        CreateDirs();
        CreatePrefabs();
        CreateMaterials();
        CreateAppIcon();

        LoginSceneSetup.Build();
        SceneWiring.WireLogin();
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());

        MainSceneSetup.Build();
        SceneWiring.WireMain();
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());

        ARSceneSetup.Build();
        SceneWiring.WireAR();
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());

        Debug.Log("[AutoSetup] BUILD ALL COMPLETE - All scenes created, built, wired, and saved.");
        EditorUtility.DisplayDialog(
            "Setup Complete",
            "All scenes built, wired, and saved.\n\n" +
            "Console should show:\n" +
            "  [SceneWiring] LoginScene wired.\n" +
            "  [SceneWiring] MainScene wired.\n" +
            "  [SceneWiring] ARScene wired.\n\n" +
            "Open LoginScene and press Play to test.",
            "OK"
        );
    }

    static void CreateDirs()
    {
        string[] dirs = new string[]
        {
            "Assets/Scenes",
            "Assets/Prefabs/Panels",
            "Assets/Prefabs/Scenarios",
            "Assets/Prefabs/UI",
            "Assets/Prefabs/MineObjects",
            "Assets/Materials",
            "Assets/Audio",
            "Assets/Icons",
            "Assets/Sprites",
            "Assets/Resources/Lang"
        };

        foreach (string dir in dirs)
        {
            string full = Path.Combine(Application.dataPath, "..", dir);
            if (!Directory.Exists(full))
            {
                Directory.CreateDirectory(full);
                Debug.Log("[AutoSetup] Created directory: " + dir);
            }
        }

        AssetDatabase.Refresh();
    }

    static void CreatePrefabs()
    {
        CreateScenarioPrefab("FirePrefab", new Color(0.9f, 0.15f, 0.1f));
        CreateScenarioPrefab("GasLeakPrefab", new Color(0.1f, 0.8f, 0.1f));
        CreateScenarioPrefab("StructuralPrefab", new Color(0.55f, 0.3f, 0.1f));
        CreateScenarioPrefab("EvacuationPrefab", Color.cyan);
        CreateScenarioPrefab("SafetyEquipmentPrefab", Color.yellow);
        CreateScenarioPrefab("ChemicalSpillPrefab", new Color(0.55f, 0.0f, 0.85f));
        CreateDomainButtonPrefab();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    static void CreateDomainButtonPrefab()
    {
        string path = "Assets/Prefabs/UI/DomainButton.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) return;

        var go = new GameObject("DomainButton", typeof(RectTransform));
        var img = go.AddComponent<Image>();
        img.color = new Color(0.133f, 0.141f, 0.176f);
        go.AddComponent<Button>();
        var lbl = new GameObject("Label", typeof(RectTransform));
        lbl.transform.SetParent(go.transform, false);
        var lr = lbl.GetComponent<RectTransform>();
        lr.anchorMin = new Vector2(0.05f, 0f);
        lr.anchorMax = new Vector2(0.95f, 1f);
        lr.offsetMin = Vector2.zero;
        lr.offsetMax = Vector2.zero;
        var lt = lbl.AddComponent<TextMeshProUGUI>();
        lt.text = "Domain";
        lt.fontSize = 18;
        lt.color = Color.white;
        lt.alignment = TextAlignmentOptions.MidlineLeft;
        var le = go.AddComponent<LayoutElement>();
        le.minHeight = 70;
        le.preferredHeight = 70;
        PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        Debug.Log("[AutoSetup] Created DomainButton prefab");
    }

    static void CreateScenarioPrefab(string name, Color color)
    {
        string path = "Assets/Prefabs/Scenarios/" + name + ".prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
            return;

        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.localScale = Vector3.one * 0.5f;

        Renderer r = go.GetComponent<Renderer>();
        if (r != null)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = color;
            AssetDatabase.CreateAsset(mat, "Assets/Materials/" + name + "Mat.mat");
            r.sharedMaterial = mat;
        }

        PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        Debug.Log("[AutoSetup] Created prefab: " + path);
    }

    static void CreateMaterials()
    {
        CreateMaterial("FireMat", new Color(0.9f, 0.15f, 0.1f));
        CreateMaterial("GasMat", new Color(0.1f, 0.8f, 0.1f));
        CreateMaterial("PipeMat", new Color(0.4f, 0.4f, 0.4f));
        CreateMaterial("SupportMat", new Color(0.35f, 0.25f, 0.15f));
        CreateMaterial("SteelMat", new Color(0.6f, 0.6f, 0.65f));
        CreateMaterial("WarningMat", new Color(1f, 0.65f, 0f));
        CreateMaterial("DangerMat", new Color(0.85f, 0.0f, 0.0f));
        CreateMaterial("SafeMat", new Color(0.0f, 0.7f, 0.0f));

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    static void CreateMaterial(string name, Color color)
    {
        string path = "Assets/Materials/" + name + ".mat";
        if (AssetDatabase.LoadAssetAtPath<Material>(path) != null)
            return;

        Material mat = new Material(Shader.Find("Standard"));
        mat.color = color;
        AssetDatabase.CreateAsset(mat, path);
        Debug.Log("[AutoSetup] Created material: " + path);
    }

    static void CreateAppIcon()
    {
        string dir = "Assets/Icons";
        string full = Path.Combine(Application.dataPath, "..", dir);
        if (!Directory.Exists(full))
            Directory.CreateDirectory(full);

        string path = dir + "/AppIcon.png";
        if (AssetDatabase.LoadAssetAtPath<Texture2D>(path) != null)
            return;

        int size = 512;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);

        Color bgColor = new Color(0.12f, 0.14f, 0.2f, 1f);
        Color shieldColor = new Color(0.2f, 0.55f, 0.9f, 1f);
        Color accentColor = new Color(1f, 0.8f, 0.2f, 1f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                tex.SetPixel(x, y, bgColor);
            }
        }

        float cx = size * 0.5f;
        float cy = size * 0.5f;
        float w = size * 0.35f;
        float h = size * 0.42f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = (x - cx) / w;
                float dy = (y - cy) / h;

                float shieldShape = dx * dx + (dy + 0.15f) * (dy + 0.15f);
                float outerR = 0.65f;
                float innerR = 0.55f;

                if (dy < 0f)
                {
                    float edgeDx = Mathf.Abs(dx);
                    float taper = Mathf.Clamp01((dy + 0.5f) * 2f);
                    float edgeX = Mathf.Lerp(0f, 1f, taper);
                    if (edgeDx > edgeX * 0.6f)
                        continue;
                }

                if (shieldShape < outerR * outerR && shieldShape > innerR * innerR)
                {
                    tex.SetPixel(x, y, shieldColor);
                }

                if (dx * dx + (dy - 0.05f) * (dy - 0.05f) < 0.12f)
                {
                    tex.SetPixel(x, y, shieldColor);
                }
            }
        }

        for (int i = -3; i <= 3; i++)
        {
            int px = (int)cx + i;
            int py;
            for (int t = 0; t < 15; t++)
            {
                py = (int)(cy + 30 + t);
                if (px >= 0 && px < size && py >= 0 && py < size)
                    tex.SetPixel(px, py, accentColor);
            }
        }

        for (int i = -8; i <= 8; i++)
        {
            int px = (int)cx + i;
            int py = (int)(cy + 45);
            if (px >= 0 && px < size && py >= 0 && py < size)
                tex.SetPixel(px, py, accentColor);
        }

        for (int y = (int)(cy + 25); y < (int)(cy + 50); y++)
        {
            for (int x = (int)(cx - 1); x <= (int)(cx + 1); x++)
            {
                if (x >= 0 && x < size && y >= 0 && y < size)
                    tex.SetPixel(x, y, accentColor);
            }
        }

        for (int x = (int)(cx - 12); x <= (int)(cx + 12); x++)
        {
            int y = (int)(cy + 35);
            if (x >= 0 && x < size && y >= 0 && y < size)
                tex.SetPixel(x, y, accentColor);
        }

        tex.Apply();

        byte[] png = tex.EncodeToPNG();
        File.WriteAllBytes(Path.Combine(Application.dataPath, "..", path), png);
        Object.DestroyImmediate(tex);

        AssetDatabase.Refresh();
        Debug.Log("[AutoSetup] Created app icon: " + path);
    }
}
