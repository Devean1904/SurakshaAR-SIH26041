using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SurakshaAR.Shared;
using SurakshaAR.Backend.Services;
using System.Security.Claims;

namespace SurakshaAR.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _auth;
    private readonly OtpService _otp;

    public AuthController(AuthService auth, OtpService otp)
    {
        _auth = auth;
        _otp = otp;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> LoginById([FromBody] LoginRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.UserId) || string.IsNullOrWhiteSpace(req.Password))
            return BadRequest(new AuthResponse { Success = false, Message = "UserId and Password are required" });

        var result = await _auth.LoginByIdAsync(req.UserId, req.Password, req.CompanyId?.Trim());
        if (!result.Success) return Unauthorized(result);
        return Ok(result);
    }

    [HttpPost("otp/send")]
    [AllowAnonymous]
    public async Task<IActionResult> SendOtp([FromBody] OtpRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.PhoneNumber))
            return BadRequest(new { message = "Phone number is required" });

        var ok = await _otp.GenerateOtpAsync(req.PhoneNumber);
        if (!ok)
            return StatusCode(503, new AuthResponse { Success = false, Message = "SMS service unavailable" });
        return Ok(new AuthResponse { Success = true, Message = "OTP sent" });
    }

    [HttpPost("otp/verify")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyOtp([FromBody] OtpVerifyRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.PhoneNumber) || string.IsNullOrWhiteSpace(req.Otp))
            return BadRequest(new { message = "Phone number and OTP are required" });

        if (!_otp.VerifyOtp(req.PhoneNumber, req.Otp))
            return Unauthorized(new AuthResponse { Success = false, Message = "Invalid or expired OTP" });

        var result = await _auth.LoginByOtpAsync(req.PhoneNumber, req.Otp, req.CompanyId?.Trim());
        if (!result.Success) return Unauthorized(result);
        return Ok(result);
    }

    [HttpPost("admin/create")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> CreateAdmin([FromBody] CreateAdminRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.UserId) || string.IsNullOrWhiteSpace(req.Name) || string.IsNullOrWhiteSpace(req.Password))
            return BadRequest(new { message = "UserId, Name, and Password are required" });

        var callerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        var result = await _auth.RegisterAdminUnderCompanyAsync(callerId, req.UserId.Trim(), req.Name.Trim(), req.Password);
        if (!result) return BadRequest(new { message = "Failed to create admin. UserId may already exist." });

        return Ok(new { message = "Admin created successfully", userId = req.UserId });
    }

    [HttpPost("admin/reset")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> ResetAllUsers()
    {
        var companyId = User.FindFirst("companyId")?.Value ?? "";
        if (string.IsNullOrEmpty(companyId))
            return BadRequest(new { message = "Company context missing" });

        var deleted = await _auth.DeleteUsersInCompanyAsync(companyId);
        return Ok(new { message = $"Deleted {deleted} users in company {companyId}" });
    }

    [HttpGet("admin/exists")]
    [AllowAnonymous]
    public async Task<IActionResult> AdminExists([FromQuery] string? companyId = null)
    {
        bool exists = await _auth.AdminExistsAsync(companyId);
        return Ok(new { exists, companyId = companyId ?? "" });
    }

    [HttpGet("user/{userId}")]
    [Authorize]
    public async Task<IActionResult> GetUser(string userId)
    {
        var callerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var callerRole = User.FindFirst(ClaimTypes.Role)?.Value;

        if (callerRole != "Admin" && callerId != userId)
            return Forbid();

        var user = await _auth.GetUserAsync(userId);
        if (user == null) return NotFound();

        if (callerRole == "Admin")
        {
            var callerCompanyId = User.FindFirst("companyId")?.Value ?? "";
            if (!string.Equals(user.CompanyId, callerCompanyId, StringComparison.Ordinal))
                return Forbid();
        }

        return Ok(new
        {
            success = true,
            user.UserId, user.Name, user.Role, user.PhoneNumber,
            theme = user.PreferredTheme, user.Language, user.CreatedAt, user.CompanyId
        });
    }

    [HttpPost("theme")]
    [Authorize]
    public async Task<IActionResult> UpdateTheme([FromBody] ThemeRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.UserId) || string.IsNullOrWhiteSpace(req.Theme))
            return BadRequest(new { message = "UserId and Theme are required" });

        var callerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var callerRole = User.FindFirst(ClaimTypes.Role)?.Value;
        var callerCompanyId = User.FindFirst("companyId")?.Value ?? "";

        if (callerId != req.UserId && callerRole != "Admin")
            return Forbid();

        if (callerRole == "Admin" && callerId != req.UserId)
        {
            var target = await _auth.GetUserInCompanyAsync(req.UserId, callerCompanyId);
            if (target == null) return Forbid();
        }

        var updated = await _auth.UpdateThemeAsync(req.UserId, req.Theme);
        if (!updated) return NotFound(new { message = "User not found" });
        return Ok(new { message = "Theme updated" });
    }

    [HttpPost("language")]
    [Authorize]
    public async Task<IActionResult> UpdateLanguage([FromBody] LanguageRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.UserId) || string.IsNullOrWhiteSpace(req.Language))
            return BadRequest(new { message = "UserId and Language are required" });

        var callerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var callerRole = User.FindFirst(ClaimTypes.Role)?.Value;
        var callerCompanyId = User.FindFirst("companyId")?.Value ?? "";

        if (callerId != req.UserId && callerRole != "Admin")
            return Forbid();

        if (callerRole == "Admin" && callerId != req.UserId)
        {
            var target = await _auth.GetUserInCompanyAsync(req.UserId, callerCompanyId);
            if (target == null) return Forbid();
        }

        var updated = await _auth.UpdateLanguageAsync(req.UserId, req.Language);
        if (!updated) return NotFound(new { message = "User not found" });
        return Ok(new { message = "Language updated" });
    }
}

public class ThemeRequest { public string UserId { get; set; } = ""; public string Theme { get; set; } = "Dark"; }
public class LanguageRequest { public string UserId { get; set; } = ""; public string Language { get; set; } = "en"; }
public class CreateAdminRequest { public string UserId { get; set; } = ""; public string Name { get; set; } = ""; public string Password { get; set; } = ""; }
