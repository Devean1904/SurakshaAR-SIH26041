using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ObjectClassifier : MonoBehaviour
{
    public static ObjectClassifier Instance { get; private set; }

    public enum ObjectType
    {
        Unknown,
        Floor,
        Wall,
        Ceiling,
        Pipe,
        SupportBeam,
        Doorway,
        Equipment,
        VentilationDuct,
        CableTray
    }

    [System.Serializable]
    public class ClassificationResult
    {
        public ObjectType Type;
        public float Confidence;
        public string Label;
        public ARPlane Plane;
        public Pose Pose;
    }

    [Header("Classification Settings")]
    [SerializeField] private float _pipeAspectRatioMin = 3f;
    [SerializeField] private float _pipeAspectRatioMax = 20f;
    [SerializeField] private float _supportBeamMinWidth = 0.05f;
    [SerializeField] private float _supportBeamMaxWidth = 0.5f;
    [SerializeField] private float _ductMinWidth = 0.3f;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public ClassificationResult Classify(ARPlane plane, Pose hitPose)
    {
        var result = new ClassificationResult
        {
            Plane = plane,
            Pose = hitPose,
            Type = ObjectType.Unknown,
            Confidence = 0f,
            Label = "Unknown"
        };

        if (plane == null)
        {
            result.Type = ObjectType.Wall;
            result.Confidence = 0.3f;
            result.Label = "Wall (assumed)";
            return result;
        }

        float sizeX = plane.size.x;
        float sizeY = plane.size.y;
        float aspect = Mathf.Max(sizeX, sizeY) / Mathf.Max(Mathf.Min(sizeX, sizeY), 0.001f);
        float area = sizeX * sizeY;

        // AR Foundation 6.0: plane.alignment values are ints
        // 0 = HorizontalUp, 1 = HorizontalDown, 2 = Vertical, 3 = None
        int alignment = (int)plane.alignment;

        if (alignment == 0)
            result = ClassifyHorizontal(plane, hitPose, aspect, area);
        else if (alignment == 1)
        {
            result.Type = ObjectType.Ceiling;
            result.Confidence = 0.8f;
            result.Label = "Ceiling/Roof";
        }
        else if (alignment == 2)
            result = ClassifyVertical(plane, hitPose, aspect, area);
        else
            result = ClassifyArbitrary(plane, hitPose, aspect, area);

        Debug.Log($"[ObjectClassifier] Classified as: {result.Type} (confidence: {result.Confidence:F2})");
        return result;
    }

    ClassificationResult ClassifyHorizontal(ARPlane plane, Pose hitPose, float aspect, float area)
    {
        var result = new ClassificationResult
        {
            Plane = plane,
            Pose = hitPose
        };

        if (aspect > _pipeAspectRatioMin && area < 0.5f)
        {
            result.Type = ObjectType.Pipe;
            result.Confidence = 0.6f;
            result.Label = "Horizontal Pipe";
        }
        else if (area > 2f)
        {
            result.Type = ObjectType.Floor;
            result.Confidence = 0.85f;
            result.Label = "Floor/Ground";
        }
        else if (aspect > 2f && area < 1f)
        {
            result.Type = ObjectType.VentilationDuct;
            result.Confidence = 0.5f;
            result.Label = "Ventilation Duct";
        }
        else
        {
            result.Type = ObjectType.Floor;
            result.Confidence = 0.6f;
            result.Label = "Horizontal Surface";
        }

        return result;
    }

    ClassificationResult ClassifyVertical(ARPlane plane, Pose hitPose, float aspect, float area)
    {
        var result = new ClassificationResult
        {
            Plane = plane,
            Pose = hitPose
        };

        float width = Mathf.Min(plane.size.x, plane.size.y);
        float height = Mathf.Max(plane.size.x, plane.size.y);

        if (aspect > _pipeAspectRatioMin && width < 0.15f)
        {
            result.Type = ObjectType.Pipe;
            result.Confidence = 0.7f;
            result.Label = "Vertical Pipe";
        }
        else if (width >= _supportBeamMinWidth && width <= _supportBeamMaxWidth && height > 0.5f)
        {
            result.Type = ObjectType.SupportBeam;
            result.Confidence = 0.65f;
            result.Label = "Support Beam";
        }
        else if (width > _ductMinWidth && aspect > 1.5f)
        {
            result.Type = ObjectType.VentilationDuct;
            result.Confidence = 0.55f;
            result.Label = "Ventilation Duct";
        }
        else if (area > 1.5f)
        {
            result.Type = ObjectType.Wall;
            result.Confidence = 0.75f;
            result.Label = "Wall";
        }
        else
        {
            result.Type = ObjectType.Wall;
            result.Confidence = 0.5f;
            result.Label = "Vertical Surface";
        }

        return result;
    }

    ClassificationResult ClassifyArbitrary(ARPlane plane, Pose hitPose, float aspect, float area)
    {
        return new ClassificationResult
        {
            Plane = plane,
            Pose = hitPose,
            Type = ObjectType.Equipment,
            Confidence = 0.3f,
            Label = "Angled Surface"
        };
    }

    public ClassificationResult ForceClassify(ObjectType type, ARPlane plane, Pose hitPose)
    {
        return new ClassificationResult
        {
            Type = type,
            Confidence = 1.0f,
            Label = type.ToString(),
            Plane = plane,
            Pose = hitPose
        };
    }

    public string GetLocalizedLabel(ObjectType type, string lang = "en")
    {
        return type switch
        {
            ObjectType.Pipe => "Pipe",
            ObjectType.SupportBeam => "Support Beam",
            ObjectType.Wall => "Wall",
            ObjectType.Floor => "Floor",
            ObjectType.Ceiling => "Ceiling",
            ObjectType.VentilationDuct => "Ventilation Duct",
            ObjectType.CableTray => "Cable Tray",
            ObjectType.Doorway => "Doorway",
            ObjectType.Equipment => "Equipment",
            _ => "Unknown Surface"
        };
    }
}