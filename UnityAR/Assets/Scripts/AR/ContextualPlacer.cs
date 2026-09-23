using System.Collections.Generic;
using UnityEngine;

public class ContextualPlacer : MonoBehaviour
{
    public static ContextualPlacer Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private bool _autoClassify = true;
    [SerializeField] private bool _showDebugInfo = true;

    private Dictionary<string, GameObject> _placedHazards = new();
    private Dictionary<string, ObjectClassifier.ClassificationResult> _classifiedObjects = new();
    private int _objectCounter;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (SurfaceDetector.Instance != null)
        {
            SurfaceDetector.Instance.OnSurfaceDetected += OnSurfaceDetected;
        }
    }

    void OnDestroy()
    {
        if (SurfaceDetector.Instance != null)
        {
            SurfaceDetector.Instance.OnSurfaceDetected -= OnSurfaceDetected;
        }
    }

    void OnSurfaceDetected(Pose pose, UnityEngine.XR.ARFoundation.ARPlane plane)
    {
        if (!_autoClassify) return;

        if (ObjectClassifier.Instance == null) return;

        var classification = ObjectClassifier.Instance.Classify(plane, pose);
        string id = $"surface_{_objectCounter++}";

        _classifiedObjects[id] = classification;

        Debug.Log($"[ContextualPlacer] Auto-classified surface: {classification.Type} ({classification.Label})");

        if (_showDebugInfo)
        {
            ShowDebugLabel(pose, classification);
        }
    }

    public string PlaceHazardAtTap(Vector2 screenPosition)
    {
        if (SurfaceDetector.Instance == null)
        {
            Debug.LogWarning("[ContextualPlacer] SurfaceDetector not found");
            return null;
        }

        if (!SurfaceDetector.Instance.TryRaycast(screenPosition, out var pose, out var plane))
        {
            Debug.Log("[ContextualPlacer] No surface detected at tap position");
            return null;
        }

        return PlaceHazardAtPose(pose, plane);
    }

    public string PlaceHazardAtPose(Pose pose, UnityEngine.XR.ARFoundation.ARPlane plane)
    {
        var classification = ObjectClassifier.Instance != null
            ? ObjectClassifier.Instance.Classify(plane, pose)
            : new ObjectClassifier.ClassificationResult
            {
                Type = ObjectClassifier.ObjectType.Wall,
                Confidence = 0.5f,
                Label = "Default Surface",
                Plane = plane,
                Pose = pose
            };

        string scenarioId = GetCurrentScenarioId();
        var mapping = MineHazardLibrary.Instance?.GetMapping(scenarioId, classification.Type);

        if (mapping == null)
        {
            mapping = GetFallbackMapping(scenarioId, classification.Type);
        }

        if (mapping == null)
        {
            Debug.Log($"[ContextualPlacer] No hazard mapping for {classification.Type} in scenario {scenarioId}");
            return null;
        }

        string hazardId = $"hazard_{_objectCounter++}";
        var hazardObj = CreateHazardVisual(mapping, pose, classification);

        if (hazardObj != null)
        {
            _placedHazards[hazardId] = hazardObj;
            _classifiedObjects[hazardId] = classification;

            Debug.Log($"[ContextualPlacer] Placed hazard: {mapping.HazardEffect} on {classification.Type} at {pose.position}");

            if (ScenarioManager.Instance != null && mapping.HazardEffect.Contains("fire"))
            {
                string actionId = $"detect-fire-{classification.Type.ToString().ToLower()}";
                ScenarioManager.Instance.OnActionCompleted(actionId);
            }

            return hazardId;
        }

        return null;
    }

    public string ForcePlaceAtPose(Pose pose, ObjectClassifier.ObjectType overrideType)
    {
        string scenarioId = GetCurrentScenarioId();
        var mapping = MineHazardLibrary.Instance?.GetMapping(scenarioId, overrideType);

        if (mapping == null)
        {
            mapping = GetFallbackMapping(scenarioId, overrideType);
        }

        if (mapping == null) return null;

        string hazardId = $"hazard_{_objectCounter++}";
        var classification = new ObjectClassifier.ClassificationResult
        {
            Type = overrideType,
            Confidence = 1.0f,
            Label = overrideType.ToString(),
            Pose = pose
        };

        var hazardObj = CreateHazardVisual(mapping, pose, classification);
        if (hazardObj != null)
        {
            _placedHazards[hazardId] = hazardObj;
            _classifiedObjects[hazardId] = classification;
            return hazardId;
        }
        return null;
    }

    GameObject CreateHazardVisual(MineHazardLibrary.HazardMapping mapping, Pose pose, ObjectClassifier.ClassificationResult classification)
    {
        var go = new GameObject($"Hazard_{mapping.HazardEffect}_{_objectCounter}");
        go.transform.SetPositionAndRotation(pose.position, pose.rotation);

        switch (mapping.HazardEffect)
        {
            case "gas_leak_fire":
            case "gas_leak_source":
                CreateGasLeakEffect(go, mapping);
                break;

            case "fire_spread":
            case "electrical_fire":
            case "fire_on_floor":
                CreateFireEffect(go, mapping);
                break;

            case "smoke_wall":
            case "smoke_ceiling":
                CreateSmokeEffect(go, mapping);
                break;

            case "gas_pool":
            case "gas_accumulation":
            case "gas_in_duct":
                CreateGasCloudEffect(go, mapping);
                break;

            case "beam_crack":
            case "wall_crack":
                CreateCrackEffect(go, mapping);
                break;

            case "roof_collapse":
                CreateCollapseEffect(go, mapping);
                break;

            case "rubble_pile":
                CreateRubbleEffect(go, mapping);
                break;

            case "spark_origin":
                CreateSparkEffect(go, mapping);
                break;

            case "fall_zone":
                CreateFallZoneEffect(go, mapping);
                break;

            case "anchor_point":
                CreateAnchorPointEffect(go, mapping);
                break;

            default:
                CreateGenericHazardEffect(go, mapping);
                break;
        }

        return go;
    }

    void CreateGasLeakEffect(GameObject go, MineHazardLibrary.HazardMapping mapping)
    {
        var ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startColor = mapping.TintColor;
        main.startSize = 0.05f;
        main.startLifetime = 2f;
        main.startSpeed = 0.3f;
        main.loop = true;
        main.maxParticles = 50;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.rateOverTime = 20;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.02f;

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        var grad = new Gradient();
        grad.SetKeys(
            new[] { new GradientColorKey(mapping.TintColor, 0f), new GradientColorKey(mapping.TintColor, 1f) },
            new[] { new GradientAlphaKey(0.8f, 0f), new GradientAlphaKey(0f, 1f) }
        );
        colorOverLifetime.color = grad;

        var collider = go.AddComponent<SphereCollider>();
        collider.isTrigger = true;
        collider.radius = 0.5f;
    }

    void CreateFireEffect(GameObject go, MineHazardLibrary.HazardMapping mapping)
    {
        var ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(1f, 0.3f, 0f),
            new Color(1f, 0.8f, 0f)
        );
        main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.15f);
        main.startLifetime = 0.8f;
        main.startSpeed = 1f;
        main.loop = true;
        main.maxParticles = 100;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = -0.3f;

        var emission = ps.emission;
        emission.rateOverTime = 50;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 25f;
        shape.radius = 0.03f;

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        var grad = new Gradient();
        grad.SetKeys(
            new[] {
                new GradientColorKey(new Color(1f, 0.8f, 0f), 0f),
                new GradientColorKey(new Color(1f, 0.3f, 0f), 0.5f),
                new GradientColorKey(new Color(0.5f, 0.1f, 0f), 1f)
            },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
        );
        colorOverLifetime.color = grad;

        go.transform.localScale = Vector3.one * mapping.Scale;
    }

    void CreateSmokeEffect(GameObject go, MineHazardLibrary.HazardMapping mapping)
    {
        var ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startColor = new Color(0.3f, 0.3f, 0.3f, 0.4f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.3f);
        main.startLifetime = 4f;
        main.startSpeed = 0.2f;
        main.loop = true;
        main.maxParticles = 30;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = -0.1f;

        var emission = ps.emission;
        emission.rateOverTime = 8;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(0.5f, 0.01f, 0.1f);

        go.transform.localScale = Vector3.one * mapping.Scale;
    }

    void CreateGasCloudEffect(GameObject go, MineHazardLibrary.HazardMapping mapping)
    {
        var ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startColor = new Color(0.2f, 1f, 0.2f, 0.3f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.2f, 0.5f);
        main.startLifetime = 5f;
        main.startSpeed = 0.05f;
        main.loop = true;
        main.maxParticles = 20;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.rateOverTime = 4;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(1f, 0.01f, 1f);

        go.transform.localScale = Vector3.one * mapping.Scale;
    }

    void CreateCrackEffect(GameObject go, MineHazardLibrary.HazardMapping mapping)
    {
        var lineRenderer = go.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.005f;
        lineRenderer.endWidth = 0.002f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = mapping.TintColor;
        lineRenderer.endColor = new Color(mapping.TintColor.r, mapping.TintColor.g, mapping.TintColor.b, 0.2f);

        int points = Random.Range(5, 10);
        lineRenderer.positionCount = points;
        for (int i = 0; i < points; i++)
        {
            float t = (float)i / (points - 1);
            Vector3 pos = Vector3.Lerp(Vector3.zero, Vector3.up * 0.3f, t);
            pos += new Vector3(Random.Range(-0.02f, 0.02f), 0, Random.Range(-0.02f, 0.02f));
            lineRenderer.SetPosition(i, pos);
        }
    }

    void CreateCollapseEffect(GameObject go, MineHazardLibrary.HazardMapping mapping)
    {
        var ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startColor = new Color(0.4f, 0.3f, 0.2f, 0.8f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.02f, 0.08f);
        main.startLifetime = 1.5f;
        main.startSpeed = 2f;
        main.loop = true;
        main.maxParticles = 80;
        main.gravityModifier = 2f;

        var emission = ps.emission;
        emission.rateOverTime = 30;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(0.5f, 0.01f, 0.5f);

        go.transform.localScale = Vector3.one * mapping.Scale;
    }

    void CreateRubbleEffect(GameObject go, MineHazardLibrary.HazardMapping mapping)
    {
        for (int i = 0; i < 8; i++)
        {
            var rock = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rock.name = "Rubble";
            rock.transform.SetParent(go.transform);
            rock.transform.localPosition = new Vector3(
                Random.Range(-0.2f, 0.2f),
                Random.Range(0f, 0.05f),
                Random.Range(-0.2f, 0.2f)
            );
            rock.transform.localScale = Vector3.one * Random.Range(0.02f, 0.06f);
            rock.transform.rotation = Random.rotation;
            rock.GetComponent<Renderer>().material.color = new Color(0.4f, 0.3f, 0.2f);
        }
        go.transform.localScale = Vector3.one * mapping.Scale;
    }

    void CreateSparkEffect(GameObject go, MineHazardLibrary.HazardMapping mapping)
    {
        var ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startColor = new Color(1f, 1f, 0.5f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.005f, 0.02f);
        main.startLifetime = 0.3f;
        main.startSpeed = 3f;
        main.loop = true;
        main.maxParticles = 40;
        main.gravityModifier = 1f;

        var emission = ps.emission;
        emission.rateOverTime = 20;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.01f;

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        var grad = new Gradient();
        grad.SetKeys(
            new[] {
                new GradientColorKey(Color.yellow, 0f),
                new GradientColorKey(new Color(1f, 0.5f, 0f), 1f)
            },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
        );
        colorOverLifetime.color = grad;

        go.transform.localScale = Vector3.one * mapping.Scale;
    }

    void CreateFallZoneEffect(GameObject go, MineHazardLibrary.HazardMapping mapping)
    {
        var ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startColor = new Color(1f, 0f, 0f, 0.3f);
        main.startSize = 0.3f;
        main.startLifetime = 3f;
        main.startSpeed = 0f;
        main.loop = true;
        main.maxParticles = 10;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.rateOverTime = 3;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(1f, 0.01f, 1f);

        go.transform.localScale = Vector3.one * mapping.Scale;
    }

    void CreateAnchorPointEffect(GameObject go, MineHazardLibrary.HazardMapping mapping)
    {
        var anchor = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        anchor.name = "SafetyAnchor";
        anchor.transform.SetParent(go.transform);
        anchor.transform.localPosition = Vector3.zero;
        anchor.transform.localScale = Vector3.one * 0.05f;
        anchor.GetComponent<Renderer>().material.color = Color.green;

        var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ring.name = "AnchorRing";
        ring.transform.SetParent(go.transform);
        ring.transform.localPosition = Vector3.down * 0.03f;
        ring.transform.localScale = new Vector3(0.04f, 0.002f, 0.04f);
        ring.GetComponent<Renderer>().material.color = new Color(0.5f, 0.5f, 0.5f);

        var line = go.AddComponent<LineRenderer>();
        line.startWidth = 0.003f;
        line.endWidth = 0.003f;
        line.positionCount = 2;
        line.SetPosition(0, Vector3.zero);
        line.SetPosition(1, Vector3.down * 0.15f);
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = Color.yellow;
        line.endColor = Color.yellow;
    }

    void CreateGenericHazardEffect(GameObject go, MineHazardLibrary.HazardMapping mapping)
    {
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = "GenericHazard";
        cube.transform.SetParent(go.transform);
        cube.transform.localPosition = Vector3.zero;
        cube.transform.localScale = Vector3.one * 0.1f;
        cube.GetComponent<Renderer>().material.color = mapping.TintColor;
    }

    void ShowDebugLabel(Pose pose, ObjectClassifier.ClassificationResult classification)
    {
        var debugGo = new GameObject($"DebugLabel_{classification.Type}");
        debugGo.transform.position = pose.position + Vector3.up * 0.2f;

        var textMesh = debugGo.AddComponent<TMPro.TextMeshPro>();
        textMesh.text = $"{classification.Type}\n{classification.Confidence:P0}";
        textMesh.fontSize = 2;
        textMesh.alignment = TMPro.TextAlignmentOptions.Center;
        textMesh.color = Color.white;

        Destroy(debugGo, 5f);
    }

    string GetCurrentScenarioId()
    {
        string moduleId = GameManager.Instance?.GetSelectedModuleId() ?? "";

        if (moduleId.Contains("fire")) return "fire-gas-leak";
        if (moduleId.Contains("gas")) return "gas-leak-only";
        if (moduleId.Contains("machinery")) return "structural-failure";
        if (moduleId.Contains("electrical")) return "electrical-fire";
        if (moduleId.Contains("height")) return "heights-fall";

        return "fire-gas-leak";
    }

    MineHazardLibrary.HazardMapping GetFallbackMapping(string scenarioId, ObjectClassifier.ObjectType type)
    {
        if (MineHazardLibrary.Instance == null) return null;

        var allMappings = MineHazardLibrary.Instance.GetAllMappings(scenarioId);
        if (allMappings.Count > 0) return allMappings[0];

        return null;
    }

    public void ClearAllHazards()
    {
        foreach (var kvp in _placedHazards)
        {
            if (kvp.Value != null) Destroy(kvp.Value);
        }
        _placedHazards.Clear();
        _classifiedObjects.Clear();
        Debug.Log("[ContextualPlacer] All hazards cleared");
    }

    public int GetPlacedCount() => _placedHazards.Count;
    public Dictionary<string, ObjectClassifier.ClassificationResult> GetClassifiedObjects() => _classifiedObjects;
}