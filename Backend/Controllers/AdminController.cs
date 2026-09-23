using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SurakshaAR.Shared;
using SurakshaAR.Backend.Services;
using System.Security.Claims;
using MongoDB.Bson;
using MongoDB.Driver;

namespace SurakshaAR.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AdminOnly")]
public class AdminController : ControllerBase
{
    private readonly AuthService _auth;
    private readonly ScenarioService _scenario;
    private readonly EscalationService _escalation;
    private readonly MongoService _mongo;

    public AdminController(AuthService auth, ScenarioService scenario, EscalationService escalation, MongoService mongo)
    {
        _auth = auth;
        _scenario = scenario;
        _escalation = escalation;
        _mongo = mongo;
    }

    private string GetAdminId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
    private string GetCompanyId() => User.FindFirst("companyId")?.Value ?? "";

    [HttpGet("compliance")]
    public IActionResult GetCompliance()
    {
        var companyId = GetCompanyId();
        if (string.IsNullOrEmpty(companyId))
            return BadRequest(new { message = "Company context missing" });

        var assessments = _mongo.RawCollection("assessments");
        var certificates = _mongo.RawCollection("certificates");
        var attempts = _mongo.RawCollection("training_attempts");
        var companyFilter = Builders<BsonDocument>.Filter.Eq("companyId", companyId);

        var assessmentDocs = assessments.Find(companyFilter).ToList();
        var certDocs = certificates.Find(companyFilter).ToList();
        var attemptDocs = attempts.Find(companyFilter).ToList();

        int totalAssessments = assessmentDocs.Count;
        int passedAssessments = assessmentDocs.Count(d => d.Contains("passed") && d["passed"].AsBoolean);
        double passRate = totalAssessments > 0 ? Math.Round(100.0 * passedAssessments / totalAssessments, 1) : 0;
        double avgScore = totalAssessments > 0
            ? Math.Round(assessmentDocs.Average(d => d.Contains("totalScore") ? d["totalScore"].AsInt32 : 0), 1)
            : 0;

        var byModule = assessmentDocs
            .GroupBy(d => d.Contains("moduleId") ? d["moduleId"].AsString : "unknown")
            .Select(g => new
            {
                moduleId = g.Key,
                attempts = g.Count(),
                passed = g.Count(d => d.Contains("passed") && d["passed"].AsBoolean),
                avgScore = Math.Round(g.Average(d => d.Contains("totalScore") ? d["totalScore"].AsInt32 : 0), 1)
            })
            .OrderByDescending(x => x.attempts)
            .ToList();

        var recentAssessments = assessmentDocs
            .OrderByDescending(d => d.Contains("submittedAt") ? d["submittedAt"].AsString : "")
            .Take(20)
            .Select(d => new
            {
                assessmentId = d.Contains("assessmentId") ? d["assessmentId"].AsString : "",
                employeeId = d.Contains("employeeId") ? d["employeeId"].AsString : "",
                moduleId = d.Contains("moduleId") ? d["moduleId"].AsString : "",
                actionScore = d.Contains("actionScore") ? d["actionScore"].AsInt32 : 0,
                questionScore = d.Contains("questionScore") ? d["questionScore"].AsInt32 : 0,
                totalScore = d.Contains("totalScore") ? d["totalScore"].AsInt32 : 0,
                passed = d.Contains("passed") && d["passed"].AsBoolean,
                submittedAt = d.Contains("submittedAt") ? d["submittedAt"].AsString : ""
            })
            .ToList();

        var recentCertificates = certDocs
            .OrderByDescending(d => d.Contains("issuedAt") ? d["issuedAt"].AsString : "")
            .Take(20)
            .Select(d => new
            {
                certificateId = d.Contains("certificateId") ? d["certificateId"].AsString : "",
                employeeId = d.Contains("employeeId") ? d["employeeId"].AsString : "",
                employeeName = d.Contains("employeeName") ? d["employeeName"].AsString : "",
                moduleId = d.Contains("moduleId") ? d["moduleId"].AsString : "",
                moduleName = d.Contains("moduleName") ? d["moduleName"].AsString : "",
                score = d.Contains("score") ? d["score"].AsInt32 : 0,
                issuedAt = d.Contains("issuedAt") ? d["issuedAt"].AsString : "",
                blockchainTx = d.Contains("blockchainTx") ? d["blockchainTx"].AsString : ""
            })
            .ToList();

        var completedAttempts = attemptDocs.Count(d => d.Contains("status") && d["status"].AsString == "completed");

        return Ok(new
        {
            totalWorkers = _auth.GetUsersByRoleAsync("Worker", companyId).Result.Count,
            totalAssessments,
            passedAssessments,
            failedAssessments = totalAssessments - passedAssessments,
            passRate,
            avgScore,
            totalCertificates = certDocs.Count,
            completedAttempts,
            totalAttempts = attemptDocs.Count,
            inProgressAttempts = attemptDocs.Count - completedAttempts,
            byModule,
            recentAssessments,
            recentCertificates
        });
    }

    [HttpGet("certificates")]
    public IActionResult GetCertificates()
    {
        var companyId = GetCompanyId();
        if (string.IsNullOrEmpty(companyId))
            return BadRequest(new { message = "Company context missing" });

        var certificates = _mongo.RawCollection("certificates");
        var docs = certificates.Find(Builders<BsonDocument>.Filter.Eq("companyId", companyId))
            .SortByDescending(d => d["issuedAt"])
            .Limit(200)
            .ToList();
        return Ok(docs.Select(d => new
        {
            certificateId = d.Contains("certificateId") ? d["certificateId"].AsString : "",
            employeeId = d.Contains("employeeId") ? d["employeeId"].AsString : "",
            employeeName = d.Contains("employeeName") ? d["employeeName"].AsString : "",
            moduleId = d.Contains("moduleId") ? d["moduleId"].AsString : "",
            moduleName = d.Contains("moduleName") ? d["moduleName"].AsString : "",
            score = d.Contains("score") ? d["score"].AsInt32 : 0,
            passed = d.Contains("passed") && d["passed"].AsBoolean,
            issuedAt = d.Contains("issuedAt") ? d["issuedAt"].AsString : "",
            blockchainTx = d.Contains("blockchainTx") ? d["blockchainTx"].AsString : "",
            qrPayload = d.Contains("qrPayload") ? d["qrPayload"].AsString : ""
        }).ToList());
    }

    [HttpGet("attempts")]
    public IActionResult GetAttempts()
    {
        var companyId = GetCompanyId();
        if (string.IsNullOrEmpty(companyId))
            return BadRequest(new { message = "Company context missing" });

        var assessments = _mongo.RawCollection("assessments");
        var trainingAttempts = _mongo.RawCollection("training_attempts");
        var companyFilter = Builders<BsonDocument>.Filter.Eq("companyId", companyId);

        var assessmentDocs = assessments.Find(companyFilter)
            .SortByDescending(d => d["submittedAt"])
            .Limit(100)
            .ToList();

        var trainingDocs = trainingAttempts.Find(companyFilter)
            .SortByDescending(d => d["startTime"])
            .Limit(100)
            .ToList();

        var assessmentRows = assessmentDocs.Select(d => new
        {
            type = "assessment",
            id = d.Contains("assessmentId") ? d["assessmentId"].AsString : "",
            attemptId = d.Contains("attemptId") ? d["attemptId"].AsString : "",
            employeeId = d.Contains("employeeId") ? d["employeeId"].AsString : "",
            moduleId = d.Contains("moduleId") ? d["moduleId"].AsString : "",
            scenarioId = "",
            actionScore = d.Contains("actionScore") ? d["actionScore"].AsInt32 : 0,
            questionScore = d.Contains("questionScore") ? d["questionScore"].AsInt32 : 0,
            totalScore = d.Contains("totalScore") ? d["totalScore"].AsInt32 : 0,
            passed = d.Contains("passed") && d["passed"].AsBoolean,
            status = d.Contains("passed") && d["passed"].AsBoolean ? "passed" : "failed",
            escalationLevel = 0,
            startTime = "",
            endTime = d.Contains("submittedAt") ? d["submittedAt"].AsString : "",
            submittedAt = d.Contains("submittedAt") ? d["submittedAt"].AsString : ""
        }).ToList();

        var trainingRows = trainingDocs.Select(d => new
        {
            type = "training",
            id = d.Contains("attemptId") ? d["attemptId"].AsString : "",
            attemptId = d.Contains("attemptId") ? d["attemptId"].AsString : "",
            employeeId = d.Contains("employeeId") ? d["employeeId"].AsString : "",
            moduleId = d.Contains("moduleId") ? d["moduleId"].AsString : "",
            scenarioId = d.Contains("scenarioId") ? d["scenarioId"].AsString : "",
            actionScore = d.Contains("actionScore") ? d["actionScore"].AsInt32 : 0,
            questionScore = d.Contains("questionScore") ? d["questionScore"].AsInt32 : 0,
            totalScore = d.Contains("totalScore") ? d["totalScore"].AsInt32 : 0,
            passed = d.Contains("passed") && d["passed"].AsBoolean,
            status = d.Contains("status") ? d["status"].AsString : "",
            escalationLevel = d.Contains("escalationLevelReached") ? d["escalationLevelReached"].AsInt32 : 0,
            startTime = d.Contains("startTime") ? d["startTime"].AsString : "",
            endTime = d.Contains("endTime") ? d["endTime"].AsString : "",
            submittedAt = d.Contains("endTime") ? d["endTime"].AsString :
                (d.Contains("startTime") ? d["startTime"].AsString : "")
        }).ToList();

        var combined = assessmentRows.Cast<object>()
            .Concat(trainingRows.Cast<object>())
            .ToList();

        return Ok(new
        {
            assessments = assessmentRows,
            trainingAttempts = trainingRows,
            items = combined,
            total = combined.Count
        });
    }

    [HttpPost("manager/add")]
    public async Task<IActionResult> AddManager([FromBody] AddManagerRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.UserId) || string.IsNullOrWhiteSpace(req.Name) || string.IsNullOrWhiteSpace(req.Password))
            return BadRequest(new { message = "UserId, Name, and Password are required" });

        var manager = new User
        {
            UserId = req.UserId,
            Name = req.Name,
            PhoneNumber = req.PhoneNumber,
            Role = "Manager"
        };
        var result = await _auth.RegisterManagerAsync(GetAdminId(), manager, req.Password);
        if (!result) return BadRequest(new { message = "Failed to add manager. UserId may already exist." });
        return Ok(new { message = "Manager added" });
    }

    [HttpPost("manager/remove")]
    public async Task<IActionResult> RemoveManager([FromBody] RemoveUserRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.UserId))
            return BadRequest(new { message = "UserId is required" });

        var target = await _auth.GetUserInCompanyAsync(req.UserId, GetCompanyId());
        if (target == null || target.Role != "Manager")
            return BadRequest(new { message = "Manager not found in your company" });

        var result = await _auth.DeleteUserAsync(req.UserId, GetCompanyId());
        if (!result) return BadRequest(new { message = "Manager not found" });
        return Ok(new { message = "Manager removed" });
    }

    [HttpGet("managers")]
    public async Task<IActionResult> GetManagers()
    {
        var managers = await _auth.GetUsersByRoleAsync("Manager", GetCompanyId());
        return Ok(managers.Select(m => new
        {
            m.UserId, m.Name, m.PhoneNumber, m.CreatedAt, m.AssignedWorkerIds
        }));
    }

    [HttpPost("worker/add")]
    public async Task<IActionResult> AddWorker([FromBody] AdminAddWorkerRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.UserId) || string.IsNullOrWhiteSpace(req.Name) || string.IsNullOrWhiteSpace(req.Password) || string.IsNullOrWhiteSpace(req.ManagerId))
            return BadRequest(new { message = "UserId, Name, Password, and ManagerId are required" });

        var manager = await _auth.GetUserInCompanyAsync(req.ManagerId, GetCompanyId());
        if (manager == null || manager.Role != "Manager")
            return BadRequest(new { message = "Manager not found in your company" });

        var worker = new User
        {
            UserId = req.UserId,
            Name = req.Name,
            PhoneNumber = req.PhoneNumber,
            Role = "Worker"
        };
        var result = await _auth.RegisterWorkerAsync(req.ManagerId, worker, req.Password);
        if (!result) return BadRequest(new { message = "Failed to add worker. UserId may already exist or Manager not found." });
        return Ok(new { message = "Worker added (pending admin confirmation)" });
    }

    [HttpPost("worker/confirm")]
    public async Task<IActionResult> ConfirmWorker([FromBody] ConfirmWorkerRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.WorkerId))
            return BadRequest(new { message = "WorkerId is required" });

        var result = await _auth.ConfirmWorkerAsync(req.WorkerId, GetCompanyId());
        if (!result) return BadRequest(new { message = "Worker not found or already confirmed" });
        return Ok(new { message = "Worker confirmed" });
    }

    [HttpGet("workers/pending")]
    public async Task<IActionResult> GetPendingWorkers()
    {
        var users = await _auth.GetUsersByRoleAsync("Worker", GetCompanyId());
        var pending = users.Where(u => u.AwaitingAdminConfirmation).ToList();
        return Ok(pending.Select(w => new { w.UserId, w.Name, w.PhoneNumber, w.EnrolledByManagerId }));
    }

    [HttpGet("workers")]
    public async Task<IActionResult> GetAllWorkers()
    {
        var workers = await _auth.GetUsersByRoleAsync("Worker", GetCompanyId());
        return Ok(workers.Select(w => new
        {
            w.UserId, w.Name, w.PhoneNumber, w.AwaitingAdminConfirmation,
            w.EnrolledByManagerId, w.IsActive, w.CreatedAt
        }));
    }

    [HttpPost("worker/remove")]
    public async Task<IActionResult> RemoveWorker([FromBody] RemoveUserRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.UserId))
            return BadRequest(new { message = "UserId is required" });

        var target = await _auth.GetUserInCompanyAsync(req.UserId, GetCompanyId());
        if (target == null || target.Role != "Worker")
            return BadRequest(new { message = "Worker not found in your company" });

        var result = await _auth.DeleteUserAsync(req.UserId, GetCompanyId());
        if (!result) return BadRequest(new { message = "Worker not found" });
        return Ok(new { message = "Worker removed" });
    }

    [HttpPost("site/map")]
    public async Task<IActionResult> MapSite([FromBody] SiteMapping mapping)
    {
        var companyId = GetCompanyId();
        if (string.IsNullOrEmpty(companyId))
            return BadRequest(new { message = "Company context missing" });

        mapping.AdminId = GetAdminId();
        mapping.CompanyId = companyId;
        var id = await _scenario.CreateSiteMappingAsync(mapping);
        return Ok(new { message = "Site mapped", mappingId = id });
    }

    [HttpGet("sites")]
    public async Task<IActionResult> GetSites()
    {
        var companyId = GetCompanyId();
        if (string.IsNullOrEmpty(companyId))
            return BadRequest(new { message = "Company context missing" });
        var sites = await _scenario.GetSiteMappingsAsync(GetAdminId(), companyId);
        return Ok(sites);
    }

    [HttpGet("escalations")]
    public async Task<IActionResult> GetEscalations()
    {
        var companyId = GetCompanyId();
        if (string.IsNullOrEmpty(companyId))
            return BadRequest(new { message = "Company context missing" });
        var escalations = await _escalation.GetEscalationsForCompanyAsync(GetCompanyId());
        return Ok(escalations);
    }

    [HttpPost("escalation/assign")]
    public async Task<IActionResult> AssignEscalation([FromBody] AssignEscalationRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.ReportId) || string.IsNullOrWhiteSpace(req.AssignedTo))
            return BadRequest(new { message = "ReportId and AssignedTo are required" });

        var companyId = GetCompanyId();
        if (string.IsNullOrEmpty(companyId))
            return BadRequest(new { message = "Company context missing" });

        var assignee = await _auth.GetUserInCompanyAsync(req.AssignedTo, companyId);
        if (assignee == null)
            return BadRequest(new { message = "Assignee not found in your company" });

        var result = await _escalation.AssignEscalationAsync(req.ReportId, req.AssignedTo, companyId);
        if (!result) return NotFound(new { message = "Escalation not found in your company" });
        return Ok(new { message = "Assigned" });
    }

    [HttpPost("escalation/resolve")]
    public async Task<IActionResult> ResolveEscalation([FromBody] ResolveRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.ReportId))
            return BadRequest(new { message = "ReportId is required" });

        var companyId = GetCompanyId();
        if (string.IsNullOrEmpty(companyId))
            return BadRequest(new { message = "Company context missing" });

        var result = await _escalation.ResolveEscalationAsync(req.ReportId, companyId);
        if (!result) return NotFound(new { message = "Escalation not found in your company" });
        return Ok(new { message = "Resolved" });
    }

    [HttpGet("admins")]
    public async Task<IActionResult> GetAdmins()
    {
        var admins = await _auth.GetUsersByRoleAsync("Admin", GetCompanyId());
        return Ok(admins.Select(a => new
        {
            a.UserId, a.Name, a.PhoneNumber, a.CompanyId, a.CreatedAt
        }));
    }

    [HttpPost("admin/remove")]
    public async Task<IActionResult> RemoveAdmin([FromBody] RemoveAdminRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.UserId))
            return BadRequest(new { message = "UserId is required" });

        if (req.UserId == GetAdminId())
            return BadRequest(new { message = "Cannot remove yourself" });

        var target = await _auth.GetUserInCompanyAsync(req.UserId, GetCompanyId());
        if (target == null || target.Role != "Admin")
            return BadRequest(new { message = "Admin not found in your company" });

        var admins = await _auth.GetUsersByRoleAsync("Admin", GetCompanyId());
        if (admins.Count <= 1)
            return BadRequest(new { message = "Cannot remove the last admin" });

        var result = await _auth.DeleteUserAsync(req.UserId, GetCompanyId());
        if (!result) return BadRequest(new { message = "Admin not found" });
        return Ok(new { message = "Admin removed" });
    }
}

public class AddManagerRequest
{
    public string UserId { get; set; } = "";
    public string Name { get; set; } = "";
    public string PhoneNumber { get; set; } = "";
    public string Password { get; set; } = "";
}

public class AdminAddWorkerRequest
{
    public string ManagerId { get; set; } = "";
    public string UserId { get; set; } = "";
    public string Name { get; set; } = "";
    public string PhoneNumber { get; set; } = "";
    public string Password { get; set; } = "";
}

public class RemoveUserRequest { public string UserId { get; set; } = ""; }
public class ConfirmWorkerRequest { public string WorkerId { get; set; } = ""; }
public class AssignEscalationRequest { public string ReportId { get; set; } = ""; public string AssignedTo { get; set; } = ""; }
public class ResolveRequest { public string ReportId { get; set; } = ""; }
public class RemoveAdminRequest { public string UserId { get; set; } = ""; }
