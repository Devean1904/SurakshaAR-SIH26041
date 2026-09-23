using MongoDB.Driver;
using MongoDB.Bson;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using SurakshaAR.Shared;

namespace SurakshaAR.Backend.Services;

public class AuthService
{
    private readonly MongoService _mongo;
    private readonly IConfiguration _config;

    public AuthService(MongoService mongo, IConfiguration config)
    {
        _mongo = mongo;
        _config = config;
    }

    private static string NormalizeCompanyId(string companyId)
        => companyId.Trim().ToUpperInvariant();

    private static string GenerateCompanyId()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var bytes = Guid.NewGuid().ToByteArray();
        var sb = new System.Text.StringBuilder(8);
        for (int i = 0; i < 8; i++)
            sb.Append(chars[bytes[i % bytes.Length] % chars.Length]);
        return sb.ToString();
    }

    private async Task<Company?> FindCompanyAsync(string companyId)
    {
        var companies = _mongo.Collection<Company>("companies");
        var id = NormalizeCompanyId(companyId);
        return await companies.Find(c => c.Id == id && c.IsActive).FirstOrDefaultAsync();
    }

    public async Task<AuthResponse> LoginByIdAsync(string userId, string password, string? companyId = null)
    {
        var users = _mongo.Collection<User>("users");
        var user = await users.Find(u => u.UserId == userId && u.IsActive).FirstOrDefaultAsync();
        if (user == null)
            return new AuthResponse { Success = false, Message = "User not found" };

        if (user.AwaitingAdminConfirmation)
            return new AuthResponse { Success = false, Message = "Account pending admin confirmation" };

        if (!string.IsNullOrWhiteSpace(companyId))
        {
            var company = await FindCompanyAsync(companyId);
            if (company == null)
                return new AuthResponse { Success = false, Message = "Company ID not found" };
            if (!string.Equals(user.CompanyId, company.Id, StringComparison.Ordinal))
                return new AuthResponse { Success = false, Message = "User does not belong to this Company ID" };
        }

        if (string.IsNullOrEmpty(user.PasswordHash))
            return new AuthResponse { Success = false, Message = "Invalid password" };

        bool passwordOk;
        try
        {
            passwordOk = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        }
        catch
        {
            passwordOk = false;
        }
        if (!passwordOk)
            return new AuthResponse { Success = false, Message = "Invalid password" };

        return new AuthResponse
        {
            Success = true,
            Token = GenerateJwt(user),
            UserId = user.UserId,
            Role = user.Role,
            Name = user.Name,
            Theme = user.PreferredTheme,
            Language = user.Language,
            CompanyId = user.CompanyId,
            Message = "Login successful"
        };
    }

    public async Task<AuthResponse> LoginByOtpAsync(string phone, string otp, string? companyId = null)
    {
        var users = _mongo.Collection<User>("users");
        var user = await users.Find(u => u.PhoneNumber == phone && u.IsActive).FirstOrDefaultAsync();
        if (user == null)
            return new AuthResponse { Success = false, Message = "Phone number not registered" };

        if (user.AwaitingAdminConfirmation)
            return new AuthResponse { Success = false, Message = "Account pending admin confirmation" };

        if (!string.IsNullOrWhiteSpace(companyId))
        {
            var company = await FindCompanyAsync(companyId);
            if (company == null)
                return new AuthResponse { Success = false, Message = "Company ID not found" };
            if (!string.Equals(user.CompanyId, company.Id, StringComparison.Ordinal))
                return new AuthResponse { Success = false, Message = "User does not belong to this Company ID" };
        }

        return new AuthResponse
        {
            Success = true,
            Token = GenerateJwt(user),
            UserId = user.UserId,
            Role = user.Role,
            Name = user.Name,
            Theme = user.PreferredTheme,
            Language = user.Language,
            CompanyId = user.CompanyId,
            Message = "OTP login successful"
        };
    }

    public async Task<bool> RegisterManagerAsync(string adminId, User manager, string password)
    {
        var users = _mongo.Collection<User>("users");
        var admin = await users.Find(u => u.UserId == adminId && u.Role == "Admin" && u.IsActive).FirstOrDefaultAsync();
        if (admin == null) return false;

        var companyId = admin.CompanyId;
        if (string.IsNullOrEmpty(companyId)) return false;

        var exists = await users.Find(u => u.UserId == manager.UserId).AnyAsync();
        if (exists) return false;

        manager.Id = Guid.NewGuid().ToString("N");
        manager.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
        manager.Role = "Manager";
        manager.ManagedByAdminId = adminId;
        manager.CompanyId = companyId;
        manager.IsActive = true;
        manager.CreatedAt = DateTime.UtcNow;
        await users.InsertOneAsync(manager);
        return true;
    }

    public async Task<(bool Ok, string CompanyId, string Error)> CreateCompanyWithAdminAsync(
        string? requestedCompanyId, string companyName, string adminUserId, string adminName, string password)
    {
        if (string.IsNullOrWhiteSpace(companyName))
            return (false, "", "Company name is required");
        if (string.IsNullOrWhiteSpace(adminUserId) || string.IsNullOrWhiteSpace(adminName) || string.IsNullOrWhiteSpace(password))
            return (false, "", "Admin UserId, Name, and Password are required");

        var users = _mongo.Collection<User>("users");
        var companies = _mongo.Collection<Company>("companies");

        if (await users.Find(u => u.UserId == adminUserId).AnyAsync())
            return (false, "", "UserId may already exist");

        var companyId = string.IsNullOrWhiteSpace(requestedCompanyId)
            ? GenerateCompanyId()
            : NormalizeCompanyId(requestedCompanyId);

        if (companyId.Length is < 3 or > 32)
            return (false, "", "Company ID must be 3-32 characters");
        if (!companyId.All(char.IsLetterOrDigit))
            return (false, "", "Company ID may only contain letters and numbers");

        if (await companies.Find(c => c.Id == companyId).AnyAsync())
            return (false, "", "Company ID already exists");

        var company = new Company
        {
            Id = companyId,
            Name = companyName.Trim(),
            AdminId = adminUserId,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        await companies.InsertOneAsync(company);

        var admin = new User
        {
            Id = Guid.NewGuid().ToString("N"),
            UserId = adminUserId,
            Name = adminName,
            Role = "Admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            CompanyId = companyId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        await users.InsertOneAsync(admin);
        return (true, companyId, "");
    }

    public async Task<bool> RegisterAdminUnderCompanyAsync(string callerAdminId, string userId, string name, string password)
    {
        var users = _mongo.Collection<User>("users");
        var admin = await users.Find(u => u.UserId == callerAdminId && u.Role == "Admin" && u.IsActive).FirstOrDefaultAsync();
        if (admin == null) return false;

        var companyId = admin.CompanyId;
        if (string.IsNullOrEmpty(companyId)) return false;

        if (await users.Find(u => u.UserId == userId).AnyAsync()) return false;

        await users.InsertOneAsync(new User
        {
            Id = Guid.NewGuid().ToString("N"),
            UserId = userId,
            Name = name,
            Role = "Admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            CompanyId = companyId,
            ManagedByAdminId = callerAdminId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });
        return true;
    }

    public async Task<Company?> GetCompanyAsync(string companyId)
        => await FindCompanyAsync(companyId);

    public async Task<bool> AdminExistsAsync(string? companyId = null)
    {
        var users = _mongo.Collection<User>("users");
        var filter = Builders<User>.Filter.Eq(u => u.Role, "Admin") & Builders<User>.Filter.Eq(u => u.IsActive, true);
        if (!string.IsNullOrEmpty(companyId))
            filter &= Builders<User>.Filter.Eq(u => u.CompanyId, NormalizeCompanyId(companyId));
        return await users.Find(filter).AnyAsync();
    }

    public async Task<bool> RegisterWorkerAsync(string managerId, User worker, string password)
    {
        var users = _mongo.Collection<User>("users");
        var manager = await users.Find(u => u.UserId == managerId && u.Role == "Manager" && u.IsActive).FirstOrDefaultAsync();
        if (manager == null) return false;

        var companyId = manager.CompanyId;
        if (string.IsNullOrEmpty(companyId)) return false;

        var exists = await users.Find(u => u.UserId == worker.UserId).AnyAsync();
        if (exists) return false;

        worker.Id = Guid.NewGuid().ToString("N");
        worker.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
        worker.Role = "Worker";
        worker.EnrolledByManagerId = managerId;
        worker.CompanyId = companyId;
        worker.AwaitingAdminConfirmation = true;
        worker.CreatedAt = DateTime.UtcNow;
        await users.InsertOneAsync(worker);

        var update = Builders<User>.Update.Push(u => u.AssignedWorkerIds, worker.UserId);
        await users.UpdateOneAsync(u => u.Id == manager.Id, update);
        return true;
    }

    public async Task<bool> ConfirmWorkerAsync(string workerId, string? companyId = null)
    {
        if (string.IsNullOrEmpty(companyId)) return false;
        var users = _mongo.Collection<User>("users");
        var update = Builders<User>.Update.Set(u => u.AwaitingAdminConfirmation, false);
        var filter = Builders<User>.Filter.Eq(u => u.UserId, workerId)
            & Builders<User>.Filter.Eq(u => u.AwaitingAdminConfirmation, true)
            & Builders<User>.Filter.Eq(u => u.CompanyId, companyId);
        var result = await users.UpdateOneAsync(filter, update);
        return result.MatchedCount > 0;
    }

    public async Task<User?> GetUserInCompanyAsync(string userId, string companyId)
    {
        if (string.IsNullOrEmpty(companyId)) return null;
        var users = _mongo.Collection<User>("users");
        return await users.Find(u => u.UserId == userId && u.CompanyId == companyId && u.IsActive)
            .FirstOrDefaultAsync();
    }

    public async Task<List<User>> GetUsersByRoleAsync(string role, string? companyId = null)
    {
        if (string.IsNullOrEmpty(companyId)) return new List<User>();
        var users = _mongo.Collection<User>("users");
        var filter = Builders<User>.Filter.Eq(u => u.Role, role)
            & Builders<User>.Filter.Eq(u => u.IsActive, true)
            & Builders<User>.Filter.Eq(u => u.CompanyId, companyId);
        return await users.Find(filter).ToListAsync();
    }

    public async Task<bool> DeleteUserAsync(string userId, string? companyId = null)
    {
        if (string.IsNullOrEmpty(companyId)) return false;
        var users = _mongo.Collection<User>("users");
        var update = Builders<User>.Update.Set(u => u.IsActive, false);
        var filter = Builders<User>.Filter.Eq(u => u.UserId, userId)
            & Builders<User>.Filter.Eq(u => u.CompanyId, companyId);
        var result = await users.UpdateOneAsync(filter, update);
        if (result.MatchedCount == 0) return false;

        await users.UpdateOneAsync(
            Builders<User>.Filter.Eq(u => u.Role, "Manager") & Builders<User>.Filter.Eq(u => u.CompanyId, companyId),
            Builders<User>.Update.Pull(u => u.AssignedWorkerIds, userId));
        return true;
    }

    public async Task<long> DeleteUsersInCompanyAsync(string companyId)
    {
        if (string.IsNullOrEmpty(companyId)) return 0;
        var users = _mongo.Collection<User>("users");
        var result = await users.DeleteManyAsync(Builders<User>.Filter.Eq(u => u.CompanyId, companyId));
        return result.DeletedCount;
    }

    public async Task<long> CountCompaniesAsync()
    {
        var companies = _mongo.Collection<Company>("companies");
        return await companies.CountDocumentsAsync(Builders<Company>.Filter.Empty);
    }

    public async Task<User?> GetUserAsync(string userId)
    {
        var users = _mongo.Collection<User>("users");
        return await users.Find(u => u.UserId == userId && u.IsActive).FirstOrDefaultAsync();
    }

    public async Task<bool> UpdateThemeAsync(string userId, string theme)
    {
        var users = _mongo.Collection<User>("users");
        var update = Builders<User>.Update.Set(u => u.PreferredTheme, theme);
        var result = await users.UpdateOneAsync(u => u.UserId == userId && u.IsActive, update);
        return result.MatchedCount > 0;
    }

    public async Task<bool> UpdateLanguageAsync(string userId, string language)
    {
        var users = _mongo.Collection<User>("users");
        var update = Builders<User>.Update.Set(u => u.Language, language);
        var result = await users.UpdateOneAsync(u => u.UserId == userId && u.IsActive, update);
        return result.MatchedCount > 0;
    }

    private string GenerateJwt(User user)
    {
        var keyMaterial = _config["Jwt:Key"] ?? "SurakshaAR_SuperSecret_Key_2026!SecureEnough";
        if (keyMaterial.Length < 32)
            keyMaterial = keyMaterial.PadRight(32, '!');

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyMaterial));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("name", user.Name),
            new Claim("companyId", NormalizeCompanyId(user.CompanyId ?? ""))
        };

        if (!double.TryParse(
                _config["Jwt:ExpiryMinutes"],
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture,
                out var expiryMinutes) || expiryMinutes <= 0)
            expiryMinutes = 1440;

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
