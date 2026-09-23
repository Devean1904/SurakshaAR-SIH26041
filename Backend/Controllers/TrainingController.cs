using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using SurakshaAR.Backend.Services;

namespace SurakshaAR.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TrainingController : ControllerBase
    {
        private readonly IMongoCollection<BsonDocument> _modules;
        private readonly IMongoCollection<BsonDocument> _attempts;
        private readonly IMongoCollection<BsonDocument> _users;

        public TrainingController(MongoService mongo)
        {
            _modules = mongo.RawCollection("training_modules");
            _attempts = mongo.RawCollection("training_attempts");
            _users = mongo.RawCollection("users");
        }

        [HttpGet("modules")]
        [Authorize(Policy = "AuthenticatedUser")]
        public IActionResult GetModules()
        {
            var modules = _modules.Find(new BsonDocument()).ToList();
            var result = modules.Select(m => MapModule(m, includeAnswers: false)).ToList();
            return Ok(result);
        }

        [HttpGet("modules/{id}")]
        [Authorize(Policy = "AuthenticatedUser")]
        public IActionResult GetModule(string id)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("moduleId", id);
            var m = _modules.Find(filter).FirstOrDefault();
            if (m == null) return NotFound(new { message = "Module not found" });
            return Ok(MapModule(m, includeAnswers: false));
        }

        private object MapModule(BsonDocument m, bool includeAnswers)
        {
            var scenarios = new List<object>();
            if (m.Contains("scenarios"))
            {
                foreach (var s in m["scenarios"].AsBsonArray)
                {
                    var sd = s.AsBsonDocument;

                    var hazardZones = new List<object>();
                    if (sd.Contains("hazardZones"))
                    {
                        foreach (var z in sd["hazardZones"].AsBsonArray)
                        {
                            var zd = z.AsBsonDocument;
                            var posArray = zd.Contains("position") ? zd["position"].AsBsonArray.Select(p => (float)p.ToDouble()).ToArray() : new float[3];
                            hazardZones.Add(new
                            {
                                zoneId = zd.Contains("zoneId") ? zd["zoneId"].AsString : "",
                                zoneName = zd.Contains("zoneName") ? zd["zoneName"].AsString : "",
                                riskColor = zd.Contains("riskColor") ? zd["riskColor"].AsString : "#FF0000",
                                position = posArray,
                                radius = zd.Contains("radius") ? (float)zd["radius"].ToDouble() : 1f,
                                dangerLevel = zd.Contains("dangerLevel") ? (float)zd["dangerLevel"].ToDouble() : 0.5f
                            });
                        }
                    }

                    var requiredActions = new List<object>();
                    if (sd.Contains("requiredActions"))
                    {
                        foreach (var a in sd["requiredActions"].AsBsonArray)
                        {
                            var ad = a.AsBsonDocument;
                            requiredActions.Add(new
                            {
                                actionId = ad.Contains("actionId") ? ad["actionId"].AsString : "",
                                actionName = ad.Contains("actionName") ? ad["actionName"].AsString : "",
                                description = ad.Contains("description") ? ad["description"].AsString : "",
                                completionFeedback = ad.Contains("completionFeedback") ? ad["completionFeedback"].AsString : "",
                                interactionRadius = ad.Contains("interactionRadius") ? (float)ad["interactionRadius"].ToDouble() : 2f,
                                timeLimitSeconds = ad.Contains("timeLimitSeconds") ? (float)ad["timeLimitSeconds"].ToDouble() : 15f,
                                scoreValue = ad.Contains("scoreValue") ? ad["scoreValue"].AsInt32 : 20,
                                isCritical = ad.Contains("isCritical") && ad["isCritical"].ToBoolean()
                            });
                        }
                    }

                    var escalationEvents = new List<object>();
                    if (sd.Contains("escalationEvents"))
                    {
                        foreach (var e in sd["escalationEvents"].AsBsonArray)
                        {
                            var ed = e.AsBsonDocument;
                            escalationEvents.Add(new
                            {
                                level = ed.Contains("level") ? ed["level"].AsInt32 : 0,
                                levelName = ed.Contains("levelName") ? ed["levelName"].AsString : "",
                                timeThreshold = ed.Contains("timeThreshold") ? (float)ed["timeThreshold"].ToDouble() : 0f,
                                scorePenalty = ed.Contains("scorePenalty") ? (float)ed["scorePenalty"].ToDouble() : 0f,
                                visualEffect = ed.Contains("visualEffect") ? ed["visualEffect"].AsString : "",
                                audioEffect = ed.Contains("audioEffect") ? ed["audioEffect"].AsString : "",
                                warningText = ed.Contains("warningText") ? ed["warningText"].AsString : "",
                                showEvacuationArrows = ed.Contains("showEvacuationArrows") && ed["showEvacuationArrows"].ToBoolean(),
                                triggerAutoFail = ed.Contains("triggerAutoFail") && ed["triggerAutoFail"].ToBoolean(),
                                hazardGrowthMultiplier = ed.Contains("hazardGrowthMultiplier") ? (float)ed["hazardGrowthMultiplier"].ToDouble() : 1f
                            });
                        }
                    }

                    scenarios.Add(new
                    {
                        scenarioId = sd["scenarioId"].AsString,
                        moduleId = sd["moduleId"].AsString,
                        hazardType = sd["hazardType"].AsString,
                        scenarioName = sd["scenarioName"].AsString,
                        initialDangerLevel = sd["initialDangerLevel"].ToDouble(),
                        escalationIntervalSeconds = sd.Contains("escalationIntervalSeconds") ? (float)sd["escalationIntervalSeconds"].ToDouble() : 15f,
                        maxEscalationLevel = sd["maxEscalationLevel"].AsInt32,
                        hazardZones,
                        requiredActions,
                        escalationEvents
                    });
                }
            }

            var questions = new List<object>();
            if (m.Contains("questions"))
            {
                foreach (var q in m["questions"].AsBsonArray)
                {
                    var qd = q.AsBsonDocument;
                    var opts = qd["options"].AsBsonArray.Select(o => o.AsString).ToList();
                    if (includeAnswers)
                    {
                        questions.Add(new
                        {
                            questionId = qd["questionId"].AsString,
                            moduleId = qd["moduleId"].AsString,
                            questionText = qd["questionText"].AsString,
                            options = opts,
                            correctAnswerIndex = qd.Contains("correctAnswerIndex") ? qd["correctAnswerIndex"].AsInt32 : 0,
                            scoreValue = qd["scoreValue"].AsInt32,
                            explanation = qd.Contains("explanation") ? qd["explanation"].AsString : "",
                            difficulty = qd.Contains("difficulty") ? qd["difficulty"].AsString : "medium"
                        });
                    }
                    else
                    {
                        questions.Add(new
                        {
                            questionId = qd["questionId"].AsString,
                            moduleId = qd["moduleId"].AsString,
                            questionText = qd["questionText"].AsString,
                            options = opts,
                            scoreValue = qd["scoreValue"].AsInt32,
                            difficulty = qd.Contains("difficulty") ? qd["difficulty"].AsString : "medium"
                        });
                    }
                }
            }

            var equipment = new List<string>();
            if (m.Contains("requiredEquipment"))
            {
                foreach (var e in m["requiredEquipment"].AsBsonArray)
                    equipment.Add(e.AsString);
            }

            return new
            {
                moduleId = m.Contains("moduleId") ? m["moduleId"].AsString : "",
                title = m.Contains("title") ? m["title"].AsString : "",
                description = m.Contains("description") ? m["description"].AsString : "",
                safetyDomain = m.Contains("safetyDomain") ? m["safetyDomain"].AsString : "",
                difficultyLevel = m.Contains("difficultyLevel") ? m["difficultyLevel"].AsInt32 : 1,
                timeLimitSeconds = m.Contains("timeLimitSeconds") ? m["timeLimitSeconds"].ToDouble() : 120,
                passThreshold = m.Contains("passThreshold") && m["passThreshold"].ToDouble() > 0
                    ? m["passThreshold"].ToDouble()
                    : 70,
                scenarios,
                questions,
                requiredEquipment = equipment
            };
        }

        [HttpPost("start")]
        [Authorize(Policy = "AuthenticatedUser")]
        public IActionResult StartTraining([FromBody] TrainingStartRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.EmployeeId) || string.IsNullOrWhiteSpace(request.ModuleId) || string.IsNullOrWhiteSpace(request.ScenarioId))
                return BadRequest(new { message = "EmployeeId, ModuleId, and ScenarioId are required" });

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
                var target = _users.Find(Builders<BsonDocument>.Filter.Eq("userId", request.EmployeeId)
                    & Builders<BsonDocument>.Filter.Eq("companyId", companyId)).FirstOrDefault();
                if (target == null) return Forbid();
            }

            var doc = new BsonDocument
            {
                { "attemptId", Guid.NewGuid().ToString("N").Substring(0, 12).ToUpper() },
                { "employeeId", request.EmployeeId },
                { "moduleId", request.ModuleId },
                { "scenarioId", request.ScenarioId },
                { "companyId", companyId },
                { "startTime", DateTime.UtcNow.ToString("O") },
                { "status", "in_progress" }
            };
            _attempts.InsertOne(doc);
            return Ok(new { attemptId = doc["attemptId"].AsString, message = "Training started" });
        }

        [HttpPost("complete")]
        [Authorize(Policy = "AuthenticatedUser")]
        public IActionResult CompleteTraining([FromBody] TrainingCompleteRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.AttemptId))
                return BadRequest(new { message = "AttemptId is required" });

            var callerId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var callerRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var attemptFilter = Builders<BsonDocument>.Filter.Eq("attemptId", request.AttemptId);
            var attempt = _attempts.Find(attemptFilter).FirstOrDefault();
            if (attempt == null) return NotFound(new { message = "Attempt not found" });

            var employeeId = attempt.Contains("employeeId") ? attempt["employeeId"].AsString : "";
            if (string.Equals(callerRole, "Worker", StringComparison.Ordinal) && callerId != employeeId)
                return Forbid();
            if (!string.Equals(callerRole, "Worker", StringComparison.Ordinal))
            {
                var attemptCompany = attempt.Contains("companyId") ? attempt["companyId"].AsString : "";
                var callerCompany = User.FindFirst("companyId")?.Value ?? "";
                if (string.IsNullOrEmpty(callerCompany)
                    || !string.Equals(attemptCompany, callerCompany, StringComparison.Ordinal))
                    return Forbid();
            }

            request.ActionScore = Math.Clamp(request.ActionScore, 0, 100);
            request.QuestionScore = Math.Clamp(request.QuestionScore, 0, 100);
            request.TotalScore = Math.Clamp(request.TotalScore, 0, 100);
            request.EscalationLevel = Math.Clamp(request.EscalationLevel, 0, 4);

            var moduleId = attempt.Contains("moduleId") ? attempt["moduleId"].AsString : "";
            var threshold = GetPassThreshold(moduleId);
            // Pass only if score meets module threshold AND client did not mark auto-fail.
            request.Passed = request.Passed && request.TotalScore >= threshold;

            var update = Builders<BsonDocument>.Update
                .Set("endTime", DateTime.UtcNow.ToString("O"))
                .Set("actionScore", request.ActionScore)
                .Set("questionScore", request.QuestionScore)
                .Set("totalScore", request.TotalScore)
                .Set("passed", request.Passed)
                .Set("escalationLevelReached", request.EscalationLevel)
                .Set("status", "completed");
            var result = _attempts.UpdateOne(attemptFilter, update);
            if (result.MatchedCount == 0)
                return NotFound(new { message = "Attempt not found" });
            return Ok(new { message = "Training recorded", score = request.TotalScore, passed = request.Passed });
        }

        private float GetPassThreshold(string moduleId)
        {
            if (string.IsNullOrEmpty(moduleId)) return 70f;
            var m = _modules.Find(Builders<BsonDocument>.Filter.Eq("moduleId", moduleId)).FirstOrDefault();
            if (m != null && m.Contains("passThreshold"))
            {
                var t = (float)m["passThreshold"].ToDouble();
                if (t > 0f) return t;
            }
            return 70f;
        }

        [HttpGet("history/{employeeId}")]
        [Authorize(Policy = "AuthenticatedUser")]
        public IActionResult GetHistory(string employeeId)
        {
            var callerId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var callerRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            if (string.Equals(callerRole, "Worker", StringComparison.Ordinal) && callerId != employeeId)
                return Forbid();
            if (string.Equals(callerRole, "Admin", StringComparison.Ordinal) || string.Equals(callerRole, "Manager", StringComparison.Ordinal))
            {
                var callerCompany = User.FindFirst("companyId")?.Value ?? "";
                if (string.IsNullOrEmpty(callerCompany)) return Forbid();

                var companyFilter = Builders<BsonDocument>.Filter.Eq("employeeId", employeeId)
                    & Builders<BsonDocument>.Filter.Eq("companyId", callerCompany);
                if (!_attempts.Find(companyFilter).Any())
                {
                    var sample = _attempts.Find(Builders<BsonDocument>.Filter.Eq("employeeId", employeeId)).FirstOrDefault();
                    if (sample == null) return Ok(new List<object>());
                    var sampleCompany = sample.Contains("companyId") ? sample["companyId"].AsString : "";
                    if (!string.Equals(sampleCompany, callerCompany, StringComparison.Ordinal))
                        return Forbid();
                }
            }

            var filter = Builders<BsonDocument>.Filter.Eq("employeeId", employeeId);
            if (string.Equals(callerRole, "Admin", StringComparison.Ordinal) || string.Equals(callerRole, "Manager", StringComparison.Ordinal))
            {
                filter &= Builders<BsonDocument>.Filter.Eq("companyId", User.FindFirst("companyId")?.Value ?? "");
            }
            var attempts = _attempts.Find(filter).SortByDescending(x => x["startTime"]).Limit(50).ToList();
            var result = attempts.Select(a => new
            {
                attemptId = a.Contains("attemptId") ? a["attemptId"].AsString : "",
                employeeId = a.Contains("employeeId") ? a["employeeId"].AsString : "",
                moduleId = a.Contains("moduleId") ? a["moduleId"].AsString : "",
                scenarioId = a.Contains("scenarioId") ? a["scenarioId"].AsString : "",
                startTime = a.Contains("startTime") ? a["startTime"].AsString : "",
                endTime = a.Contains("endTime") ? a["endTime"].AsString : "",
                actionScore = a.Contains("actionScore") ? a["actionScore"].AsInt32 : 0,
                questionScore = a.Contains("questionScore") ? a["questionScore"].AsInt32 : 0,
                totalScore = a.Contains("totalScore") ? a["totalScore"].AsInt32 : 0,
                passed = a.Contains("passed") && a["passed"].AsBoolean,
                escalationLevel = a.Contains("escalationLevelReached") ? a["escalationLevelReached"].AsInt32 : 0,
                status = a.Contains("status") ? a["status"].AsString : ""
            }).ToList();
            return Ok(result);
        }
    }

    public class TrainingStartRequest
    {
        public string EmployeeId { get; set; } = "";
        public string ModuleId { get; set; } = "";
        public string ScenarioId { get; set; } = "";
    }

    public class TrainingCompleteRequest
    {
        public string AttemptId { get; set; } = "";
        public int ActionScore { get; set; }
        public int QuestionScore { get; set; }
        public int TotalScore { get; set; }
        public bool Passed { get; set; }
        public int EscalationLevel { get; set; }
    }
}
