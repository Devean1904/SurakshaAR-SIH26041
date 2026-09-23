using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class TrainingModule
{
    public string ModuleId;
    public string Title;
    public string Description;
    public string SafetyDomain;
    public int DifficultyLevel;
    public float TimeLimitSeconds;
    public float PassThreshold;
    public List<ScenarioConfig> Scenarios;
    public List<AssessmentQuestion> Questions;
    public List<string> RequiredEquipment;
    public string LocalizedTitleKey;
    public string LocalizedDescKey;
}

[System.Serializable]
public class ScenarioConfig
{
    public string ScenarioId;
    public string ModuleId;
    public string HazardType;
    public string ScenarioName;
    public float InitialDangerLevel;
    public float EscalationIntervalSeconds;
    public int MaxEscalationLevel;
    public List<HazardZone> HazardZones;
    public List<RequiredAction> RequiredActions;
    public List<EscalationEvent> EscalationEvents;
    public SpawnConfig HazardSpawn;
    public EvacuationConfig Evacuation;
}

[System.Serializable]
public class HazardZone
{
    public string ZoneId;
    public string ZoneName;
    public float[] Position;
    public float Radius;
    public float DangerLevel;
    public string RiskColor;
}

[System.Serializable]
public class RequiredAction
{
    public string ActionId;
    public string ActionName;
    public string Description;
    public float[] InteractionPosition;
    public float InteractionRadius;
    public float TimeLimitSeconds;
    public int ScoreValue;
    public bool IsCritical;
    public string PrefabToSpawn;
    public string CompletionFeedback;
}

[System.Serializable]
public class EscalationEvent
{
    public int Level;
    public string LevelName;
    public float TimeThreshold;
    public float ScorePenalty;
    public string VisualEffect;
    public string AudioEffect;
    public string WarningText;
    public string LocalizedWarningKey;
    public float HazardGrowthMultiplier;
    public bool ShowEvacuationArrows;
    public bool TriggerAutoFail;
}

[System.Serializable]
public class SpawnConfig
{
    public string PrefabPath;
    public float[] Position;
    public float[] Rotation;
    public float[] Scale;
    public bool AttachToSurface;
    public bool FollowCamera;
}

[System.Serializable]
public class EvacuationConfig
{
    public bool HasEvacuationRoute;
    public string RouteType;
    public List<float[]> Waypoints;
    public float ArrowSpacing;
    public string ArrowPrefabPath;
    public float AnimationSpeed;
}

[System.Serializable]
public class AssessmentQuestion
{
    public string QuestionId;
    public string ModuleId;
    public string QuestionText;
    public string LocalizedTextKey;
    public string QuestionType;
    public List<string> Options;
    public List<string> LocalizedOptionKeys;
    public int CorrectAnswerIndex;
    public string Explanation;
    public string LocalizedExplanationKey;
    public int ScoreValue;
    public string Difficulty;
}

[System.Serializable]
public class CertificateData
{
    public string CertificateId;
    public string EmployeeId;
    public string EmployeeName;
    public string ModuleId;
    public string ModuleName;
    public int Score;
    public bool Passed;
    public string IssuedAt;
    public string Signature;
    public string BlockchainTx;
    public string QrPayload;
    public string QRCodeBase64;
}

[System.Serializable]
public class TrainingAttempt
{
    public string AttemptId;
    public string EmployeeId;
    public string ModuleId;
    public string ScenarioId;
    public float StartTime;
    public float EndTime;
    public float ResponseTime;
    public int EscalationLevelReached;
    public int ActionScore;
    public int QuestionScore;
    public int TotalScore;
    public bool Passed;
    public List<string> ActionsPerformed;
    public List<int> QuestionAnswers;
    public string BlockchainHash;
}

public static class TrainingDataStore
{
    public static List<TrainingModule> Modules = new List<TrainingModule>
    {
        new TrainingModule
        {
            ModuleId = "fire-safety-101",
            Title = "Fire & Explosion Response",
            Description = "Learn to identify fire hazards, use extinguishers, and evacuate safely",
            SafetyDomain = "Fire & Explosion",
            DifficultyLevel = 1,
            TimeLimitSeconds = 120f,
            PassThreshold = 70f,
            LocalizedTitleKey = "module_fire_title",
            LocalizedDescKey = "module_fire_desc",
            RequiredEquipment = new List<string> { "Fire Extinguisher", "Exit Map", "Alarm" },
            Scenarios = new List<ScenarioConfig>
            {
                new ScenarioConfig
                {
                    ScenarioId = "fire-scenario-01",
                    ModuleId = "fire-safety-101",
                    HazardType = "fire",
                    ScenarioName = "Workshop Fire",
                    InitialDangerLevel = 0.2f,
                    EscalationIntervalSeconds = 15f,
                    MaxEscalationLevel = 4,
                    HazardZones = new List<HazardZone>
                    {
                        new HazardZone
                        {
                            ZoneId = "fire-zone-1",
                            ZoneName = "Fire Origin",
                            Position = new float[] { 0, 0, 2 },
                            Radius = 1.5f,
                            DangerLevel = 0.8f,
                            RiskColor = "red"
                        }
                    },
                    RequiredActions = new List<RequiredAction>
                    {
                        new RequiredAction
                        {
                            ActionId = "raise-alarm",
                            ActionName = "Raise Alarm",
                            Description = "Pull the nearest fire alarm",
                            InteractionPosition = new float[] { 1, 1, 0 },
                            InteractionRadius = 0.5f,
                            TimeLimitSeconds = 15f,
                            ScoreValue = 20,
                            IsCritical = true,
                            CompletionFeedback = "Alarm raised! Help is on the way."
                        },
                        new RequiredAction
                        {
                            ActionId = "use-extinguisher",
                            ActionName = "Use Fire Extinguisher",
                            Description = "Grab extinguisher and spray at base of fire",
                            InteractionPosition = new float[] { -1, 0, 1 },
                            InteractionRadius = 0.5f,
                            TimeLimitSeconds = 30f,
                            ScoreValue = 40,
                            IsCritical = true,
                            CompletionFeedback = "Fire extinguished! Good work."
                        },
                        new RequiredAction
                        {
                            ActionId = "evacuate",
                            ActionName = "Evacuate Area",
                            Description = "Move to the designated safe zone",
                            InteractionPosition = new float[] { 0, 0, -5 },
                            InteractionRadius = 2f,
                            TimeLimitSeconds = 45f,
                            ScoreValue = 30,
                            IsCritical = true,
                            CompletionFeedback = "Successfully evacuated!"
                        }
                    },
                    EscalationEvents = new List<EscalationEvent>
                    {
                        new EscalationEvent
                        {
                            Level = 0,
                            LevelName = "Normal",
                            TimeThreshold = 0f,
                            ScorePenalty = 0f,
                            VisualEffect = "none",
                            WarningText = "Fire detected! Take action!",
                            LocalizedWarningKey = "escalation_fire_normal",
                            HazardGrowthMultiplier = 1f,
                            ShowEvacuationArrows = false,
                            TriggerAutoFail = false
                        },
                        new EscalationEvent
                        {
                            Level = 1,
                            LevelName = "Minor",
                            TimeThreshold = 15f,
                            ScorePenalty = 10f,
                            VisualEffect = "yellow_alert",
                            WarningText = "Fire spreading! Act quickly!",
                            LocalizedWarningKey = "escalation_fire_minor",
                            HazardGrowthMultiplier = 1.5f,
                            ShowEvacuationArrows = false,
                            TriggerAutoFail = false
                        },
                        new EscalationEvent
                        {
                            Level = 2,
                            LevelName = "Moderate",
                            TimeThreshold = 30f,
                            ScorePenalty = 25f,
                            VisualEffect = "orange_alert_flash",
                            WarningText = "Fire intensifying! Danger!",
                            LocalizedWarningKey = "escalation_fire_moderate",
                            HazardGrowthMultiplier = 2f,
                            ShowEvacuationArrows = true,
                            TriggerAutoFail = false
                        },
                        new EscalationEvent
                        {
                            Level = 3,
                            LevelName = "Severe",
                            TimeThreshold = 45f,
                            ScorePenalty = 50f,
                            VisualEffect = "red_alert_shake",
                            WarningText = "DANGER! Evacuate immediately!",
                            LocalizedWarningKey = "escalation_fire_severe",
                            HazardGrowthMultiplier = 3f,
                            ShowEvacuationArrows = true,
                            TriggerAutoFail = false
                        },
                        new EscalationEvent
                        {
                            Level = 4,
                            LevelName = "Critical",
                            TimeThreshold = 60f,
                            ScorePenalty = 100f,
                            VisualEffect = "red_screen_fail",
                            WarningText = "Scenario Failed - Fire too large",
                            LocalizedWarningKey = "escalation_fire_critical",
                            HazardGrowthMultiplier = 4f,
                            ShowEvacuationArrows = true,
                            TriggerAutoFail = true
                        }
                    }
                }
            },
            Questions = new List<AssessmentQuestion>
            {
                new AssessmentQuestion
                {
                    QuestionId = "fire-q1",
                    ModuleId = "fire-safety-101",
                    QuestionText = "What is the first thing to do when you discover a fire?",
                    LocalizedTextKey = "q_fire_1",
                    QuestionType = "multiple_choice",
                    Options = new List<string> { "Try to put it out yourself", "Raise the alarm and alert others", "Ignore it if it's small", "Open all windows" },
                    CorrectAnswerIndex = 1,
                    Explanation = "Always raise the alarm first to alert everyone in the area.",
                    ScoreValue = 20,
                    Difficulty = "easy"
                },
                new AssessmentQuestion
                {
                    QuestionId = "fire-q2",
                    ModuleId = "fire-safety-101",
                    QuestionText = "Where should you aim a fire extinguisher?",
                    LocalizedTextKey = "q_fire_2",
                    QuestionType = "multiple_choice",
                    Options = new List<string> { "At the flames", "At the base of the fire", "At the ceiling", "At the smoke" },
                    CorrectAnswerIndex = 1,
                    Explanation = "Aim at the base of the fire to cut off the fuel source.",
                    ScoreValue = 20,
                    Difficulty = "easy"
                },
                new AssessmentQuestion
                {
                    QuestionId = "fire-q3",
                    ModuleId = "fire-safety-101",
                    QuestionText = "What does PASS stand for in fire extinguisher use?",
                    LocalizedTextKey = "q_fire_3",
                    QuestionType = "multiple_choice",
                    Options = new List<string> { "Pull, Aim, Squeeze, Sweep", "Push, Aim, Spray, Sweep", "Pull, Alert, Squeeze, Spray", "Push, Alert, Spray, Stop" },
                    CorrectAnswerIndex = 0,
                    Explanation = "PASS: Pull the pin, Aim at the base, Squeeze the handle, Sweep side to side.",
                    ScoreValue = 20,
                    Difficulty = "medium"
                },
                new AssessmentQuestion
                {
                    QuestionId = "fire-q4",
                    ModuleId = "fire-safety-101",
                    QuestionText = "When should you evacuate during a fire?",
                    LocalizedTextKey = "q_fire_4",
                    QuestionType = "multiple_choice",
                    Options = new List<string> { "Only when told to", "When the fire is small", "When smoke fills the room or alarm sounds", "Never, stay and fight the fire" },
                    CorrectAnswerIndex = 2,
                    Explanation = "Evacuate when smoke fills the room or when the alarm sounds. Your life is the priority.",
                    ScoreValue = 20,
                    Difficulty = "easy"
                },
                new AssessmentQuestion
                {
                    QuestionId = "fire-q5",
                    ModuleId = "fire-safety-101",
                    QuestionText = "What should you do if your clothes catch fire?",
                    LocalizedTextKey = "q_fire_5",
                    QuestionType = "multiple_choice",
                    Options = new List<string> { "Run to get help", "Stop, Drop, and Roll", "Jump in water", "Use a fire extinguisher on yourself" },
                    CorrectAnswerIndex = 1,
                    Explanation = "Stop, Drop, and Roll to smother the flames. Running spreads the fire.",
                    ScoreValue = 20,
                    Difficulty = "easy"
                }
            }
        },
        new TrainingModule
        {
            ModuleId = "gas-leak-101",
            Title = "Gas Leak & Confined Space",
            Description = "Learn to detect gas leaks, select PPE, and follow buddy-system procedures",
            SafetyDomain = "Gas Leak & Confined Space",
            DifficultyLevel = 2,
            TimeLimitSeconds = 120f,
            PassThreshold = 75f,
            LocalizedTitleKey = "module_gas_title",
            LocalizedDescKey = "module_gas_desc",
            RequiredEquipment = new List<string> { "Gas Detector", "PPE Kit", "Ventilation Fan" },
            Scenarios = new List<ScenarioConfig>
            {
                new ScenarioConfig
                {
                    ScenarioId = "gas-scenario-01",
                    ModuleId = "gas-leak-101",
                    HazardType = "gas_leak",
                    ScenarioName = "Methane Leak",
                    InitialDangerLevel = 0.15f,
                    EscalationIntervalSeconds = 15f,
                    MaxEscalationLevel = 4,
                    HazardZones = new List<HazardZone>
                    {
                        new HazardZone
                        {
                            ZoneId = "gas-zone-1",
                            ZoneName = "Leak Source",
                            Position = new float[] { 0, 0, 2 },
                            Radius = 2f,
                            DangerLevel = 0.9f,
                            RiskColor = "green"
                        }
                    },
                    RequiredActions = new List<RequiredAction>
                    {
                        new RequiredAction
                        {
                            ActionId = "detect-gas",
                            ActionName = "Detect Gas Level",
                            Description = "Use gas detector to identify hazard",
                            InteractionPosition = new float[] { 0, 1, 0 },
                            InteractionRadius = 0.5f,
                            TimeLimitSeconds = 15f,
                            ScoreValue = 20,
                            IsCritical = true,
                            CompletionFeedback = "Gas detected! Methane at dangerous levels."
                        },
                        new RequiredAction
                        {
                            ActionId = "select-ppe",
                            ActionName = "Select PPE",
                            Description = "Put on breathing apparatus and protective gear",
                            InteractionPosition = new float[] { -1, 0, 0 },
                            InteractionRadius = 0.5f,
                            TimeLimitSeconds = 20f,
                            ScoreValue = 30,
                            IsCritical = true,
                            CompletionFeedback = "PPE equipped. You are protected."
                        },
                        new RequiredAction
                        {
                            ActionId = "activate-ventilation",
                            ActionName = "Activate Ventilation",
                            Description = "Turn on ventilation to reduce gas concentration",
                            InteractionPosition = new float[] { 1, 2, 0 },
                            InteractionRadius = 0.5f,
                            TimeLimitSeconds = 25f,
                            ScoreValue = 25,
                            IsCritical = false,
                            CompletionFeedback = "Ventilation active. Gas levels decreasing."
                        },
                        new RequiredAction
                        {
                            ActionId = "evacuate-zone",
                            ActionName = "Evacuate Zone",
                            Description = "Move to the safe area outside the danger zone",
                            InteractionPosition = new float[] { 0, 0, -5 },
                            InteractionRadius = 2f,
                            TimeLimitSeconds = 40f,
                            ScoreValue = 25,
                            IsCritical = true,
                            CompletionFeedback = "Successfully evacuated the danger zone!"
                        }
                    },
                    EscalationEvents = new List<EscalationEvent>
                    {
                        new EscalationEvent
                        {
                            Level = 0,
                            LevelName = "Normal",
                            TimeThreshold = 0f,
                            ScorePenalty = 0f,
                            WarningText = "Gas detected! Level: 1.5%",
                            LocalizedWarningKey = "escalation_gas_normal",
                            HazardGrowthMultiplier = 1f,
                            TriggerAutoFail = false
                        },
                        new EscalationEvent
                        {
                            Level = 1,
                            LevelName = "Minor",
                            TimeThreshold = 15f,
                            ScorePenalty = 10f,
                            WarningText = "Level rising! 2.8% - Act now!",
                            LocalizedWarningKey = "escalation_gas_minor",
                            HazardGrowthMultiplier = 1.5f,
                            TriggerAutoFail = false
                        },
                        new EscalationEvent
                        {
                            Level = 2,
                            LevelName = "Moderate",
                            TimeThreshold = 30f,
                            ScorePenalty = 25f,
                            WarningText = "Level: 4.2% - DANGER ZONE!",
                            LocalizedWarningKey = "escalation_gas_moderate",
                            HazardGrowthMultiplier = 2f,
                            ShowEvacuationArrows = true,
                            TriggerAutoFail = false
                        },
                        new EscalationEvent
                        {
                            Level = 3,
                            LevelName = "Severe",
                            TimeThreshold = 45f,
                            ScorePenalty = 50f,
                            WarningText = "CRITICAL LEVEL: 6.1% - EVACUATE!",
                            LocalizedWarningKey = "escalation_gas_severe",
                            HazardGrowthMultiplier = 3f,
                            ShowEvacuationArrows = true,
                            TriggerAutoFail = false
                        },
                        new EscalationEvent
                        {
                            Level = 4,
                            LevelName = "Critical",
                            TimeThreshold = 60f,
                            ScorePenalty = 100f,
                            WarningText = "Scenario Failed - Evacuation required",
                            LocalizedWarningKey = "escalation_gas_critical",
                            HazardGrowthMultiplier = 4f,
                            ShowEvacuationArrows = true,
                            TriggerAutoFail = true
                        }
                    }
                }
            },
            Questions = new List<AssessmentQuestion>
            {
                new AssessmentQuestion
                {
                    QuestionId = "gas-q1",
                    ModuleId = "gas-leak-101",
                    QuestionText = "What is the first sign of a gas leak?",
                    QuestionType = "multiple_choice",
                    Options = new List<string> { "Visible flames", "Smell of rotten eggs", "Loud noise", "Temperature drop" },
                    CorrectAnswerIndex = 1,
                    Explanation = "Natural gas is odorless, but mercaptan is added to give it a rotten egg smell for detection.",
                    ScoreValue = 20,
                    Difficulty = "easy"
                },
                new AssessmentQuestion
                {
                    QuestionId = "gas-q2",
                    ModuleId = "gas-leak-101",
                    QuestionText = "What should you do FIRST when you suspect a gas leak?",
                    QuestionType = "multiple_choice",
                    Options = new List<string> { "Use your phone to call for help", "Light a match to see better", "Evacuate the area immediately", "Turn on electrical switches" },
                    CorrectAnswerIndex = 2,
                    Explanation = "Evacuate immediately. Do not use any electrical devices as they can spark an explosion.",
                    ScoreValue = 20,
                    Difficulty = "easy"
                },
                new AssessmentQuestion
                {
                    QuestionId = "gas-q3",
                    ModuleId = "gas-leak-101",
                    QuestionText = "What does LEL stand for in gas detection?",
                    QuestionType = "multiple_choice",
                    Options = new List<string> { "Low Energy Level", "Lower Explosive Limit", "Limited Exit Location", "Leak Exposure Level" },
                    CorrectAnswerIndex = 1,
                    Explanation = "LEL is the Lower Explosive Limit - the minimum concentration of gas that can ignite.",
                    ScoreValue = 20,
                    Difficulty = "medium"
                },
                new AssessmentQuestion
                {
                    QuestionId = "gas-q4",
                    ModuleId = "gas-leak-101",
                    QuestionText = "In confined spaces, what is the buddy system?",
                    QuestionType = "multiple_choice",
                    Options = new List<string> { "Working in pairs only", "Having someone outside monitor you while you work inside", "Using two gas detectors", "Having two exit routes" },
                    CorrectAnswerIndex = 1,
                    Explanation = "The buddy system means someone stays outside to monitor and call for help if needed.",
                    ScoreValue = 20,
                    Difficulty = "medium"
                },
                new AssessmentQuestion
                {
                    QuestionId = "gas-q5",
                    ModuleId = "gas-leak-101",
                    QuestionText = "At what methane concentration should you evacuate?",
                    QuestionType = "multiple_choice",
                    Options = new List<string> { "1% LEL", "5% LEL", "10% LEL", "Any detectable level" },
                    CorrectAnswerIndex = 3,
                    Explanation = "Any detectable level of methane in a confined space warrants evacuation and investigation.",
                    ScoreValue = 20,
                    Difficulty = "hard"
                }
            }
        }
    };

    public static TrainingModule GetModule(string moduleId)
    {
        return Modules.Find(m => m.ModuleId == moduleId);
    }

    public static ScenarioConfig GetScenario(string moduleId, string scenarioId)
    {
        var module = GetModule(moduleId);
        return module?.Scenarios.Find(s => s.ScenarioId == scenarioId);
    }

    public static List<AssessmentQuestion> GetRandomQuestions(string moduleId, int count)
    {
        var module = GetModule(moduleId);
        if (module == null) return new List<AssessmentQuestion>();

        var shuffled = new List<AssessmentQuestion>(module.Questions);
        for (int i = shuffled.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
        }
        return shuffled.GetRange(0, Mathf.Min(count, shuffled.Count));
    }
}
