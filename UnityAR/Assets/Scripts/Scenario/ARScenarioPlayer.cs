using UnityEngine;
using UnityEngine.XR.ARFoundation;
using Unity.XR.CoreUtils;
using System.Collections.Generic;

public class ARScenarioPlayer : MonoBehaviour
{
    public static ARScenarioPlayer Instance { get; private set; }

    [Header("AR")]
    public XROrigin arSessionOrigin;
    public ARTrackedImageManager trackedImageManager;

    [Header("Scenario Prefabs")]
    public GameObject firePrefab;
    public GameObject chemicalSpillPrefab;
    public GameObject structuralDamagePrefab;
    public GameObject evacuationRoutePrefab;
    public GameObject safetyEquipmentPrefab;

    [Header("Mine Detection")]
    [SerializeField] private bool _useContextualDetection = true;
    [SerializeField] private bool _spawnMineEnvironment = true;

    private List<ScenarioPointData> _activePoints = new();
    private List<GameObject> _spawnedObjects = new();
    private List<GameObject> _mineObjects = new();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void LoadScenario(List<ScenarioPointData> points)
    {
        ClearScenario();
        _activePoints = points;

        foreach (var point in points)
        {
            SpawnScenarioObject(point);
        }

        Debug.Log($"[Scenario] Loaded {points.Count} scenario points");
    }

    void SpawnScenarioObject(ScenarioPointData point)
    {
        GameObject prefab = GetPrefabForType(point.ScenarioType);
        if (prefab == null) return;

        var position = new Vector3(point.Position[0], point.Position[1], point.Position[2]);
        var rotation = new Quaternion(point.Rotation[0], point.Rotation[1], point.Rotation[2], point.Rotation[3]);
        var obj = Instantiate(prefab, position, rotation);
        obj.name = $"Scenario_{point.Id}_{point.ScenarioType}";
        _spawnedObjects.Add(obj);
    }

    GameObject GetPrefabForType(string type)
    {
        return type.ToLower() switch
        {
            "fire" => firePrefab,
            "chemical" => chemicalSpillPrefab,
            "structural" => structuralDamagePrefab,
            "evacuation" => evacuationRoutePrefab,
            "safety" => safetyEquipmentPrefab,
            _ => firePrefab
        };
    }

    public void HighlightPoint(string pointId)
    {
        foreach (var obj in _spawnedObjects)
        {
            if (obj.name.Contains(pointId))
            {
                var renderer = obj.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.EnableKeyword("_EMISSION");
                    renderer.material.SetColor("_EmissionColor", Color.yellow * 2f);
                }
                Debug.Log($"[Scenario] Highlighted: {pointId}");
                return;
            }
        }
    }

    public void CompletePoint(string pointId)
    {
        foreach (var obj in _spawnedObjects)
        {
            if (obj.name.Contains(pointId))
            {
                var renderer = obj.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.SetColor("_EmissionColor", Color.green * 2f);
                }
                LocalBlockchain.Instance.AddBlock("scenario_completed", PlayerPrefs.GetString("UserId"), pointId);
                return;
            }
        }
    }

    public void ClearScenario()
    {
        foreach (var obj in _spawnedObjects)
        {
            if (obj != null) Destroy(obj);
        }
        _spawnedObjects.Clear();
        _activePoints.Clear();

        ClearMineEnvironment();

        if (ContextualPlacer.Instance != null)
            ContextualPlacer.Instance.ClearAllHazards();
    }

    public void CompleteAction(string actionId)
    {
        Debug.Log($"[Scenario] Action completed: {actionId}");
        LocalBlockchain.Instance?.AddBlock("action_completed", PlayerPrefs.GetString("UserId"), actionId);

        foreach (var obj in _spawnedObjects)
        {
            if (obj.name.Contains(actionId))
            {
                var renderer = obj.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.EnableKeyword("_EMISSION");
                    renderer.material.SetColor("_EmissionColor", Color.green * 2f);
                }
                return;
            }
        }
    }

    public void ShowEvacuationArrows()
    {
        if (evacuationRoutePrefab == null) return;

        var arrow = Instantiate(evacuationRoutePrefab, Vector3.zero, Quaternion.identity);
        arrow.name = "EvacuationArrows";
        _spawnedObjects.Add(arrow);
        Debug.Log("[Scenario] Evacuation arrows displayed");
    }

    public void SpawnHazard(ScenarioConfig scenario)
    {
        ClearScenario();

        if (_useContextualDetection)
        {
            SpawnContextualHazard(scenario);
            return;
        }

        SpawnFallbackHazard(scenario);
    }

    void SpawnContextualHazard(ScenarioConfig scenario)
    {
        Debug.Log($"[Scenario] Contextual mode: waiting for surface detection to place {scenario.HazardType} hazards");

        if (_spawnMineEnvironment)
        {
            SpawnMineEnvironment(scenario);
        }

        if (ContextualPlacer.Instance != null)
        {
            Debug.Log("[Scenario] ContextualPlacer active — tap surfaces to place hazards");
        }
    }

    void SpawnMineEnvironment(ScenarioConfig scenario)
    {
        if (MineObjectFactory.Instance == null)
        {
            Debug.LogWarning("[Scenario] MineObjectFactory not found — using fallback");
            SpawnFallbackHazard(scenario);
            return;
        }

        Vector3 basePos = Vector3.zero;

        if (scenario.HazardType == "gas_leak" || scenario.HazardType == "gas")
        {
            SpawnGasLeakEnvironment(basePos);
        }
        else if (scenario.HazardType == "fire")
        {
            SpawnFireEnvironment(basePos);
        }
        else if (scenario.HazardType == "structural")
        {
            SpawnStructuralEnvironment(basePos);
        }
        else if (scenario.HazardType == "electrical")
        {
            SpawnElectricalEnvironment(basePos);
        }
        else
        {
            SpawnGasLeakEnvironment(basePos);
        }

        Debug.Log($"[Scenario] Mine environment spawned: {scenario.HazardType}");
    }

    void SpawnGasLeakEnvironment(Vector3 origin)
    {
        var floor = MineObjectFactory.Instance.CreateMineFloor(origin + new Vector3(0, -1.5f, 0), 6f);
        _mineObjects.Add(floor);

        var wall1 = MineObjectFactory.Instance.CreateMineWall(origin + new Vector3(0, 0, -2f), Quaternion.identity, 4f, 3f);
        _mineObjects.Add(wall1);

        var wall2 = MineObjectFactory.Instance.CreateMineWall(origin + new Vector3(-2f, 0, 0), Quaternion.Euler(0, 90, 0), 4f, 3f);
        _mineObjects.Add(wall2);

        var pipe1 = MineObjectFactory.Instance.CreateMethanePipe(origin + new Vector3(-1f, 0.5f, -1.5f), Quaternion.Euler(0, 0, 0), 3f);
        _mineObjects.Add(pipe1);

        var pipe2 = MineObjectFactory.Instance.CreateMethanePipe(origin + new Vector3(1f, 1f, -1.5f), Quaternion.Euler(0, 0, 90), 2f);
        _mineObjects.Add(pipe2);

        var detector = MineObjectFactory.Instance.CreateGasDetector(origin + new Vector3(0, 1.5f, -1.8f), Quaternion.identity);
        _mineObjects.Add(detector);

        var duct = MineObjectFactory.Instance.CreateVentilationDuct(origin + new Vector3(1.5f, 1.5f, 0), Quaternion.Euler(0, 0, 90), 3f);
        _mineObjects.Add(duct);
    }

    void SpawnFireEnvironment(Vector3 origin)
    {
        SpawnGasLeakEnvironment(origin);
    }

    void SpawnStructuralEnvironment(Vector3 origin)
    {
        var floor = MineObjectFactory.Instance.CreateMineFloor(origin + new Vector3(0, -1.5f, 0), 6f);
        _mineObjects.Add(floor);

        var beam1 = MineObjectFactory.Instance.CreateSupportBeam(origin + new Vector3(-1.5f, 0, -1.5f), Quaternion.identity, 3f);
        _mineObjects.Add(beam1);

        var beam2 = MineObjectFactory.Instance.CreateSupportBeam(origin + new Vector3(1.5f, 0, -1.5f), Quaternion.identity, 3f);
        _mineObjects.Add(beam2);

        var beam3 = MineObjectFactory.Instance.CreateSupportBeam(origin + new Vector3(0, 0, -1.5f), Quaternion.identity, 3f);
        _mineObjects.Add(beam3);

        var pipe = MineObjectFactory.Instance.CreateMethanePipe(origin + new Vector3(-1f, 1.2f, -1.5f), Quaternion.Euler(0, 0, 0), 3f);
        _mineObjects.Add(pipe);

        var wall = MineObjectFactory.Instance.CreateMineWall(origin + new Vector3(0, 0, -2f), Quaternion.identity, 5f, 3f);
        _mineObjects.Add(wall);
    }

    void SpawnElectricalEnvironment(Vector3 origin)
    {
        var floor = MineObjectFactory.Instance.CreateMineFloor(origin + new Vector3(0, -1.5f, 0), 6f);
        _mineObjects.Add(floor);

        var tray = MineObjectFactory.Instance.CreateCableTray(origin + new Vector3(0, 1.8f, -1.5f), Quaternion.Euler(0, 0, 0), 3f);
        _mineObjects.Add(tray);

        var wall = MineObjectFactory.Instance.CreateMineWall(origin + new Vector3(0, 0, -2f), Quaternion.identity, 4f, 3f);
        _mineObjects.Add(wall);

        var duct = MineObjectFactory.Instance.CreateVentilationDuct(origin + new Vector3(1.5f, 1.5f, 0), Quaternion.Euler(0, 0, 90), 3f);
        _mineObjects.Add(duct);
    }

    void SpawnFallbackHazard(ScenarioConfig scenario)
    {
        GameObject prefab = GetPrefabForType(scenario.HazardType);
        if (prefab == null)
        {
            prefab = firePrefab;
        }

        foreach (var zone in scenario.HazardZones)
        {
            var position = new Vector3(zone.Position[0], zone.Position[1], zone.Position[2]);
            var obj = Instantiate(prefab, position, Quaternion.identity);
            obj.name = $"Hazard_{zone.ZoneId}_{scenario.HazardType}";
            _spawnedObjects.Add(obj);
        }

        Debug.Log($"[Scenario] Fallback hazard spawned: {scenario.HazardType} with {scenario.HazardZones.Count} zones");
    }

    void ClearMineEnvironment()
    {
        foreach (var obj in _mineObjects)
        {
            if (obj != null) Destroy(obj);
        }
        _mineObjects.Clear();
    }

    public void OnTapToPlaceHazard(Vector2 screenPosition)
    {
        if (ContextualPlacer.Instance != null)
        {
            string hazardId = ContextualPlacer.Instance.PlaceHazardAtTap(screenPosition);
            if (hazardId != null)
            {
                Debug.Log($"[Scenario] Hazard placed via tap: {hazardId}");
            }
            else
            {
                Debug.Log("[Scenario] No surface detected at tap position");
            }
        }
    }

    public void ForcePlaceHazard(Vector3 position, ObjectClassifier.ObjectType type)
    {
        if (ContextualPlacer.Instance != null)
        {
            var pose = new Pose(position, Quaternion.identity);
            string hazardId = ContextualPlacer.Instance.ForcePlaceAtPose(pose, type);
            Debug.Log($"[Scenario] Forced hazard placement: {hazardId}");
        }
    }
}

[System.Serializable]
public class ScenarioPointData
{
    public string Id;
    public string ScenarioType;
    public float[] Position;
    public float[] Rotation;
    public string Description;
    public string RiskLevel;
}
