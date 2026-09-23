using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using SurakshaAR.Backend.Services;

namespace SurakshaAR.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AssessmentController : ControllerBase
    {
        private readonly IMongoCollection<BsonDocument> _modules;
        private readonly IMongoCollection<BsonDocument> _assessments;
        private readonly MongoService _mongo;

        public AssessmentController(MongoService mongo)
        {
            _modules = mongo.RawCollection("training_modules");
            _assessments = mongo.RawCollection("assessments");
            _mongo = mongo;
        }

        [HttpGet("{moduleId}/questions")]
        [Authorize(Policy = "AuthenticatedUser")]
        public IActionResult GetQuestions(string moduleId)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("moduleId", moduleId);
            var module = _modules.Find(filter).FirstOrDefault();
            if (module == null) return NotFound(new { message = "Module not found" });

            if (!module.Contains("questions"))
                return Ok(new List<object>());

            var questions = new List<object>();
            foreach (var q in module["questions"].AsBsonArray)
            {
                var qd = q.AsBsonDocument;
                var opts = qd.Contains("options")
                    ? qd["options"].AsBsonArray.Select(o => o.AsString).ToList()
                    : new List<string>();

                questions.Add(new
                {
                    questionId = qd.Contains("questionId") ? qd["questionId"].AsString : "",
                    moduleId = moduleId,
                    questionText = qd.Contains("questionText") ? qd["questionText"].AsString : "",
                    options = opts,
                    scoreValue = qd.Contains("scoreValue") ? qd["scoreValue"].AsInt32 : 20,
                    difficulty = qd.Contains("difficulty") ? qd["difficulty"].AsString : "medium",
                    // Exposed so client can show immediate correct/incorrect feedback.
                    // Server still re-grades independently on submit.
                    correctAnswerIndex = qd.Contains("correctAnswerIndex") ? qd["correctAnswerIndex"].AsInt32 : -1
                });
            }

            return Ok(questions);
        }

        [HttpPost("submit")]
        [Authorize(Policy = "AuthenticatedUser")]
        public IActionResult SubmitAssessment([FromBody] AssessmentSubmitRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.EmployeeId) || string.IsNullOrWhiteSpace(request.ModuleId))
                return BadRequest(new { message = "EmployeeId and ModuleId are required" });

            var callerId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var callerRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var companyId = User.FindFirst("companyId")?.Value ?? "";
            if (string.Equals(callerRole, "Worker", StringComparison.Ordinal))
            {
                if (callerId != request.EmployeeId) return Forbid();
            }
            else
            {
                if (string.IsNullOrEmpty(companyId)) return Forbid();
                var users = _mongo.RawCollection("users");
                var target = users.Find(Builders<BsonDocument>.Filter.Eq("userId", request.EmployeeId)
                    & Builders<BsonDocument>.Filter.Eq("companyId", companyId)).FirstOrDefault();
                if (target == null) return Forbid();
            }

            var moduleFilter = Builders<BsonDocument>.Filter.Eq("moduleId", request.ModuleId);
            var module = _modules.Find(moduleFilter).FirstOrDefault();
            if (module == null) return NotFound(new { message = "Module not found" });

            request.ActionScore = Math.Clamp(request.ActionScore, 0, 100);

            int questionScore = request.QuestionScore;
            if (module.Contains("questions") && request.Answers != null && request.Answers.Count > 0)
            {
                // Weighted by scoreValue and truncated — matches Android AssessmentActivity.
                var qs = module["questions"].AsBsonArray;
                int earned = 0;
                int maxPoints = 0;
                for (int i = 0; i < qs.Count && i < request.Answers.Count; i++)
                {
                    var qd = qs[i].AsBsonDocument;
                    if (!qd.Contains("correctAnswerIndex")) continue;
                    int sv = qd.Contains("scoreValue") ? qd["scoreValue"].AsInt32 : 20;
                    if (sv <= 0) sv = 20;
                    maxPoints += sv;
                    if (request.Answers[i] == qd["correctAnswerIndex"].AsInt32) earned += sv;
                }
                if (maxPoints > 0)
                    questionScore = (int)(100.0 * earned / maxPoints);
            }

            request.QuestionScore = Math.Clamp(questionScore, 0, 100);
            request.TotalScore = Math.Clamp((request.ActionScore + request.QuestionScore) / 2, 0, 100);

            float threshold = 70f;
            if (module.Contains("passThreshold"))
            {
                var t = (float)module["passThreshold"].ToDouble();
                if (t > 0f) threshold = t;
            }
            request.Passed = request.TotalScore >= threshold;

            var doc = new BsonDocument
            {
                { "assessmentId", Guid.NewGuid().ToString("N").Substring(0, 12).ToUpper() },
                { "employeeId", request.EmployeeId },
                { "moduleId", request.ModuleId },
                { "attemptId", request.AttemptId ?? "" },
                { "companyId", User.FindFirst("companyId")?.Value ?? "" },
                { "actionScore", request.ActionScore },
                { "questionScore", request.QuestionScore },
                { "totalScore", request.TotalScore },
                { "passed", request.Passed },
                { "answers", new BsonArray(request.Answers ?? new List<int>()) },
                { "submittedAt", DateTime.UtcNow.ToString("O") }
            };
            _assessments.InsertOne(doc);

            return Ok(new
            {
                assessmentId = doc["assessmentId"].AsString,
                totalScore = request.TotalScore,
                passed = request.Passed,
                message = request.Passed ? "Assessment passed" : "Assessment failed"
            });
        }
    }

    public class AssessmentSubmitRequest
    {
        public string EmployeeId { get; set; } = "";
        public string ModuleId { get; set; } = "";
        public string AttemptId { get; set; } = "";
        public int ActionScore { get; set; }
        public int QuestionScore { get; set; }
        public int TotalScore { get; set; }
        public bool Passed { get; set; }
        public List<int> Answers { get; set; } = new();
    }
}
