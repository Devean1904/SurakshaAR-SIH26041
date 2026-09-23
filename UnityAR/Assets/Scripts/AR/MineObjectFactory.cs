using UnityEngine;

public class MineObjectFactory : MonoBehaviour
{
    public static MineObjectFactory Instance { get; private set; }

    private Material _pipeMat;
    private Material _rustPipeMat;
    private Material _supportMat;
    private Material _steelMat;
    private Material _gasDetectorMat;
    private Material _ventMat;
    private Material _cableMat;
    private Material _warningStripeMat;

    private Color _pipeColor = new(0.4f, 0.45f, 0.5f, 1f);
    private Color _rustColor = new(0.5f, 0.25f, 0.1f, 1f);
    private Color _supportColor = new(0.35f, 0.35f, 0.38f, 1f);
    private Color _steelColor = new(0.6f, 0.6f, 0.65f, 1f);
    private Color _gasDetectorColor = new(1f, 0.8f, 0f, 1f);
    private Color _ventColor = new(0.45f, 0.45f, 0.5f, 1f);
    private Color _cableColor = new(0.1f, 0.1f, 0.15f, 1f);
    private Color _warningColor = new(1f, 0.8f, 0f, 1f);

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        CreateMaterials();
    }

    void CreateMaterials()
    {
        var shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");

        _pipeMat = new Material(shader) { name = "PipeMat" };
        _pipeMat.color = _pipeColor;
        _pipeMat.SetFloat("_Metallic", 0.7f);
        _pipeMat.SetFloat("_Smoothness", 0.3f);

        _rustPipeMat = new Material(shader) { name = "RustPipeMat" };
        _rustPipeMat.color = _rustColor;
        _rustPipeMat.SetFloat("_Metallic", 0.3f);
        _rustPipeMat.SetFloat("_Smoothness", 0.2f);

        _supportMat = new Material(shader) { name = "SupportMat" };
        _supportMat.color = _supportColor;
        _supportMat.SetFloat("_Metallic", 0.6f);
        _supportMat.SetFloat("_Smoothness", 0.4f);

        _steelMat = new Material(shader) { name = "SteelMat" };
        _steelMat.color = _steelColor;
        _steelMat.SetFloat("_Metallic", 0.8f);
        _steelMat.SetFloat("_Smoothness", 0.5f);

        _gasDetectorMat = new Material(shader) { name = "GasDetectorMat" };
        _gasDetectorMat.color = _gasDetectorColor;
        _gasDetectorMat.SetFloat("_Metallic", 0.1f);
        _gasDetectorMat.SetFloat("_Smoothness", 0.3f);
        _gasDetectorMat.EnableKeyword("_EMISSION");
        _gasDetectorMat.SetColor("_EmissionColor", _gasDetectorColor * 2f);

        _ventMat = new Material(shader) { name = "VentMat" };
        _ventMat.color = _ventColor;
        _ventMat.SetFloat("_Metallic", 0.5f);
        _ventMat.SetFloat("_Smoothness", 0.3f);

        _cableMat = new Material(shader) { name = "CableMat" };
        _cableMat.color = _cableColor;
        _cableMat.SetFloat("_Metallic", 0.2f);
        _cableMat.SetFloat("_Smoothness", 0.6f);

        _warningStripeMat = new Material(shader) { name = "WarningStripeMat" };
        _warningStripeMat.color = _warningColor;
        _warningStripeMat.SetFloat("_Metallic", 0.1f);
        _warningStripeMat.SetFloat("_Smoothness", 0.2f);
        _warningStripeMat.EnableKeyword("_EMISSION");
        _warningStripeMat.SetColor("_EmissionColor", _warningColor * 1.5f);
    }

    public GameObject CreateMethanePipe(Vector3 position, Quaternion rotation, float length = 2f, Transform parent = null)
    {
        var pipe = new GameObject("MethanePipe");
        pipe.transform.SetPositionAndRotation(position, rotation);
        if (parent != null) pipe.transform.SetParent(parent);

        var mainPipe = CreateCylinder("MainPipe", Vector3.zero, new Vector3(0.06f, length / 2f, 0.06f), _pipeMat, pipe.transform);
        mainPipe.transform.localScale = new Vector3(0.06f, length / 2f, 0.06f);

        var joint1 = CreateCylinder("Joint1", new Vector3(0, length * 0.3f, 0), new Vector3(0.08f, 0.03f, 0.08f), _rustPipeMat, pipe.transform);
        var joint2 = CreateCylinder("Joint2", new Vector3(0, -length * 0.3f, 0), new Vector3(0.08f, 0.03f, 0.08f), _rustPipeMat, pipe.transform);

        var valve = CreateValve(pipe.transform, new Vector3(0.06f, 0, 0));

        var label = new GameObject("GasLabel");
        label.transform.SetParent(pipe.transform);
        label.transform.localPosition = new Vector3(0, 0, 0.035f);
        var textMesh = label.AddComponent<TMPro.TextMeshPro>();
        textMesh.text = "CH4\nMETHANE";
        textMesh.fontSize = 1.5f;
        textMesh.alignment = TMPro.TextAlignmentOptions.Center;
        textMesh.color = new Color(1f, 0.8f, 0f);

        var collider = pipe.AddComponent<BoxCollider>();
        collider.size = new Vector3(0.1f, length, 0.1f);
        collider.center = Vector3.zero;

        var tag = pipe.AddComponent<MineObjectTag>();
        tag.ObjectType = "pipe";
        tag.SubType = "methane";

        Debug.Log($"[MineObjectFactory] Created methane pipe: length={length}m");
        return pipe;
    }

    public GameObject CreateSupportBeam(Vector3 position, Quaternion rotation, float height = 3f, Transform parent = null)
    {
        var beam = new GameObject("SupportBeam");
        beam.transform.SetPositionAndRotation(position, rotation);
        if (parent != null) beam.transform.SetParent(parent);

        var vertical = CreateCylinder("VerticalPost", Vector3.zero, new Vector3(0.1f, height / 2f, 0.1f), _supportMat, beam.transform);

        var topBeam = CreateCylinder("TopBeam", new Vector3(0, height * 0.45f, 0), new Vector3(0.08f, 0.02f, 0.08f), _steelMat, beam.transform);
        topBeam.transform.localScale = new Vector3(1.5f, 1f, 1f);

        var basePlate = CreateCube("BasePlate", new Vector3(0, -height * 0.5f + 0.01f, 0), new Vector3(0.2f, 0.02f, 0.2f), _steelMat, beam.transform);

        var warningStripe = CreateCube("WarningStripe", new Vector3(0, height * 0.3f, 0.06f), new Vector3(0.12f, 0.04f, 0.001f), _warningStripeMat, beam.transform);

        var bolt1 = CreateCylinder("Bolt1", new Vector3(0.06f, height * 0.45f, 0), new Vector3(0.01f, 0.01f, 0.01f), _steelMat, beam.transform);
        var bolt2 = CreateCylinder("Bolt2", new Vector3(-0.06f, height * 0.45f, 0), new Vector3(0.01f, 0.01f, 0.01f), _steelMat, beam.transform);

        var collider = beam.AddComponent<BoxCollider>();
        collider.size = new Vector3(0.15f, height, 0.15f);

        var tag = beam.AddComponent<MineObjectTag>();
        tag.ObjectType = "support";
        tag.SubType = "beam";

        Debug.Log($"[MineObjectFactory] Created support beam: height={height}m");
        return beam;
    }

    public GameObject CreateGasDetector(Vector3 position, Quaternion rotation, Transform parent = null)
    {
        var detector = new GameObject("GasDetector");
        detector.transform.SetPositionAndRotation(position, rotation);
        if (parent != null) detector.transform.SetParent(parent);

        var body = CreateCube("Body", Vector3.zero, new Vector3(0.08f, 0.12f, 0.04f), _gasDetectorMat, detector.transform);

        var sensor = CreateCylinder("Sensor", new Vector3(0, -0.05f, 0.025f), new Vector3(0.04f, 0.005f, 0.04f), _gasDetectorMat, detector.transform);

        var screen = CreateCube("Screen", new Vector3(0, 0.02f, 0.021f), new Vector3(0.05f, 0.03f, 0.001f), _steelMat, detector.transform);

        var led1 = CreateCylinder("LED_Green", new Vector3(-0.025f, 0.05f, 0.021f), new Vector3(0.008f, 0.002f, 0.008f), _gasDetectorMat, detector.transform);
        var led2 = CreateCylinder("LED_Red", new Vector3(0.025f, 0.05f, 0.021f), new Vector3(0.008f, 0.002f, 0.008f), _warningStripeMat, detector.transform);

        var mount = CreateCube("Mount", new Vector3(0, 0.07f, -0.01f), new Vector3(0.03f, 0.02f, 0.03f), _steelMat, detector.transform);

        var collider = detector.AddComponent<BoxCollider>();
        collider.size = new Vector3(0.1f, 0.15f, 0.06f);

        var tag = detector.AddComponent<MineObjectTag>();
        tag.ObjectType = "detector";
        tag.SubType = "methane";

        Debug.Log("[MineObjectFactory] Created gas detector");
        return detector;
    }

    public GameObject CreateVentilationDuct(Vector3 position, Quaternion rotation, float length = 2f, Transform parent = null)
    {
        var duct = new GameObject("VentilationDuct");
        duct.transform.SetPositionAndRotation(position, rotation);
        if (parent != null) duct.transform.SetParent(parent);

        var mainDuct = CreateCylinder("MainDuct", Vector3.zero, new Vector3(0.2f, length / 2f, 0.2f), _ventMat, duct.transform);

        var ring1 = CreateCylinder("Ring1", new Vector3(0, length * 0.3f, 0), new Vector3(0.22f, 0.01f, 0.22f), _steelMat, duct.transform);
        var ring2 = CreateCylinder("Ring2", new Vector3(0, -length * 0.3f, 0), new Vector3(0.22f, 0.01f, 0.22f), _steelMat, duct.transform);

        var fan = CreateFan(duct.transform, new Vector3(0, -length * 0.45f, 0));

        var collider = duct.AddComponent<BoxCollider>();
        collider.size = new Vector3(0.25f, length, 0.25f);

        var tag = duct.AddComponent<MineObjectTag>();
        tag.ObjectType = "duct";
        tag.SubType = "ventilation";

        Debug.Log($"[MineObjectFactory] Created ventilation duct: length={length}m");
        return duct;
    }

    public GameObject CreateCableTray(Vector3 position, Quaternion rotation, float length = 2f, Transform parent = null)
    {
        var tray = new GameObject("CableTray");
        tray.transform.SetPositionAndRotation(position, rotation);
        if (parent != null) tray.transform.SetParent(parent);

        var basePlate = CreateCube("Base", Vector3.zero, new Vector3(0.15f, 0.005f, length), _steelMat, tray.transform);
        var side1 = CreateCube("Side1", new Vector3(0.075f, 0.025f, 0), new Vector3(0.005f, 0.05f, length), _steelMat, tray.transform);
        var side2 = CreateCube("Side2", new Vector3(-0.075f, 0.025f, 0), new Vector3(0.005f, 0.05f, length), _steelMat, tray.transform);

        for (float z = -length * 0.4f; z <= length * 0.4f; z += 0.3f)
        {
            CreateCylinder($"Cable_{z:F1}", new Vector3(0, 0.015f, z), new Vector3(0.015f, length * 0.01f, 0.015f), _cableMat, tray.transform);
        }

        var collider = tray.AddComponent<BoxCollider>();
        collider.size = new Vector3(0.2f, 0.06f, length);

        var tag = tray.AddComponent<MineObjectTag>();
        tag.ObjectType = "tray";
        tag.SubType = "cable";

        Debug.Log($"[MineObjectFactory] Created cable tray: length={length}m");
        return tray;
    }

    public GameObject CreateMineWall(Vector3 position, Quaternion rotation, float width = 3f, float height = 2.5f, Transform parent = null)
    {
        var wall = new GameObject("MineWall");
        wall.transform.SetPositionAndRotation(position, rotation);
        if (parent != null) wall.transform.SetParent(parent);

        var mainWall = CreateCube("MainWall", Vector3.zero, new Vector3(width, height, 0.15f), _supportMat, wall.transform);

        for (float x = -width * 0.4f; x <= width * 0.4f; x += 0.8f)
        {
            CreateCube($"Bolt_{x:F1}", new Vector3(x, height * 0.3f, 0.08f), new Vector3(0.02f, 0.02f, 0.02f), _steelMat, wall.transform);
        }

        var collider = wall.AddComponent<BoxCollider>();
        collider.size = new Vector3(width, height, 0.15f);

        var tag = wall.AddComponent<MineObjectTag>();
        tag.ObjectType = "wall";
        tag.SubType = "mine_wall";

        Debug.Log($"[MineObjectFactory] Created mine wall: {width}x{height}m");
        return wall;
    }

    public GameObject CreateMineFloor(Vector3 position, float size = 5f, Transform parent = null)
    {
        var floor = new GameObject("MineFloor");
        floor.transform.position = position;
        if (parent != null) floor.transform.SetParent(parent);

        var mainFloor = CreateCube("MainFloor", Vector3.zero, new Vector3(size, 0.1f, size), _supportMat, floor.transform);

        for (float x = -size * 0.4f; x <= size * 0.4f; x += 1f)
        {
            for (float z = -size * 0.4f; z <= size * 0.4f; z += 1f)
            {
                CreateCube($"Tread_{x:F0}_{z:F0}", new Vector3(x, 0.055f, z), new Vector3(0.3f, 0.005f, 0.3f), _rustPipeMat, floor.transform);
            }
        }

        var collider = floor.AddComponent<BoxCollider>();
        collider.size = new Vector3(size, 0.1f, size);

        var tag = floor.AddComponent<MineObjectTag>();
        tag.ObjectType = "floor";
        tag.SubType = "mine_floor";

        Debug.Log($"[MineObjectFactory] Created mine floor: {size}x{size}m");
        return floor;
    }

    GameObject CreateValve(Transform parent, Vector3 localPos)
    {
        var valve = CreateCylinder("Valve", localPos, new Vector3(0.03f, 0.01f, 0.03f), _steelMat, parent);
        var handle = CreateCube("Handle", localPos + new Vector3(0, 0, 0.02f), new Vector3(0.04f, 0.005f, 0.005f), _warningStripeMat, parent);
        return valve;
    }

    GameObject CreateFan(Transform parent, Vector3 localPos)
    {
        var fan = new GameObject("Fan");
        fan.transform.SetParent(parent);
        fan.transform.localPosition = localPos;

        CreateCylinder("FanBody", Vector3.zero, new Vector3(0.18f, 0.02f, 0.18f), _steelMat, fan.transform);

        for (int i = 0; i < 4; i++)
        {
            var blade = CreateCube($"Blade_{i}", Vector3.zero, new Vector3(0.06f, 0.003f, 0.015f), _ventMat, fan.transform);
            blade.transform.localRotation = Quaternion.Euler(0, i * 90f, 30f);
        }

        return fan;
    }

    GameObject CreateCube(string name, Vector3 localPos, Vector3 scale, Material mat, Transform parent)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.localPosition = localPos;
        go.transform.localScale = scale;
        go.transform.SetParent(parent);
        go.GetComponent<Renderer>().material = mat;
        Destroy(go.GetComponent<BoxCollider>());
        return go;
    }

    GameObject CreateCylinder(string name, Vector3 localPos, Vector3 scale, Material mat, Transform parent)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = name;
        go.transform.localPosition = localPos;
        go.transform.localScale = scale;
        go.transform.SetParent(parent);
        go.GetComponent<Renderer>().material = mat;
        Destroy(go.GetComponent<CapsuleCollider>());
        return go;
    }

    void OnDestroy()
    {
        if (_pipeMat != null) Destroy(_pipeMat);
        if (_rustPipeMat != null) Destroy(_rustPipeMat);
        if (_supportMat != null) Destroy(_supportMat);
        if (_steelMat != null) Destroy(_steelMat);
        if (_gasDetectorMat != null) Destroy(_gasDetectorMat);
        if (_ventMat != null) Destroy(_ventMat);
        if (_cableMat != null) Destroy(_cableMat);
        if (_warningStripeMat != null) Destroy(_warningStripeMat);
    }
}

public class MineObjectTag : MonoBehaviour
{
    public string ObjectType;
    public string SubType;
}