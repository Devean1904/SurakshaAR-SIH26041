using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SurakshaAR.Shared;
using SurakshaAR.Backend.Services;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Security.Claims;

namespace SurakshaAR.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AuthenticatedUser")]
public class WorkerController : ControllerBase
{
    private readonly ScenarioService _scenario;
    private readonly EscalationService _escalation;
    private readonly AuthService _auth;
    private readonly MongoService _mongo;
    private readonly IMongoCollection<BsonDocument> _attempts;

    public WorkerController(ScenarioService scenario, EscalationService escalation, AuthService auth, MongoService mongo)
    {
        _scenario = scenario;
        _escalation = escalation;
        _auth = auth;
        _mongo = mongo;
        _attempts = mongo.RawCollection("training_attempts");
    }

    private string GetCallerId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
    private string GetCallerRole() => User.FindFirst(ClaimTypes.Role)?.Value ?? "";
    private string GetCompanyId() => User.FindFirst("companyId")?.Value ?? "";

    private async Task<bool> CanAccessWorkerDataAsync(string workerId)
    {
        var role = GetCallerRole();
        if (string.Equals(role, "Worker", StringComparison.Ordinal))
            return GetCallerId() == workerId;

        var companyId = GetCompanyId();
        if (string.IsNullOrEmpty(companyId)) return false;
        var target = await _auth.GetUserInCompanyAsync(workerId, companyId);
        return target != null && target.Role == "Worker";
    }

    [HttpGet("scenarios/{workerId}")]
    public async Task<IActionResult> GetMyScenarios(string workerId)
    {
        if (!await CanAccessWorkerDataAsync(workerId))
            return Forbid();

        var scenarios = await _scenario.GetScenariosForWorkerAsync(workerId, GetCompanyId());
        return Ok(scenarios);
    }

    [HttpPost("escalation/report")]
    public async Task<IActionResult> ReportEscalation([FromBody] EscalationReport report)
    {
        if (string.IsNullOrWhiteSpace(report.ReportType) || string.IsNullOrWhiteSpace(report.Description))
            return BadRequest(new { message = "ReportType and Description are required" });

        var companyId = GetCompanyId();
        if (string.IsNullOrEmpty(companyId))
            return BadRequest(new { message = "Company context missing" });

        report.WorkerId = GetCallerId();
        report.CompanyId = companyId;
        var result = await _escalation.ReportEscalationAsync(report);
        return Ok(new { message = "Escalation reported", id = result.Id, hash = result.BlockchainHash });
    }

    [HttpGet("escalations/{workerId}")]
    public async Task<IActionResult> GetMyEscalations(string workerId)
    {
        if (!await CanAccessWorkerDataAsync(workerId))
            return Forbid();

        var mine = await _escalation.GetEscalationsForWorkerAsync(workerId);
        return Ok(mine);
    }

    [HttpPost("site/record")]
    public async Task<IActionResult> RecordSite([FromBody] SiteMapping mapping)
    {
        var companyId = GetCompanyId();
        if (string.IsNullOrEmpty(companyId))
            return BadRequest(new { message = "Company context missing" });

        mapping.AdminId = GetCallerId();
        mapping.CompanyId = companyId;
        if (string.IsNullOrEmpty(mapping.RecordedAt))
            mapping.RecordedAt = DateTime.UtcNow.ToString("O");
        var id = await _scenario.CreateSiteMappingAsync(mapping);
        return Ok(new { message = "Site recorded", mappingId = id });
    }

    [HttpGet("offline/{workerId}")]
    public async Task<IActionResult> GetOfflineData(string workerId)
    {
        if (!await CanAccessWorkerDataAsync(workerId))
            return Forbid();

        var scenarios = await _scenario.GetScenariosForWorkerAsync(workerId, GetCompanyId());
        var siteMappings = await _scenario.GetSiteMappingsForWorkerAsync(workerId);

        var attemptFilter = Builders<BsonDocument>.Filter.Eq("employeeId", workerId);
        if (!string.Equals(GetCallerRole(), "Worker", StringComparison.Ordinal))
            attemptFilter &= Builders<BsonDocument>.Filter.Eq("companyId", GetCompanyId());
        var attempts = _attempts.Find(attemptFilter)
            .SortByDescending(a => a["startTime"])
            .Limit(50)
            .ToList()
            .Select(a => new TrainingAttemptModel
            {
                AttemptId = a.Contains("attemptId") ? a["attemptId"].AsString : "",
                EmployeeId = a.Contains("employeeId") ? a["employeeId"].AsString : "",
                ModuleId = a.Contains("moduleId") ? a["moduleId"].AsString : "",
                ScenarioId = a.Contains("scenarioId") ? a["scenarioId"].AsString : "",
                StartTime = a.Contains("startTime") ? a["startTime"].AsString : "",
                EndTime = a.Contains("endTime") ? a["endTime"].AsString : "",
                ActionScore = a.Contains("actionScore") ? a["actionScore"].AsInt32 : 0,
                QuestionScore = a.Contains("questionScore") ? a["questionScore"].AsInt32 : 0,
                TotalScore = a.Contains("totalScore") ? a["totalScore"].AsInt32 : 0,
                Passed = a.Contains("passed") && a["passed"].AsBoolean,
                EscalationLevel = a.Contains("escalationLevelReached") ? a["escalationLevelReached"].AsInt32 : 0,
                Status = a.Contains("status") ? a["status"].AsString : ""
            }).ToList();

        var offlineData = new OfflineData
        {
            Scenarios = scenarios,
            SiteMappings = siteMappings,
            Attempts = attempts,
            LastSyncTime = DateTime.UtcNow,
            UserId = workerId
        };
        return Ok(offlineData);
    }

    [HttpPost("online/sync")]
    public async Task<IActionResult> SyncOnline([FromBody] SyncRequest req)
    {
        var callerId = GetCallerId();
        var companyId = GetCompanyId();
        var role = GetCallerRole();
        int completedCount = 0;
        int escalationCount = 0;
        int attemptCount = 0;
        var errors = new List<string>();

        foreach (var scenario in req.CompletedScenarios ?? new List<Scenario>())
        {
            try
            {
                var updated = await _scenario.MarkScenarioCompletedAsync(scenario.Id, callerId, role, companyId);
                if (updated) completedCount++;
                else errors.Add($"Scenario {scenario.Id}: not found or not yours");
            }
            catch (Exception ex)
            {
                errors.Add($"Scenario {scenario.Id}: {ex.Message}");
            }
        }

        if ((req.Escalations?.Count ?? 0) > 0 && string.IsNullOrEmpty(companyId))
        {
            errors.Add("Escalations require company context");
        }
        else
        {
            foreach (var escalation in req.Escalations ?? new List<EscalationReport>())
            {
                try
                {
                    escalation.WorkerId = callerId;
                    escalation.CompanyId = companyId;
                    await _escalation.ReportEscalationAsync(escalation);
                    escalationCount++;
                }
                catch (Exception ex)
                {
                    errors.Add($"Escalation: {ex.Message}");
                }
            }
        }

        foreach (var attempt in req.TrainingAttempts ?? new List<TrainingAttemptModel>())
        {
            try
            {
                if (string.IsNullOrWhiteSpace(attempt.AttemptId))
                {
                    errors.Add("Attempt missing AttemptId");
                    continue;
                }
                if (string.Equals(role, "Worker", StringComparison.Ordinal) &&
                    !string.Equals(attempt.EmployeeId, callerId, StringComparison.Ordinal) &&
                    !string.IsNullOrEmpty(attempt.EmployeeId))
                {
                    errors.Add($"Attempt {attempt.AttemptId}: not yours");
                    continue;
                }
                if (!string.Equals(role, "Worker", StringComparison.Ordinal))
                {
                    attempt.EmployeeId = string.IsNullOrEmpty(attempt.EmployeeId) ? callerId : attempt.EmployeeId;
                }
                else
                {
                    attempt.EmployeeId = callerId;
                }

                UpsertAttempt(attempt, companyId);
                attemptCount++;
            }
            catch (Exception ex)
            {
                errors.Add($"Attempt {attempt.AttemptId}: {ex.Message}");
            }
        }

        return Ok(new
        {
            message = "Synced",
            completedCount,
            escalationCount,
            attemptCount,
            errors
        });
    }

    private void UpsertAttempt(TrainingAttemptModel attempt, string companyId)
    {
        // Server-authoritative pass check: score must meet module threshold; honor client auto-fail.
        attempt.Passed = attempt.Passed && attempt.TotalScore >= GetPassThreshold(attempt.ModuleId);

        var filter = Builders<BsonDocument>.Filter.Eq("attemptId", attempt.AttemptId);
        var existing = _attempts.Find(filter).FirstOrDefault();
        if (existing == null)
        {
            var doc = new BsonDocument
            {
                { "attemptId", attempt.AttemptId },
                { "employeeId", attempt.EmployeeId },
                { "moduleId", attempt.ModuleId },
                { "scenarioId", attempt.ScenarioId },
                { "companyId", companyId },
                { "startTime", string.IsNullOrEmpty(attempt.StartTime) ? DateTime.UtcNow.ToString("O") : attempt.StartTime },
                { "endTime", attempt.EndTime ?? "" },
                { "actionScore", attempt.ActionScore },
                { "questionScore", attempt.QuestionScore },
                { "totalScore", attempt.TotalScore },
                { "passed", attempt.Passed },
                { "escalationLevelReached", attempt.EscalationLevel },
                { "status", string.IsNullOrEmpty(attempt.Status) ? (string.IsNullOrEmpty(attempt.EndTime) ? "in_progress" : "completed") : attempt.Status },
                { "syncedAt", DateTime.UtcNow.ToString("O") }
            };
            _attempts.InsertOne(doc);
            return;
        }

        var existingCompany = existing.Contains("companyId") ? existing["companyId"].AsString : "";
        if (!string.Equals(roleOrEmpty(), "Worker", StringComparison.Ordinal))
        {
            if (!string.IsNullOrEmpty(companyId) && !string.IsNullOrEmpty(existingCompany) &&
                !string.Equals(existingCompany, companyId, StringComparison.Ordinal))
                throw new InvalidOperationException("Attempt belongs to another company");
        }

        var update = Builders<BsonDocument>.Update.Set("syncedAt", DateTime.UtcNow.ToString("O"));
        if (!string.IsNullOrEmpty(attempt.EndTime))
        {
            update = update
                .Set("endTime", attempt.EndTime)
                .Set("actionScore", attempt.ActionScore)
                .Set("questionScore", attempt.QuestionScore)
                .Set("totalScore", attempt.TotalScore)
                .Set("passed", attempt.Passed)
                .Set("escalationLevelReached", attempt.EscalationLevel)
                .Set("status", string.IsNullOrEmpty(attempt.Status) ? "completed" : attempt.Status);
        }
        else if (!string.IsNullOrEmpty(attempt.Status))
        {
            update = update.Set("status", attempt.Status);
        }
        if (!string.IsNullOrEmpty(attempt.ModuleId))
            update = update.Set("moduleId", attempt.ModuleId);
        if (!string.IsNullOrEmpty(attempt.ScenarioId))
            update = update.Set("scenarioId", attempt.ScenarioId);
        if (!string.IsNullOrEmpty(attempt.StartTime))
            update = update.Set("startTime", attempt.StartTime);

        _attempts.UpdateOne(filter, update);
    }

    private float GetPassThreshold(string moduleId)
    {
        if (string.IsNullOrEmpty(moduleId)) return 70f;
        var modules = _mongo.RawCollection("training_modules");
        var m = modules.Find(Builders<BsonDocument>.Filter.Eq("moduleId", moduleId)).FirstOrDefault();
        if (m != null && m.Contains("passThreshold"))
        {
            var t = (float)m["passThreshold"].ToDouble();
            if (t > 0f) return t;
        }
        return 70f;
    }

    private string roleOrEmpty() => GetCallerRole();
}

public class SyncRequest
{
    public List<Scenario> CompletedScenarios { get; set; } = new();
    public List<EscalationReport> Escalations { get; set; } = new();
    public List<TrainingAttemptModel> TrainingAttempts { get; set; } = new();
}
