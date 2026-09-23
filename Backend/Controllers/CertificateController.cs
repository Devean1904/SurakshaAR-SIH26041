using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using SurakshaAR.Backend.Services;
using SurakshaAR.Shared;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SurakshaAR.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CertificateController : ControllerBase
    {
        private readonly IMongoCollection<BsonDocument> _certificates;
        private readonly IMongoCollection<BsonDocument> _assessments;
        private readonly IMongoCollection<BsonDocument> _attempts;
        private readonly IMongoCollection<BsonDocument> _modules;
        private readonly BlockchainService _blockchain;
        private readonly AuthService _auth;
        private readonly IConfiguration _config;

        public CertificateController(
            MongoService mongo,
            IConfiguration config,
            BlockchainService blockchain,
            AuthService auth)
        {
            _certificates = mongo.RawCollection("certificates");
            _assessments = mongo.RawCollection("assessments");
            _attempts = mongo.RawCollection("training_attempts");
            _modules = mongo.RawCollection("training_modules");
            _blockchain = blockchain;
            _auth = auth;
            _config = config;
        }

        private string GetCallerId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        private string GetCallerRole() => User.FindFirst(ClaimTypes.Role)?.Value ?? "";
        private string GetCompanyId() => User.FindFirst("companyId")?.Value ?? "";

        private bool TryResolveScore(string employeeId, string moduleId, out int score, out bool passed)
        {
            score = 0;
            passed = false;

            var assessmentFilter = Builders<BsonDocument>.Filter.Eq("employeeId", employeeId)
                & Builders<BsonDocument>.Filter.Eq("moduleId", moduleId);
            var assessment = _assessments.Find(assessmentFilter)
                .SortByDescending(a => a["submittedAt"]).FirstOrDefault();

            BsonDocument? source = assessment;
            if (source == null)
            {
                var attemptFilter = Builders<BsonDocument>.Filter.Eq("employeeId", employeeId)
                    & Builders<BsonDocument>.Filter.Eq("moduleId", moduleId)
                    & Builders<BsonDocument>.Filter.Eq("status", "completed");
                source = _attempts.Find(attemptFilter)
                    .SortByDescending(a => a["endTime"]).FirstOrDefault();
            }

            if (source == null) return false;

            score = source.Contains("totalScore") ? source["totalScore"].AsInt32 : 0;

            var module = _modules.Find(Builders<BsonDocument>.Filter.Eq("moduleId", moduleId)).FirstOrDefault();
            float threshold = 70f;
            if (module != null && module.Contains("passThreshold"))
            {
                var t = (float)module["passThreshold"].ToDouble();
                if (t > 0f) threshold = t;
            }

            // Always enforce module threshold; honor stored auto-fail (passed=false).
            bool storedPassed = !source.Contains("passed") || source["passed"].AsBoolean;
            passed = storedPassed && score >= threshold;
            return true;
        }

        [HttpPost("generate")]
        [Authorize(Policy = "AuthenticatedUser")]
        public async Task<IActionResult> GenerateCertificate([FromBody] CertificateGenerateRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.EmployeeId) || string.IsNullOrWhiteSpace(request.ModuleId) || string.IsNullOrWhiteSpace(request.EmployeeName) || string.IsNullOrWhiteSpace(request.ModuleName))
                return BadRequest(new { message = "EmployeeId, EmployeeName, ModuleId, and ModuleName are required" });

            var callerRole = GetCallerRole();
            var companyId = GetCompanyId();
            if (string.Equals(callerRole, "Worker", StringComparison.Ordinal))
            {
                if (!string.Equals(request.EmployeeId, GetCallerId(), StringComparison.Ordinal))
                    return Forbid();
            }
            else
            {
                if (string.IsNullOrEmpty(companyId)) return BadRequest(new { message = "Company context missing" });
                var target = await _auth.GetUserInCompanyAsync(request.EmployeeId, companyId);
                if (target == null) return Forbid();
            }

            if (!TryResolveScore(request.EmployeeId, request.ModuleId, out var score, out var passed))
                return BadRequest(new { message = "No completed assessment or training found for this employee/module" });

            if (!passed)
                return BadRequest(new { message = "Certificate requires a passing score (verified server-side)" });

            string certId = $"CERT-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";
            string dataToSign = $"{certId}:{request.EmployeeId}:{request.ModuleId}:{score}:{DateTime.UtcNow:O}";
            string signature = SignData(dataToSign);
            string hash = ComputeSHA256(dataToSign);

            string blockchainHash = _blockchain.RecordCertificate(certId, request.EmployeeId, request.ModuleId, score);

            var doc = new BsonDocument
            {
                { "certificateId", certId },
                { "employeeId", request.EmployeeId },
                { "employeeName", request.EmployeeName },
                { "moduleId", request.ModuleId },
                { "moduleName", request.ModuleName },
                { "companyId", companyId },
                { "score", score },
                { "passed", passed },
                { "issuedAt", DateTime.UtcNow.ToString("O") },
                { "signature", signature },
                { "qrPayload", $"SURAKSHA:{certId}:{hash}" },
                { "blockchainTx", blockchainHash }
            };
            _certificates.InsertOne(doc);

            return Ok(new
            {
                CertificateId = certId,
                EmployeeId = request.EmployeeId,
                ModuleId = request.ModuleId,
                Score = score,
                Passed = passed,
                Signature = signature,
                BlockchainTx = blockchainHash,
                QrPayload = $"SURAKSHA:{certId}:{hash}",
                IssuedAt = DateTime.UtcNow.ToString("O")
            });
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "AuthenticatedUser")]
        public IActionResult GetCertificate(string id)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("certificateId", id);
            var cert = _certificates.Find(filter).FirstOrDefault();
            if (cert == null) return NotFound(new { message = "Certificate not found" });

            var employeeId = cert.Contains("employeeId") ? cert["employeeId"].AsString : "";
            var certCompany = cert.Contains("companyId") ? cert["companyId"].AsString : "";
            var callerRole = GetCallerRole();
            var companyId = GetCompanyId();

            if (string.Equals(callerRole, "Worker", StringComparison.Ordinal))
            {
                if (!string.Equals(employeeId, GetCallerId(), StringComparison.Ordinal))
                    return Forbid();
            }
            else
            {
                if (string.IsNullOrEmpty(companyId)) return Forbid();
                if (!string.Equals(certCompany, companyId, StringComparison.Ordinal))
                    return Forbid();
            }

            return Ok(cert);
        }

        [HttpGet("verify/{qrPayload}")]
        [AllowAnonymous]
        public IActionResult VerifyCertificate(string qrPayload)
        {
            try
            {
                var searchPayload = qrPayload.StartsWith("SURAKSHA:") ? qrPayload : $"SURAKSHA:{qrPayload}";
                var filter = Builders<BsonDocument>.Filter.Eq("qrPayload", searchPayload);
                var cert = _certificates.Find(filter).FirstOrDefault();
                if (cert == null)
                    return Ok(new { valid = false, message = "Certificate not found" });

                return Ok(new
                {
                    valid = true,
                    certificateId = cert["certificateId"],
                    employeeName = cert["employeeName"],
                    moduleName = cert["moduleName"],
                    score = cert["score"],
                    issuedAt = cert["issuedAt"]
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Certificate] verify error: {ex.Message}");
                return StatusCode(500, new { error = "Verification failed" });
            }
        }

        [HttpPost("blockchain-anchor")]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult AnchorToBlockchain([FromBody] BlockchainAnchorRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.CertificateId))
                return BadRequest(new { message = "CertificateId is required" });

            var companyId = GetCompanyId();
            if (string.IsNullOrEmpty(companyId))
                return BadRequest(new { message = "Company context missing" });

            var filter = Builders<BsonDocument>.Filter.Eq("certificateId", request.CertificateId)
                & Builders<BsonDocument>.Filter.Eq("companyId", companyId);
            var cert = _certificates.Find(filter).FirstOrDefault();
            if (cert == null) return NotFound(new { message = "Certificate not found in your company" });

            string existing = cert.Contains("blockchainTx") ? cert["blockchainTx"].AsString : "";
            string txHash = string.IsNullOrEmpty(existing) || existing.StartsWith("LOCAL-")
                ? _blockchain.RecordCertificate(
                    cert["certificateId"].AsString,
                    cert["employeeId"].AsString,
                    cert["moduleId"].AsString,
                    cert.Contains("score") ? cert["score"].AsInt32 : 0)
                : existing;

            var update = Builders<BsonDocument>.Update.Set("blockchainTx", txHash);
            _certificates.UpdateOne(filter, update);

            return Ok(new
            {
                txHash,
                chain = "local",
                message = "Certificate hash recorded on the local hash chain (not a public blockchain)"
            });
        }

        string SignData(string data)
        {
            var key = _config["Jwt:Key"] ?? "SurakshaAR_SuperSecret_Key_2026!";
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return Convert.ToBase64String(hashBytes);
        }

        string ComputeSHA256(string data)
        {
            using var sha = SHA256.Create();
            var hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(data));
            var sb = new StringBuilder();
            foreach (byte b in hashBytes)
                sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }

    public class CertificateGenerateRequest
    {
        public string EmployeeId { get; set; } = "";
        public string EmployeeName { get; set; } = "";
        public string ModuleId { get; set; } = "";
        public string ModuleName { get; set; } = "";
    }

    public class BlockchainAnchorRequest
    {
        public string CertificateId { get; set; } = "";
    }
}
