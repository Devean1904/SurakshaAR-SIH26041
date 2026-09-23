using MongoDB.Driver;
using SurakshaAR.Shared;

namespace SurakshaAR.Backend.Services;

public class EscalationService
{
    private readonly MongoService _mongo;
    private readonly BlockchainService _blockchain;

    public EscalationService(MongoService mongo, BlockchainService blockchain)
    {
        _mongo = mongo;
        _blockchain = blockchain;
    }

    public async Task<EscalationReport> ReportEscalationAsync(EscalationReport report)
    {
        report.Id = Guid.NewGuid().ToString("N");
        report.ReportedAt = DateTime.UtcNow;
        report.Status = "Active";
        report.BlockchainHash = _blockchain.RecordEvent(
            "escalation", report.WorkerId, report.Description);
        await _mongo.Collection<EscalationReport>("escalations").InsertOneAsync(report);
        return report;
    }

    public async Task<List<EscalationReport>> GetEscalationsForCompanyAsync(string companyId)
    {
        if (string.IsNullOrEmpty(companyId)) return new List<EscalationReport>();
        return await _mongo.Collection<EscalationReport>("escalations")
            .Find(e => e.CompanyId == companyId)
            .SortByDescending(e => e.ReportedAt).ToListAsync();
    }

    public async Task<List<EscalationReport>> GetEscalationsForWorkerAsync(string workerId)
    {
        if (string.IsNullOrEmpty(workerId)) return new List<EscalationReport>();
        return await _mongo.Collection<EscalationReport>("escalations")
            .Find(e => e.WorkerId == workerId)
            .SortByDescending(e => e.ReportedAt).ToListAsync();
    }

    public async Task<bool> ResolveEscalationAsync(string reportId, string companyId)
    {
        if (string.IsNullOrEmpty(companyId)) return false;
        var update = Builders<EscalationReport>.Update.Set(e => e.Status, "Resolved");
        var result = await _mongo.Collection<EscalationReport>("escalations")
            .UpdateOneAsync(e => e.Id == reportId && e.CompanyId == companyId, update);
        return result.MatchedCount > 0;
    }

    public async Task<bool> AssignEscalationAsync(string reportId, string assignedTo, string companyId)
    {
        if (string.IsNullOrEmpty(companyId)) return false;
        var update = Builders<EscalationReport>.Update.Set(e => e.AssignedTo, assignedTo);
        var result = await _mongo.Collection<EscalationReport>("escalations")
            .UpdateOneAsync(e => e.Id == reportId && e.CompanyId == companyId, update);
        return result.MatchedCount > 0;
    }
}
