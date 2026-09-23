using UnityEngine;
using System.Collections.Generic;

public class MineHazardLibrary : MonoBehaviour
{
    public static MineHazardLibrary Instance { get; private set; }

    [System.Serializable]
    public class HazardMapping
    {
        public string ScenarioId;
        public ObjectClassifier.ObjectType ObjectType;
        public string HazardEffect;
        public string PrefabName;
        public Color TintColor;
        public float Scale;
        public bool Animate;
        public string Description;
    }

    [System.Serializable]
    public class ScenarioHazardSet
    {
        public string ScenarioId;
        public string DisplayName;
        public string Description;
        public List<HazardMapping> Mappings = new();
    }

    private Dictionary<string, ScenarioHazardSet> _hazardSets = new();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        InitializeHazardLibrary();
    }

    void InitializeHazardLibrary()
    {
        CreateFireOnGasLeakScenario();
        CreateGasLeakScenario();
        CreateStructuralFailureScenario();
        CreateElectricalFireScenario();
        CreateHeightsFallScenario();
    }

    void CreateFireOnGasLeakScenario()
    {
        var set = new ScenarioHazardSet
        {
            ScenarioId = "fire-gas-leak",
            DisplayName = "Fire from Gas Leak",
            Description = "Methane leak from pipe ignites — fire spreads along pipe route"
        };

        set.Mappings.Add(new HazardMapping
        {
            ScenarioId = "fire-gas-leak",
            ObjectType = ObjectClassifier.ObjectType.Pipe,
            HazardEffect = "gas_leak_fire",
            PrefabName = "GasLeakFirePrefab",
            TintColor = new Color(1f, 0.3f, 0f, 0.8f),
            Scale = 1.2f,
            Animate = true,
            Description = "Fire erupts from leaking pipe joint"
        });

        set.Mappings.Add(new HazardMapping
        {
            ScenarioId = "fire-gas-leak",
            ObjectType = ObjectClassifier.ObjectType.VentilationDuct,
            HazardEffect = "fire_spread",
            PrefabName = "FireSpreadPrefab",
            TintColor = new Color(1f, 0.5f, 0f, 0.6f),
            Scale = 1.0f,
            Animate = true,
            Description = "Fire travels through ventilation duct"
        });

        set.Mappings.Add(new HazardMapping
        {
            ScenarioId = "fire-gas-leak",
            ObjectType = ObjectClassifier.ObjectType.Wall,
            HazardEffect = "smoke_wall",
            PrefabName = "SmokeWallPrefab",
            TintColor = new Color(0.3f, 0.3f, 0.3f, 0.4f),
            Scale = 1.5f,
            Animate = true,
            Description = "Smoke rises along wall surface"
        });

        set.Mappings.Add(new HazardMapping
        {
            ScenarioId = "fire-gas-leak",
            ObjectType = ObjectClassifier.ObjectType.Ceiling,
            HazardEffect = "smoke_ceiling",
            PrefabName = "SmokeCeilingPrefab",
            TintColor = new Color(0.4f, 0.4f, 0.4f, 0.5f),
            Scale = 2.0f,
            Animate = true,
            Description = "Hot smoke collects at ceiling"
        });

        set.Mappings.Add(new HazardMapping
        {
            ScenarioId = "fire-gas-leak",
            ObjectType = ObjectClassifier.ObjectType.Floor,
            HazardEffect = "fire_on_floor",
            PrefabName = "FireFloorPrefab",
            TintColor = new Color(1f, 0.2f, 0f, 0.7f),
            Scale = 0.8f,
            Animate = true,
            Description = "Pooling fire on floor from dripping gas"
        });

        set.Mappings.Add(new HazardMapping
        {
            ScenarioId = "fire-gas-leak",
            ObjectType = ObjectClassifier.ObjectType.SupportBeam,
            HazardEffect = "heat_damage",
            PrefabName = "HeatDamagePrefab",
            TintColor = new Color(0.8f, 0.2f, 0f, 0.5f),
            Scale = 1.0f,
            Animate = false,
            Description = "Support beam weakens from heat exposure"
        });

        _hazardSets["fire-gas-leak"] = set;
    }

    void CreateGasLeakScenario()
    {
        var set = new ScenarioHazardSet
        {
            ScenarioId = "gas-leak-only",
            DisplayName = "Methane Gas Leak",
            Description = "Undetected methane leak from pipe — gas accumulates in confined space"
        };

        set.Mappings.Add(new HazardMapping
        {
            ScenarioId = "gas-leak-only",
            ObjectType = ObjectClassifier.ObjectType.Pipe,
            HazardEffect = "gas_leak_source",
            PrefabName = "GasLeakSourcePrefab",
            TintColor = new Color(0.2f, 1f, 0.2f, 0.6f),
            Scale = 1.0f,
            Animate = true,
            Description = "Methane escaping from pipe joint"
        });

        set.Mappings.Add(new HazardMapping
        {
            ScenarioId = "gas-leak-only",
            ObjectType = ObjectClassifier.ObjectType.Floor,
            HazardEffect = "gas_pool",
            PrefabName = "GasPoolPrefab",
            TintColor = new Color(0.3f, 1f, 0.3f, 0.3f),
            Scale = 2.0f,
            Animate = true,
            Description = "Gas pooling at floor level"
        });

        set.Mappings.Add(new HazardMapping
        {
            ScenarioId = "gas-leak-only",
            ObjectType = ObjectClassifier.ObjectType.Ceiling,
            HazardEffect = "gas_accumulation",
            PrefabName = "GasAccumulationPrefab",
            TintColor = new Color(0.2f, 1f, 0.2f, 0.4f),
            Scale = 1.5f,
            Animate = true,
            Description = "Gas accumulating near ceiling"
        });

        set.Mappings.Add(new HazardMapping
        {
            ScenarioId = "gas-leak-only",
            ObjectType = ObjectClassifier.ObjectType.VentilationDuct,
            HazardEffect = "gas_in_duct",
            PrefabName = "GasDuctPrefab",
            TintColor = new Color(0.3f, 1f, 0.3f, 0.5f),
            Scale = 1.0f,
            Animate = true,
            Description = "Gas entering ventilation system"
        });

        set.Mappings.Add(new HazardMapping
        {
            ScenarioId = "gas-leak-only",
            ObjectType = ObjectClassifier.ObjectType.Wall,
            HazardEffect = "gas_indicator",
            PrefabName = "GasIndicatorPrefab",
            TintColor = new Color(1f, 1f, 0f, 0.6f),
            Scale = 0.5f,
            Animate = true,
            Description = "Gas concentration warning indicator"
        });

        _hazardSets["gas-leak-only"] = set;
    }

    void CreateStructuralFailureScenario()
    {
        var set = new ScenarioHazardSet
        {
            ScenarioId = "structural-failure",
            DisplayName = "Support Structure Failure",
            Description = "Weakened support beam fails — roof collapse and landslide"
        };

        set.Mappings.Add(new HazardMapping
        {
            ScenarioId = "structural-failure",
            ObjectType = ObjectClassifier.ObjectType.SupportBeam,
            HazardEffect = "beam_crack",
            PrefabName = "BeamCrackPrefab",
            TintColor = new Color(1f, 0.5f, 0f, 0.7f),
            Scale = 1.0f,
            Animate = true,
            Description = "Visible cracks appearing on support beam"
        });

        set.Mappings.Add(new HazardMapping
        {
            ScenarioId = "structural-failure",
            ObjectType = ObjectClassifier.ObjectType.Ceiling,
            HazardEffect = "roof_collapse",
            PrefabName = "RoofCollapsePrefab",
            TintColor = new Color(0.5f, 0.3f, 0.1f, 0.6f),
            Scale = 3.0f,
            Animate = true,
            Description = "Ceiling fragments falling from roof"
        });

        set.Mappings.Add(new HazardMapping
        {
            ScenarioId = "structural-failure",
            ObjectType = ObjectClassifier.ObjectType.Floor,
            HazardEffect = "rubble_pile",
            PrefabName = "RubblePilePrefab",
            TintColor = new Color(0.4f, 0.3f, 0.2f, 0.8f),
            Scale = 1.5f,
            Animate = false,
            Description = "Fallen debris accumulating on floor"
        });

        set.Mappings.Add(new HazardMapping
        {
            ScenarioId = "structural-failure",
            ObjectType = ObjectClassifier.ObjectType.Wall,
            HazardEffect = "wall_crack",
            PrefabName = "WallCrackPrefab",
            TintColor = new Color(0.6f, 0.4f, 0.2f, 0.7f),
            Scale = 1.0f,
            Animate = true,
            Description = "Stress cracks propagating through wall"
        });

        set.Mappings.Add(new HazardMapping
        {
            ScenarioId = "structural-failure",
            ObjectType = ObjectClassifier.ObjectType.Pipe,
            HazardEffect = "pipe_damage",
            PrefabName = "PipeDamagePrefab",
            TintColor = new Color(0.8f, 0.4f, 0f, 0.6f),
            Scale = 1.0f,
            Animate = true,
            Description = "Pipes damaged by falling debris"
        });

        _hazardSets["structural-failure"] = set;
    }

    void CreateElectricalFireScenario()
    {
        var set = new ScenarioHazardSet
        {
            ScenarioId = "electrical-fire",
            DisplayName = "Electrical Fire",
            Description = "Short circuit in cable tray ignites surrounding materials"
        };

        set.Mappings.Add(new HazardMapping
        {
            ScenarioId = "electrical-fire",
            ObjectType = ObjectClassifier.ObjectType.CableTray,
            HazardEffect = "spark_origin",
            PrefabName = "SparkOriginPrefab",
            TintColor = new Color(1f, 1f, 0f, 0.9f),
            Scale = 0.8f,
            Animate = true,
            Description = "Electrical spark at cable tray"
        });

        set.Mappings.Add(new HazardMapping
        {
            ScenarioId = "electrical-fire",
            ObjectType = ObjectClassifier.ObjectType.Wall,
            HazardEffect = "electrical_fire",
            PrefabName = "ElectricalFirePrefab",
            TintColor = new Color(1f, 0.6f, 0f, 0.7f),
            Scale = 1.2f,
            Animate = true,
            Description = "Fire spreading from electrical panel"
        });

        set.Mappings.Add(new HazardMapping
        {
            ScenarioId = "electrical-fire",
            ObjectType = ObjectClassifier.ObjectType.Floor,
            HazardEffect = "burn_marks",
            PrefabName = "BurnMarksPrefab",
            TintColor = new Color(0.2f, 0.2f, 0.2f, 0.5f),
            Scale = 1.0f,
            Animate = false,
            Description = "Burn marks on floor from electrical fault"
        });

        _hazardSets["electrical-fire"] = set;
    }

    void CreateHeightsFallScenario()
    {
        var set = new ScenarioHazardSet
        {
            ScenarioId = "heights-fall",
            DisplayName = "Heights & Fall Hazard",
            Description = "Unprotected edge at height — fall risk zone"
        };

        set.Mappings.Add(new HazardMapping
        {
            ScenarioId = "heights-fall",
            ObjectType = ObjectClassifier.ObjectType.Floor,
            HazardEffect = "fall_zone",
            PrefabName = "FallZonePrefab",
            TintColor = new Color(1f, 0f, 0f, 0.3f),
            Scale = 1.0f,
            Animate = true,
            Description = "Danger zone near unprotected edge"
        });

        set.Mappings.Add(new HazardMapping
        {
            ScenarioId = "heights-fall",
            ObjectType = ObjectClassifier.ObjectType.SupportBeam,
            HazardEffect = "anchor_point",
            PrefabName = "AnchorPointPrefab",
            TintColor = new Color(0f, 1f, 0f, 0.7f),
            Scale = 0.5f,
            Animate = false,
            Description = "Safety harness anchor point"
        });

        _hazardSets["heights-fall"] = set;
    }

    public ScenarioHazardSet GetHazardSet(string scenarioId)
    {
        _hazardSets.TryGetValue(scenarioId, out var set);
        return set;
    }

    public HazardMapping GetMapping(string scenarioId, ObjectClassifier.ObjectType objectType)
    {
        if (!_hazardSets.TryGetValue(scenarioId, out var set)) return null;

        foreach (var mapping in set.Mappings)
        {
            if (mapping.ObjectType == objectType)
                return mapping;
        }
        return null;
    }

    public List<HazardMapping> GetAllMappings(string scenarioId)
    {
        if (_hazardSets.TryGetValue(scenarioId, out var set))
            return set.Mappings;
        return new List<HazardMapping>();
    }

    public List<string> GetAllScenarioIds()
    {
        return new List<string>(_hazardSets.Keys);
    }

    public ScenarioHazardSet GetSetForCurrentTraining()
    {
        string moduleId = GameManager.Instance?.GetSelectedModuleId() ?? "";

        if (moduleId.Contains("fire"))
            return GetHazardSet("fire-gas-leak");
        if (moduleId.Contains("gas"))
            return GetHazardSet("gas-leak-only");
        if (moduleId.Contains("structural") || moduleId.Contains("machinery"))
            return GetHazardSet("structural-failure");
        if (moduleId.Contains("electrical"))
            return GetHazardSet("electrical-fire");
        if (moduleId.Contains("height"))
            return GetHazardSet("heights-fall");

        return GetHazardSet("fire-gas-leak");
    }
}