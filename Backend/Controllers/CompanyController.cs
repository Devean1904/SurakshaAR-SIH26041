using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SurakshaAR.Backend.Services;
using SurakshaAR.Shared;
using System.Security.Claims;

namespace SurakshaAR.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CompanyController : ControllerBase
{
    private readonly AuthService _auth;
    private readonly IConfiguration Configuration;

    public CompanyController(AuthService auth, IConfiguration configuration)
    {
        _auth = auth;
        Configuration = configuration;
    }

    private string GetCompanyId() => User.FindFirst("companyId")?.Value ?? "";

    [HttpPost("create")]
    [AllowAnonymous]
    public async Task<IActionResult> CreateCompany([FromBody] CreateCompanyRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.CompanyName) ||
            string.IsNullOrWhiteSpace(req.AdminUserId) ||
            string.IsNullOrWhiteSpace(req.AdminName) ||
            string.IsNullOrWhiteSpace(req.AdminPassword))
            return BadRequest(new { message = "CompanyName, AdminUserId, AdminName, and AdminPassword are required" });

        var companyCount = await _auth.CountCompaniesAsync();
        var isAdmin = User.Identity?.IsAuthenticated == true
            && User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value == "Admin";
        var bootstrapSecret = HttpContext.Request.Headers["X-Bootstrap-Secret"].FirstOrDefault();
        var expectedSecret = Configuration["Bootstrap:Secret"];
        var hasSecretConfigured = !string.IsNullOrEmpty(expectedSecret);
        var secretOk = hasSecretConfigured
            && !string.IsNullOrEmpty(bootstrapSecret)
            && string.Equals(bootstrapSecret, expectedSecret, StringComparison.Ordinal);

        // Anonymous bootstrap only when no secret is configured and no companies exist yet.
        // Once a Bootstrap:Secret is set, only that secret or an Admin may create companies.
        var allowed = isAdmin || secretOk || (!hasSecretConfigured && companyCount == 0);

        if (!allowed)
            return StatusCode(403, new { message = "Company creation is restricted after bootstrap" });

        var (ok, companyId, error) = await _auth.CreateCompanyWithAdminAsync(
            string.IsNullOrWhiteSpace(req.CompanyId) ? null : req.CompanyId.Trim(),
            req.CompanyName.Trim(),
            req.AdminUserId.Trim(),
            req.AdminName.Trim(),
            req.AdminPassword);

        if (!ok) return BadRequest(new { message = error });

        return Ok(new
        {
            message = "Company created with admin",
            companyId,
            adminUserId = req.AdminUserId.Trim()
        });
    }

    [HttpGet("current")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> GetCurrentCompany()
    {
        var companyId = GetCompanyId();
        if (string.IsNullOrEmpty(companyId))
            return NotFound(new { message = "Company not found for this admin" });

        var company = await _auth.GetCompanyAsync(companyId);
        if (company == null)
            return NotFound(new { message = "Company not found" });

        return Ok(new
        {
            company.Id,
            company.Name,
            company.AdminId,
            company.CreatedAt,
            company.IsActive
        });
    }

    [HttpGet("exists")]
    [AllowAnonymous]
    public async Task<IActionResult> CompanyExists([FromQuery] string companyId)
    {
        if (string.IsNullOrWhiteSpace(companyId))
            return BadRequest(new { message = "companyId query parameter is required" });

        var company = await _auth.GetCompanyAsync(companyId);
        return Ok(new { exists = company != null, companyId = company?.Id ?? "" });
    }
}
