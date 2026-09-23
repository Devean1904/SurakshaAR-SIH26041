using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SurakshaAR.Shared;
using SurakshaAR.Backend.Services;
using System.Security.Claims;

namespace SurakshaAR.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "ManagerOrAdmin")]
public class ManagerController : ControllerBase
{
    private readonly AuthService _auth;
    private readonly ScenarioService _scenario;
    private readonly EscalationService _escalation;

    public ManagerController(AuthService auth, ScenarioService scenario, EscalationService escalation)
    {
        _auth = auth;
        _scenario = scenario;
        _escalation = escalation;
    }

    private string GetCallerId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
    private string GetCompanyId() => User.FindFirst("companyId")?.Value ?? "";

    [HttpPost("worker/add")]
    public async Task<IActionResult> AddWorker([FromBody] AddWorkerRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.UserId) || string.IsNullOrWhiteSpace(req.Name) || string.IsNullOrWhiteSpace(req.Password))
            return BadRequest(new { message = "UserId, Name, and Password are required" });

        var callerRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "";
        if (!string.Equals(callerRole, "Manager", StringComparison.Ordinal))
            return BadRequest(new { message = "Only managers can add workers directly; admins should use admin/worker/add" });

        var managerId = GetCallerId();
        var worker = new User
        {
            UserId = req.UserId,
            Name = req.Name,
            PhoneNumber = req.PhoneNumber
        };
        var result = await _auth.RegisterWorkerAsync(managerId, worker, req.Password);
        if (!result) return BadRequest(new { message = "Failed to add worker. UserId may already exist." });
        return Ok(new { message = "Worker added, awaiting admin confirmation" });
    }

    [HttpGet("workers")]
    public async Task<IActionResult> GetMyWorkers()
    {
        var managerId = GetCallerId();
        var companyId = GetCompanyId();
        var workers = await _auth.GetUsersByRoleAsync("Worker", companyId);
        var myWorkers = workers.Where(w => w.EnrolledByManagerId == managerId).ToList();
        return Ok(myWorkers.Select(w => new
        {
            w.UserId, w.Name, w.PhoneNumber, w.AwaitingAdminConfirmation, w.IsActive
        }));
    }

    [HttpPost("scenario/assign")]
    public async Task<IActionResult> AssignScenario([FromBody] Scenario scenario)
    {
        if (string.IsNullOrWhiteSpace(scenario.WorkerId))
            return BadRequest(new { message = "WorkerId is required" });

        var companyId = GetCompanyId();
        if (string.IsNullOrEmpty(companyId))
            return BadRequest(new { message = "Company context missing" });

        var worker = await _auth.GetUserInCompanyAsync(scenario.WorkerId, companyId);
        if (worker == null || worker.Role != "Worker")
            return BadRequest(new { message = "Worker not found in your company" });

        scenario.AssignedByManagerId = GetCallerId();
        scenario.CompanyId = companyId;
        var id = await _scenario.AssignScenarioAsync(scenario);
        return Ok(new { message = "Scenario assigned", scenarioId = id });
    }

    [HttpGet("scenarios/{workerId}")]
    public async Task<IActionResult> GetWorkerScenarios(string workerId)
    {
        var companyId = GetCompanyId();
        if (string.IsNullOrEmpty(companyId))
            return BadRequest(new { message = "Company context missing" });

        var worker = await _auth.GetUserInCompanyAsync(workerId, companyId);
        if (worker == null || worker.Role != "Worker")
            return NotFound(new { message = "Worker not found in your company" });

        var scenarios = await _scenario.GetScenariosForWorkerAsync(workerId, companyId);
        return Ok(scenarios);
    }

    [HttpGet("escalations")]
    public async Task<IActionResult> GetEscalations()
    {
        var companyId = GetCompanyId();
        if (string.IsNullOrEmpty(companyId))
            return BadRequest(new { message = "Company context missing" });

        var escalations = await _escalation.GetEscalationsForCompanyAsync(companyId);
        return Ok(escalations);
    }

    [HttpGet("citification/{siteMappingId}")]
    public async Task<IActionResult> GetCitification(string siteMappingId)
    {
        var companyId = GetCompanyId();
        var data = await _scenario.GetCitificationDataAsync(siteMappingId, companyId);
        return Ok(data);
    }

    [HttpPost("worker/remove")]
    public async Task<IActionResult> RemoveWorker([FromBody] RemoveUserRequest req)
    {
        var managerId = GetCallerId();
        var companyId = GetCompanyId();
        var workers = await _auth.GetUsersByRoleAsync("Worker", companyId);
        var worker = workers.FirstOrDefault(w => w.UserId == req.UserId && w.EnrolledByManagerId == managerId);
        if (worker == null) return BadRequest(new { message = "Worker not found or not under your management" });
        var result = await _auth.DeleteUserAsync(req.UserId, companyId);
        if (!result) return BadRequest(new { message = "Failed to remove worker" });
        return Ok(new { message = "Worker removed" });
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetManagerStats()
    {
        var managerId = GetCallerId();
        var companyId = GetCompanyId();
        var workers = await _auth.GetUsersByRoleAsync("Worker", companyId);
        var myWorkers = workers.Where(w => w.EnrolledByManagerId == managerId).ToList();
        return Ok(new
        {
            totalWorkers = myWorkers.Count,
            activeWorkers = myWorkers.Count(w => w.IsActive && !w.AwaitingAdminConfirmation),
            pendingWorkers = myWorkers.Count(w => w.AwaitingAdminConfirmation),
            inactiveWorkers = myWorkers.Count(w => !w.IsActive)
        });
    }
}

public class AddWorkerRequest
{
    public string UserId { get; set; } = "";
    public string Name { get; set; } = "";
    public string PhoneNumber { get; set; } = "";
    public string Password { get; set; } = "";
}
