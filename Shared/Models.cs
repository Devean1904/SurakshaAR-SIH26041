namespace SurakshaAR.Shared;

public class Company
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string AdminId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
}

public class CreateCompanyRequest
{
    public string CompanyId { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string AdminUserId { get; set; } = string.Empty;
    public string AdminName { get; set; } = string.Empty;
    public string AdminPassword { get; set; } = string.Empty;
}

public class User
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Role { get; set; } = "Worker";
    public string Name { get; set; } = string.Empty;
    public string PreferredTheme { get; set; } = "Dark";
    public string Language { get; set; } = "en";
    public string Otp { get; set; } = string.Empty;
    public DateTime? OtpExpiry { get; set; }
    public string ManagedByAdminId { get; set; } = string.Empty;
    public string EnrolledByManagerId { get; set; } = string.Empty;
    public string CompanyId { get; set; } = string.Empty;
    public bool AwaitingAdminConfirmation { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<string> AssignedWorkerIds { get; set; } = new();
}

public class SiteMapping
{
    public string Id { get; set; } = string.Empty;
    public string AdminId { get; set; } = string.Empty;
    public string CompanyId { get; set; } = string.Empty;
    public string SiteName { get; set; } = string.Empty;
    public float[] AnchorPosition { get; set; } = new float[3];
    public float[] AnchorRotation { get; set; } = new float[4];
    public float[] AnchorScale { get; set; } = new float[3];
    public List<ScenarioPoint> ScenarioPoints { get; set; } = new();
    public string RecordedAt { get; set; } = string.Empty;
    public bool IsSynced { get; set; }
}

public class ScenarioPoint
{
    public string Id { get; set; } = string.Empty;
    public string ScenarioType { get; set; } = string.Empty;
    public float[] Position { get; set; } = new float[3];
    public float[] Rotation { get; set; } = new float[4];
    public string Description { get; set; } = string.Empty;
    public string RiskLevel { get; set; } = "Medium";
    public List<string> RequiredActions { get; set; } = new();
}

public class Scenario
{
    public string Id { get; set; } = string.Empty;
    public string SiteMappingId { get; set; } = string.Empty;
    public string WorkerId { get; set; } = string.Empty;
    public string CompanyId { get; set; } = string.Empty;
    public string AssignedByManagerId { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public List<ScenarioPoint> Points { get; set; } = new();
    public CitificationData Citification { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class CitificationData
{
    public string ZoneId { get; set; } = string.Empty;
    public string ZoneName { get; set; } = string.Empty;
    public string RiskLevel { get; set; } = "Medium";
    public List<string> EvacuationRoutes { get; set; } = new();
    public List<string> SafetyEquipment { get; set; } = new();
    public float MaxOccupancy { get; set; }
    public float CurrentOccupancy { get; set; }
}

public class EscalationReport
{
    public string Id { get; set; } = string.Empty;
    public string WorkerId { get; set; } = string.Empty;
    public string CompanyId { get; set; } = string.Empty;
    public string ReportType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = "Medium";
    public float[] Location { get; set; } = new float[3];
    public DateTime ReportedAt { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Active";
    public string BlockchainHash { get; set; } = string.Empty;
    public string AssignedTo { get; set; } = string.Empty;
}

public class LoginRequest
{
    public string UserId { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string CompanyId { get; set; } = string.Empty;
}

public class OtpRequest
{
    public string PhoneNumber { get; set; } = string.Empty;
}

public class OtpVerifyRequest
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string Otp { get; set; } = string.Empty;
    public string CompanyId { get; set; } = string.Empty;
}

public class AuthResponse
{
    public bool Success { get; set; }
    public string Token { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Theme { get; set; } = "Dark";
    public string Language { get; set; } = "en";
    public string CompanyId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class OfflineData
{
    public List<Scenario> Scenarios { get; set; } = new();
    public List<SiteMapping> SiteMappings { get; set; } = new();
    public List<TrainingAttemptModel> Attempts { get; set; } = new();
    public DateTime LastSyncTime { get; set; }
    public string UserId { get; set; } = string.Empty;
}

public class TrainingModuleModel
{
    public string ModuleId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string SafetyDomain { get; set; } = string.Empty;
    public int DifficultyLevel { get; set; }
    public float TimeLimitSeconds { get; set; }
    public float PassThreshold { get; set; } = 70f;
    public List<ScenarioConfigModel> Scenarios { get; set; } = new();
    public List<AssessmentQuestionModel> Questions { get; set; } = new();
    public List<string> RequiredEquipment { get; set; } = new();
}

public class ScenarioConfigModel
{
    public string ScenarioId { get; set; } = string.Empty;
    public string ModuleId { get; set; } = string.Empty;
    public string HazardType { get; set; } = string.Empty;
    public string ScenarioName { get; set; } = string.Empty;
    public float InitialDangerLevel { get; set; }
    public float EscalationIntervalSeconds { get; set; } = 15f;
    public int MaxEscalationLevel { get; set; }
    public List<HazardZoneModel> HazardZones { get; set; } = new();
    public List<RequiredActionModel> RequiredActions { get; set; } = new();
    public List<EscalationEventModel> EscalationEvents { get; set; } = new();
}

public class HazardZoneModel
{
    public string ZoneId { get; set; } = string.Empty;
    public string ZoneName { get; set; } = string.Empty;
    public string RiskColor { get; set; } = "#FF0000";
    public float[] Position { get; set; } = new float[3];
    public float Radius { get; set; } = 1f;
    public float DangerLevel { get; set; } = 0.5f;
}

public class RequiredActionModel
{
    public string ActionId { get; set; } = string.Empty;
    public string ActionName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CompletionFeedback { get; set; } = string.Empty;
    public float InteractionRadius { get; set; } = 2f;
    public float TimeLimitSeconds { get; set; } = 15f;
    public int ScoreValue { get; set; } = 20;
    public bool IsCritical { get; set; }
}

public class EscalationEventModel
{
    public int Level { get; set; }
    public string LevelName { get; set; } = string.Empty;
    public float TimeThreshold { get; set; }
    public float ScorePenalty { get; set; }
    public string VisualEffect { get; set; } = string.Empty;
    public string AudioEffect { get; set; } = string.Empty;
    public string WarningText { get; set; } = string.Empty;
    public bool ShowEvacuationArrows { get; set; }
    public bool TriggerAutoFail { get; set; }
    public float HazardGrowthMultiplier { get; set; } = 1f;
}

public class AssessmentQuestionModel
{
    public string QuestionId { get; set; } = string.Empty;
    public string ModuleId { get; set; } = string.Empty;
    public string QuestionText { get; set; } = string.Empty;
    public string QuestionType { get; set; } = "multiple_choice";
    public List<string> Options { get; set; } = new();
    public int CorrectAnswerIndex { get; set; }
    public string Explanation { get; set; } = string.Empty;
    public int ScoreValue { get; set; }
    public string Difficulty { get; set; } = "medium";
}

public class CertificateModel
{
    public string CertificateId { get; set; } = string.Empty;
    public string EmployeeId { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string ModuleId { get; set; } = string.Empty;
    public string ModuleName { get; set; } = string.Empty;
    public int Score { get; set; }
    public bool Passed { get; set; }
    public string IssuedAt { get; set; } = string.Empty;
    public string Signature { get; set; } = string.Empty;
    public string BlockchainTx { get; set; } = string.Empty;
    public string QrPayload { get; set; } = string.Empty;
}

public class AssessmentModel
{
    public string AssessmentId { get; set; } = string.Empty;
    public string EmployeeId { get; set; } = string.Empty;
    public string ModuleId { get; set; } = string.Empty;
    public string AttemptId { get; set; } = string.Empty;
    public int ActionScore { get; set; }
    public int QuestionScore { get; set; }
    public int TotalScore { get; set; }
    public bool Passed { get; set; }
    public List<int> Answers { get; set; } = new();
    public string SubmittedAt { get; set; } = string.Empty;
}

public class TrainingAttemptModel
{
    public string AttemptId { get; set; } = string.Empty;
    public string EmployeeId { get; set; } = string.Empty;
    public string ModuleId { get; set; } = string.Empty;
    public string ScenarioId { get; set; } = string.Empty;
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public int ActionScore { get; set; }
    public int QuestionScore { get; set; }
    public int TotalScore { get; set; }
    public bool Passed { get; set; }
    public int EscalationLevel { get; set; }
    public string Status { get; set; } = "in_progress";
}

public class CheckpointModel
{
    public string CheckpointId { get; set; } = string.Empty;
    public string QrCode { get; set; } = string.Empty;
    public string ModuleId { get; set; } = string.Empty;
    public string ScenarioId { get; set; } = string.Empty;
    public string SiteName { get; set; } = string.Empty;
    public float[] Position { get; set; } = new float[3];
}
