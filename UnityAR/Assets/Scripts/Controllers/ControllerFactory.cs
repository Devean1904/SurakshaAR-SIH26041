using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ControllerFactory : MonoBehaviour
{
    public static ControllerFactory Instance { get; private set; }

    private Material _bodyMat;
    private Material _screenMat;
    private Material _headphonesMat;
    private Material _ledMat;
    private Material _antennaMat;
    private Material _visirMat;

    private Color _headColor = new(0.6f, 0.65f, 0.67f, 1f);
    private Color _bodyColor = new(0.18f, 0.2f, 0.22f, 1f);
    private Color _screenColor = new(0f, 0.8f, 1f, 1f);
    private Color _ledColor = Color.red;
    private Color _headphoneColor = new(0.1f, 0.1f, 0.1f, 1f);
    private Color _antennaColor = Color.gray;
    private Color _visirColor = new(0.3f, 0.8f, 1f, 0.6f);

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        CreateMaterials();
    }

    void CreateMaterials()
    {
        _bodyMat = CreateMaterial("ControllerBody", _bodyColor, Metallic(0.8f), 0.3f);
        _screenMat = CreateMaterial("ControllerScreen", _screenColor, Metallic(0f), 0.1f, emission: true);
        _headphonesMat = CreateMaterial("ControllerHeadphones", _headphoneColor, Metallic(0f), 0.8f);
        _ledMat = CreateMaterial("ControllerLED", _ledColor, Metallic(0f), 0f, emission: true);
        _antennaMat = CreateMaterial("ControllerAntenna", _antennaColor, Metallic(0.6f), 0.2f);
        _visirMat = CreateMaterial("ControllerVisor", _visirColor, Metallic(0f), 0.05f, transparent: true);
    }

    Material CreateMaterial(string name, Color color, float metallic, float smoothness, bool emission = false, bool transparent = false)
    {
        var shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        var mat = new Material(shader);
        mat.name = name;
        mat.color = color;
        mat.SetFloat("_Metallic", metallic);
        mat.SetFloat("_Smoothness", smoothness);
        if (emission)
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", color * 2f);
        }
        if (transparent)
        {
            SetMaterialTransparent(mat);
        }
        return mat;
    }

    float Metallic(float m) => m;

    void SetMaterialTransparent(Material mat)
    {
        mat.SetFloat("_Mode", 3);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;
    }

    public GameObject CreateFaceHeadset(Vector3 position, Quaternion rotation, Transform parent = null)
    {
        var headset = new GameObject("FaceHeadset");
        headset.transform.SetPositionAndRotation(position, rotation);
        if (parent != null) headset.transform.SetParent(parent);

        var body = CreateCapsule("Body", new Vector3(0, 0, 0), new Vector3(0.15f, 0.08f, 0.15f), _bodyMat, headset.transform);
        var leftSpeaker = CreateCylinder("LeftSpeaker", new Vector3(-0.09f, 0, 0), new Vector3(0.04f, 0.015f, 0.04f), _bodyMat, headset.transform);
        var rightSpeaker = CreateCylinder("RightSpeaker", new Vector3(0.09f, 0, 0), new Vector3(0.04f, 0.015f, 0.04f), _bodyMat, headset.transform);
        var screen = CreateCube("Screen", new Vector3(0, 0, -0.07f), new Vector3(0.12f, 0.04f, 0.005f), _screenMat, headset.transform);

        headset.tag = "Player";
        var rb = headset.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        headset.AddComponent<BoxCollider>();
        headset.layer = LayerMask.NameToLayer("Default");

        Debug.Log("[ControllerFactory] Face headset created");
        return headset;
    }

    public GameObject CreateHandController(string hand, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        string handName = hand == "left" ? "L" : "R";
        var controller = new GameObject($"XRController_{handName}");
        controller.transform.SetPositionAndRotation(position, rotation);
        if (parent != null) controller.transform.SetParent(parent);

        var handle = CreateCylinder("Handle", Vector3.zero, new Vector3(0.025f, 0.06f, 0.025f), _bodyMat, controller.transform);
        var trigger = CreateCube("Trigger", new Vector3(0, 0.03f, 0.015f), new Vector3(0.015f, 0.025f, 0.008f), _bodyMat, controller.transform);
        var touchpad = CreateCube("Touchpad", new Vector3(0, 0.05f, -0.01f), new Vector3(0.018f, 0.003f, 0.018f), _bodyMat, controller.transform);
        var menuButton = CreateCube("MenuButton", new Vector3(0, 0.05f, 0.015f), new Vector3(0.008f, 0.003f, 0.008f), _ledMat, controller.transform);
        var volumeUp = CreateCube("VolumeUp", new Vector3(-0.012f, 0.06f, 0), new Vector3(0.005f, 0.008f, 0.005f), _bodyMat, controller.transform);
        var volumeDown = CreateCube("VolumeDown", new Vector3(-0.012f, 0.055f, 0), new Vector3(0.005f, 0.008f, 0.005f), _bodyMat, controller.transform);
        var battery = CreateCube("Battery", new Vector3(0, -0.06f, 0), new Vector3(0.02f, 0.015f, 0.02f), _ledMat, controller.transform);

        controller.tag = "Player";
        var rb = controller.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        controller.AddComponent<BoxCollider>();

        Debug.Log($"[ControllerFactory] {handName} controller created");
        return controller;
    }

    public GameObject CreateMetaglass(string hand, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        string handName = hand == "left" ? "L" : "R";
        var metaglass = new GameObject($"Metaglass_{handName}");
        metaglass.transform.SetPositionAndRotation(position, rotation);
        if (parent != null) metaglass.transform.SetParent(parent);

        var frame = CreateCylinder("Frame", Vector3.zero, new Vector3(0.04f, 0.005f, 0.04f), _bodyMat, metaglass.transform);
        var lens = CreateCube("Lens", new Vector3(0, 0, -0.003f), new Vector3(0.035f, 0.035f, 0.003f), _visirMat, metaglass.transform);
        var arm = CreateCube("Arm", new Vector3(0, 0, 0.02f), new Vector3(0.012f, 0.008f, 0.04f), _bodyMat, metaglass.transform);

        metaglass.tag = "Player";
        var rb = metaglass.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        Debug.Log($"[ControllerFactory] Metaglass {handName} created");
        return metaglass;
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

    GameObject CreateCapsule(string name, Vector3 localPos, Vector3 scale, Material mat, Transform parent)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
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
        if (_bodyMat != null) Destroy(_bodyMat);
        if (_screenMat != null) Destroy(_screenMat);
        if (_headphonesMat != null) Destroy(_headphonesMat);
        if (_ledMat != null) Destroy(_ledMat);
        if (_antennaMat != null) Destroy(_antennaMat);
        if (_visirMat != null) Destroy(_visirMat);
    }
}