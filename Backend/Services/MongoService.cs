using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using Microsoft.Extensions.Options;
using SurakshaAR.Backend.Config;
using SurakshaAR.Shared;

namespace SurakshaAR.Backend.Services;

public class MongoService
{
    private readonly IMongoDatabase _db;

    static MongoService()
    {
        var pack = new ConventionPack { new CamelCaseElementNameConvention() };
        ConventionRegistry.Register("CamelCase", pack, type => true);
    }

    public MongoService(IOptions<MongoConfig> config)
    {
        var client = new MongoClient(config.Value.ConnectionString);
        _db = client.GetDatabase(config.Value.DatabaseName);
    }

    public IMongoCollection<T> Collection<T>(string name) => _db.GetCollection<T>(name);

    public IMongoCollection<BsonDocument> RawCollection(string name)
        => _db.GetCollection<BsonDocument>(name);

    public async Task InitializeAsync()
    {
        var users = Collection<User>("users");
        var indexKeys = Builders<User>.IndexKeys.Ascending(u => u.UserId);
        await users.Indexes.CreateOneAsync(new CreateIndexModel<User>(indexKeys));

        var userCompanyKeys = Builders<User>.IndexKeys.Ascending(u => u.CompanyId);
        await users.Indexes.CreateOneAsync(new CreateIndexModel<User>(userCompanyKeys));

        var companies = Collection<Company>("companies");
        var companyKeys = Builders<Company>.IndexKeys.Ascending(c => c.Id);
        await companies.Indexes.CreateOneAsync(new CreateIndexModel<Company>(companyKeys));

        var certs = Collection<CertificateModel>("certificates");
        var certKeys = Builders<CertificateModel>.IndexKeys.Ascending(c => c.CertificateId);
        await certs.Indexes.CreateOneAsync(new CreateIndexModel<CertificateModel>(certKeys));

        var attempts = Collection<TrainingAttemptModel>("training_attempts");
        var attemptKeys = Builders<TrainingAttemptModel>.IndexKeys.Ascending(a => a.AttemptId);
        await attempts.Indexes.CreateOneAsync(new CreateIndexModel<TrainingAttemptModel>(attemptKeys));

        var assessments = Collection<AssessmentModel>("assessments");
        var assessKeys = Builders<AssessmentModel>.IndexKeys.Ascending(a => a.AssessmentId);
        await assessments.Indexes.CreateOneAsync(new CreateIndexModel<AssessmentModel>(assessKeys));

        var modules = Collection<TrainingModuleModel>("training_modules");
        var moduleKeys = Builders<TrainingModuleModel>.IndexKeys.Ascending(m => m.ModuleId);
        await modules.Indexes.CreateOneAsync(new CreateIndexModel<TrainingModuleModel>(moduleKeys));

        await SeedTrainingModulesAsync();
    }

    async Task SeedTrainingModulesAsync()
    {
        var modules = Collection<TrainingModuleModel>("training_modules");

        async Task EnsureModuleAsync(string moduleId, TrainingModuleModel module)
        {
            var filter = Builders<TrainingModuleModel>.Filter.Eq(m => m.ModuleId, moduleId);
            var count = await modules.CountDocumentsAsync(filter);
            if (count > 0) return;
            await modules.InsertOneAsync(module);
        }

        await EnsureModuleAsync("fire-safety-101", BuildFireModule());
        await EnsureModuleAsync("gas-leak-101", BuildGasModule());
        Console.WriteLine("[MongoDB] Ensured training modules seeded (no duplicates)");
    }

    static TrainingModuleModel BuildFireModule()
    {
        return new TrainingModuleModel
        {
            ModuleId = "fire-safety-101",
            Title = "Fire & Explosion Response",
            Description = "Learn fire hazard identification, extinguisher use, and safe evacuation",
            SafetyDomain = "Fire & Explosion",
            DifficultyLevel = 1,
            TimeLimitSeconds = 300,
            PassThreshold = 70,
            RequiredEquipment = new List<string> { "ABC Dry Chemical Extinguisher", "Fire Blanket", "Safety Goggles", "Heat-Resistant Gloves" },
            Scenarios = new List<ScenarioConfigModel>
            {
                new ScenarioConfigModel
                {
                    ScenarioId = "fire-1",
                    ModuleId = "fire-safety-101",
                    HazardType = "Workshop Fire",
                    ScenarioName = "Workshop Fire Response",
                    InitialDangerLevel = 0.1f,
                    EscalationIntervalSeconds = 15f,
                    MaxEscalationLevel = 4,
                    HazardZones = new List<HazardZoneModel>
                    {
                        new HazardZoneModel { ZoneId = "fire-zone-center", ZoneName = "Ignition Point", RiskColor = "#FF4500", Position = new float[] {0,0,0}, Radius = 2.5f, DangerLevel = 0.9f },
                        new HazardZoneModel { ZoneId = "fire-zone-perimeter", ZoneName = "Heat Perimeter", RiskColor = "#FF8C00", Position = new float[] {0,0,0}, Radius = 5f, DangerLevel = 0.5f },
                        new HazardZoneModel { ZoneId = "fire-zone-exit", ZoneName = "Exit Corridor", RiskColor = "#FFD700", Position = new float[] {8,0,0}, Radius = 3f, DangerLevel = 0.3f }
                    },
                    RequiredActions = new List<RequiredActionModel>
                    {
                        new RequiredActionModel { ActionId = "activate-alarm", ActionName = "Activate Alarm", Description = "Pull the nearest fire alarm lever.", CompletionFeedback = "Fire alarm activated. All personnel notified.", InteractionRadius = 2f, TimeLimitSeconds = 15f, ScoreValue = 20, IsCritical = true },
                        new RequiredActionModel { ActionId = "use-extinguisher", ActionName = "Use Extinguisher", Description = "Select the correct ABC extinguisher and discharge at the base of the fire.", CompletionFeedback = "Extinguisher discharged. Fire size reduced.", InteractionRadius = 3f, TimeLimitSeconds = 30f, ScoreValue = 30, IsCritical = true },
                        new RequiredActionModel { ActionId = "evacuate", ActionName = "Evacuate", Description = "Lead all personnel through the nearest safe exit.", CompletionFeedback = "All personnel evacuated safely.", InteractionRadius = 4f, TimeLimitSeconds = 45f, ScoreValue = 50, IsCritical = true }
                    },
                    EscalationEvents = new List<EscalationEventModel>
                    {
                        new EscalationEventModel { Level = 0, LevelName = "Normal", TimeThreshold = 0f, ScorePenalty = 0f, VisualEffect = "none", AudioEffect = "ambient-machinery", WarningText = "All clear. Proceed with scenario.", TriggerAutoFail = false, HazardGrowthMultiplier = 1f },
                        new EscalationEventModel { Level = 1, LevelName = "Minor", TimeThreshold = 15f, ScorePenalty = 0.10f, VisualEffect = "light-smoke", AudioEffect = "distant-alarm", WarningText = "Smoke detected. Investigate the source.", TriggerAutoFail = false, HazardGrowthMultiplier = 1.2f },
                        new EscalationEventModel { Level = 2, LevelName = "Moderate", TimeThreshold = 30f, ScorePenalty = 0.25f, VisualEffect = "heavy-smoke-sparks", AudioEffect = "crackling-fire", WarningText = "Fire is spreading. Use an extinguisher.", ShowEvacuationArrows = true, TriggerAutoFail = false, HazardGrowthMultiplier = 1.5f },
                        new EscalationEventModel { Level = 3, LevelName = "Severe", TimeThreshold = 45f, ScorePenalty = 0.50f, VisualEffect = "full-flames-heat-distortion", AudioEffect = "roaring-fire", WarningText = "Danger! Fire has spread to fuel storage. Evacuate!", ShowEvacuationArrows = true, TriggerAutoFail = false, HazardGrowthMultiplier = 2f },
                        new EscalationEventModel { Level = 4, LevelName = "Critical", TimeThreshold = 60f, ScorePenalty = 1f, VisualEffect = "explosion-screen-shake-blackout", AudioEffect = "explosion-siren", WarningText = "CRITICAL: Explosion risk. Scenario failed.", ShowEvacuationArrows = true, TriggerAutoFail = true, HazardGrowthMultiplier = 3f }
                    }
                }
            },
            Questions = new List<AssessmentQuestionModel>
            {
                new AssessmentQuestionModel { QuestionId = "fire-q1", ModuleId = "fire-safety-101", QuestionText = "What is the first action you should take when a fire is detected?", Options = new List<string> { "Attempt to extinguish immediately", "Activate the nearest fire alarm", "Call the control room", "Open all windows" }, CorrectAnswerIndex = 1, Explanation = "Activate the fire alarm first to alert all personnel.", ScoreValue = 20, Difficulty = "easy" },
                new AssessmentQuestionModel { QuestionId = "fire-q2", ModuleId = "fire-safety-101", QuestionText = "Which extinguisher is correct for electrical fires?", Options = new List<string> { "Water-type", "Foam-type", "CO2 or dry chemical (ABC)", "Sand bucket only" }, CorrectAnswerIndex = 2, Explanation = "CO2 or ABC extinguishers are non-conductive.", ScoreValue = 20, Difficulty = "medium" },
                new AssessmentQuestionModel { QuestionId = "fire-q3", ModuleId = "fire-safety-101", QuestionText = "What does PASS stand for in extinguisher use?", Options = new List<string> { "Pull, Aim, Squeeze, Sweep", "Position, Activate, Secure, Suppress", "Prepare, Aim, Spray, Stop", "Pull, Alert, Spray, Safety" }, CorrectAnswerIndex = 0, Explanation = "PASS: Pull pin, Aim at base, Squeeze handle, Sweep.", ScoreValue = 20, Difficulty = "easy" },
                new AssessmentQuestionModel { QuestionId = "fire-q4", ModuleId = "fire-safety-101", QuestionText = "At what level should evacuation become the top priority?", Options = new List<string> { "Normal (level 0)", "Minor (level 1)", "Moderate (level 2)", "Severe (level 3) or when instructed" }, CorrectAnswerIndex = 3, Explanation = "At severe escalation, evacuate immediately.", ScoreValue = 20, Difficulty = "medium" },
                new AssessmentQuestionModel { QuestionId = "fire-q5", ModuleId = "fire-safety-101", QuestionText = "Where should personnel assemble after evacuating?", Options = new List<string> { "Inside the nearest vehicle", "At the designated muster point", "At the workshop entrance", "In the locker room" }, CorrectAnswerIndex = 1, Explanation = "The muster point is outside all hazard zones.", ScoreValue = 20, Difficulty = "easy" }
            }
        };
    }

    static TrainingModuleModel BuildGasModule()
    {
        return new TrainingModuleModel
        {
            ModuleId = "gas-leak-101",
            Title = "Gas Leak & Confined Space",
            Description = "Learn gas leak detection, PPE selection, and safe evacuation from confined spaces",
            SafetyDomain = "Gas Safety",
            DifficultyLevel = 2,
            TimeLimitSeconds = 300,
            PassThreshold = 75,
            RequiredEquipment = new List<string> { "4-Gas Detector", "SCBA", "Chemical-Resistant PPE Suit", "Ventilation Fan" },
            Scenarios = new List<ScenarioConfigModel>
            {
                new ScenarioConfigModel
                {
                    ScenarioId = "gas-1",
                    ModuleId = "gas-leak-101",
                    HazardType = "Methane Leak",
                    ScenarioName = "Methane Detection Response",
                    InitialDangerLevel = 0.15f,
                    EscalationIntervalSeconds = 15f,
                    MaxEscalationLevel = 4,
                    HazardZones = new List<HazardZoneModel>
                    {
                        new HazardZoneModel { ZoneId = "gas-zone-source", ZoneName = "Leak Source", RiskColor = "#FF0000", Position = new float[] {0,0,0}, Radius = 2f, DangerLevel = 0.95f },
                        new HazardZoneModel { ZoneId = "gas-zone-spread", ZoneName = "Gas Dispersion Area", RiskColor = "#FF6347", Position = new float[] {0,1,0}, Radius = 4f, DangerLevel = 0.6f },
                        new HazardZoneModel { ZoneId = "gas-zone-ventilation", ZoneName = "Ventilation Intake", RiskColor = "#32CD32", Position = new float[] {6,0,0}, Radius = 2.5f, DangerLevel = 0.2f }
                    },
                    RequiredActions = new List<RequiredActionModel>
                    {
                        new RequiredActionModel { ActionId = "detect-gas", ActionName = "Detect Gas", Description = "Use the 4-gas detector to identify methane concentration.", CompletionFeedback = "Methane detected at 2.1% LEL. Gas type confirmed.", InteractionRadius = 3f, TimeLimitSeconds = 10f, ScoreValue = 15, IsCritical = true },
                        new RequiredActionModel { ActionId = "select-ppe", ActionName = "Select PPE", Description = "Don the SCBA and chemical-resistant suit.", CompletionFeedback = "PPE equipped. You are protected against methane.", InteractionRadius = 2f, TimeLimitSeconds = 20f, ScoreValue = 20, IsCritical = true },
                        new RequiredActionModel { ActionId = "activate-ventilation", ActionName = "Activate Ventilation", Description = "Switch on the ventilation fan to disperse the gas cloud.", CompletionFeedback = "Ventilation active. Methane concentration dropping.", InteractionRadius = 3f, TimeLimitSeconds = 30f, ScoreValue = 25, IsCritical = true },
                        new RequiredActionModel { ActionId = "evacuate", ActionName = "Evacuate", Description = "Guide all workers out of the confined space.", CompletionFeedback = "All personnel evacuated. Confined space cleared.", InteractionRadius = 4f, TimeLimitSeconds = 45f, ScoreValue = 40, IsCritical = true }
                    },
                    EscalationEvents = new List<EscalationEventModel>
                    {
                        new EscalationEventModel { Level = 0, LevelName = "Normal", TimeThreshold = 0f, ScorePenalty = 0f, VisualEffect = "none", AudioEffect = "ambient-underground", WarningText = "Baseline readings normal. Begin scenario.", TriggerAutoFail = false, HazardGrowthMultiplier = 1f },
                        new EscalationEventModel { Level = 1, LevelName = "Minor", TimeThreshold = 15f, ScorePenalty = 0.10f, VisualEffect = "slight-vapour-haze", AudioEffect = "gas-hiss-alarm-beep", WarningText = "Gas detector shows rising methane. Locate the source.", TriggerAutoFail = false, HazardGrowthMultiplier = 1.3f },
                        new EscalationEventModel { Level = 2, LevelName = "Moderate", TimeThreshold = 30f, ScorePenalty = 0.25f, VisualEffect = "dense-vapour-limited-visibility", AudioEffect = "continuous-alarm-gas-hiss-loud", WarningText = "Methane above 5% LEL. Don PPE and activate ventilation!", ShowEvacuationArrows = true, TriggerAutoFail = false, HazardGrowthMultiplier = 1.6f },
                        new EscalationEventModel { Level = 3, LevelName = "Severe", TimeThreshold = 45f, ScorePenalty = 0.50f, VisualEffect = "thick-gas-screen-edges-red-tint", AudioEffect = "alarms-siren-rumbling", WarningText = "WARNING: Methane approaching explosive limit. Evacuate!", ShowEvacuationArrows = true, TriggerAutoFail = false, HazardGrowthMultiplier = 2.2f },
                        new EscalationEventModel { Level = 4, LevelName = "Critical", TimeThreshold = 60f, ScorePenalty = 1f, VisualEffect = "explosion-flash-screen-shake-blackout", AudioEffect = "explosion-siren-echo", WarningText = "CRITICAL: Methane reached explosive limit. Scenario failed.", ShowEvacuationArrows = true, TriggerAutoFail = true, HazardGrowthMultiplier = 3f }
                    }
                }
            },
            Questions = new List<AssessmentQuestionModel>
            {
                new AssessmentQuestionModel { QuestionId = "gas-q1", ModuleId = "gas-leak-101", QuestionText = "What is the Lower Explosive Limit (LEL) of methane?", Options = new List<string> { "5%", "15%", "25%", "50%" }, CorrectAnswerIndex = 0, Explanation = "Methane has an LEL of approximately 5% in air.", ScoreValue = 20, Difficulty = "medium" },
                new AssessmentQuestionModel { QuestionId = "gas-q2", ModuleId = "gas-leak-101", QuestionText = "Which PPE is essential for a confined space methane leak?", Options = new List<string> { "Hard hat and boots only", "Chemical-resistant suit with SCBA", "Dust mask and goggles", "Hi-vis vest and ear protection" }, CorrectAnswerIndex = 1, Explanation = "SCBA provides breathable air independent of the environment.", ScoreValue = 20, Difficulty = "easy" },
                new AssessmentQuestionModel { QuestionId = "gas-q3", ModuleId = "gas-leak-101", QuestionText = "What is the primary purpose of forced ventilation during a gas leak?", Options = new List<string> { "Cool the area", "Dilute and disperse gas below hazardous levels", "Improve lighting", "Create positive pressure" }, CorrectAnswerIndex = 1, Explanation = "Ventilation fans force fresh air in, diluting methane.", ScoreValue = 20, Difficulty = "medium" },
                new AssessmentQuestionModel { QuestionId = "gas-q4", ModuleId = "gas-leak-101", QuestionText = "Why should you NEVER use spark-producing devices in methane-rich spaces?", Options = new List<string> { "May damage equipment", "Methane can ignite above LEL with any ignition source", "Interferes with gas detector", "Only concern for hydrogen, not methane" }, CorrectAnswerIndex = 1, Explanation = "Methane becomes explosive between 5-15% LEL. Any spark can trigger detonation.", ScoreValue = 20, Difficulty = "easy" },
                new AssessmentQuestionModel { QuestionId = "gas-q5", ModuleId = "gas-leak-101", QuestionText = "What is the correct sequence for confined space methane emergency?", Options = new List<string> { "Evacuate then Detect then PPE then Ventilate", "Detect then PPE then Ventilate then Evacuate", "Ventilate then Detect then Evacuate then PPE", "PPE then Evacuate then Detect then Ventilate" }, CorrectAnswerIndex = 1, Explanation = "Detect first, don PPE, activate ventilation, then evacuate.", ScoreValue = 20, Difficulty = "medium" }
            }
        };
    }
}
