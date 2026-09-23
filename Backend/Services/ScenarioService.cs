using MongoDB.Driver;
using SurakshaAR.Shared;

namespace SurakshaAR.Backend.Services;

public class ScenarioService
{
    private readonly MongoService _mongo;

    public ScenarioService(MongoService mongo) => _mongo = mongo;

    public async Task<List<SiteMapping>> GetSiteMappingsAsync(string adminId, string? companyId = null)
    {
        if (string.IsNullOrEmpty(companyId)) return new List<SiteMapping>();
        return await _mongo.Collection<SiteMapping>("site_mappings")
            .Find(s => s.CompanyId == companyId).ToListAsync();
    }

    public async Task<string> CreateSiteMappingAsync(SiteMapping mapping)
    {
        if (string.IsNullOrEmpty(mapping.CompanyId))
            throw new InvalidOperationException("CompanyId is required to create a site mapping");
        if (string.IsNullOrEmpty(mapping.Id))
            mapping.Id = Guid.NewGuid().ToString("N");
        await _mongo.Collection<SiteMapping>("site_mappings").InsertOneAsync(mapping);
        return mapping.Id;
    }

    public async Task<List<Scenario>> GetScenariosForWorkerAsync(string workerId, string? companyId = null)
    {
        if (string.IsNullOrEmpty(workerId)) return new List<Scenario>();
        var filter = Builders<Scenario>.Filter.Eq(s => s.WorkerId, workerId);
        if (!string.IsNullOrEmpty(companyId))
            filter &= Builders<Scenario>.Filter.Eq(s => s.CompanyId, companyId);
        return await _mongo.Collection<Scenario>("scenarios")
            .Find(filter).ToListAsync();
    }

    public async Task<string> AssignScenarioAsync(Scenario scenario)
    {
        if (string.IsNullOrEmpty(scenario.CompanyId))
            throw new InvalidOperationException("CompanyId is required to assign a scenario");
        if (string.IsNullOrEmpty(scenario.Id))
            scenario.Id = Guid.NewGuid().ToString("N");
        await _mongo.Collection<Scenario>("scenarios").InsertOneAsync(scenario);
        return scenario.Id;
    }

    public async Task<List<SiteMapping>> GetSiteMappingsByCompanyAsync(string companyId)
    {
        if (string.IsNullOrEmpty(companyId)) return new List<SiteMapping>();
        return await _mongo.Collection<SiteMapping>("site_mappings")
            .Find(s => s.CompanyId == companyId).ToListAsync();
    }

    public async Task<List<CitificationData>> GetCitificationDataAsync(string siteMappingId, string? companyId = null)
    {
        if (string.IsNullOrEmpty(companyId)) return new List<CitificationData>();
        var mappingFilter = Builders<SiteMapping>.Filter.Eq(s => s.Id, siteMappingId)
            & Builders<SiteMapping>.Filter.Eq(s => s.CompanyId, companyId);

        var mapping = await _mongo.Collection<SiteMapping>("site_mappings")
            .Find(mappingFilter).FirstOrDefaultAsync();
        if (mapping == null) return new List<CitificationData>();

        return await _mongo.Collection<Scenario>("scenarios")
            .Find(s => s.SiteMappingId == siteMappingId && s.CompanyId == companyId)
            .Project(s => s.Citification)
            .ToListAsync();
    }

    public async Task<List<SiteMapping>> GetSiteMappingsForWorkerAsync(string workerId)
    {
        var scenarios = await GetScenariosForWorkerAsync(workerId);
        var siteMappingIds = scenarios.Select(s => s.SiteMappingId).Distinct().ToList();
        if (siteMappingIds.Count == 0) return new List<SiteMapping>();

        return await _mongo.Collection<SiteMapping>("site_mappings")
            .Find(s => siteMappingIds.Contains(s.Id)).ToListAsync();
    }

    public async Task<Scenario?> GetScenarioAsync(string scenarioId)
    {
        if (string.IsNullOrEmpty(scenarioId)) return null;
        return await _mongo.Collection<Scenario>("scenarios")
            .Find(s => s.Id == scenarioId).FirstOrDefaultAsync();
    }

    public async Task<bool> MarkScenarioCompletedAsync(string scenarioId, string callerId, string role, string companyId)
    {
        if (string.IsNullOrEmpty(scenarioId)) return false;
        var scenario = await GetScenarioAsync(scenarioId);
        if (scenario == null) return false;

        if (string.Equals(role, "Worker", StringComparison.Ordinal))
        {
            if (scenario.WorkerId != callerId) return false;
            if (!string.IsNullOrEmpty(scenario.CompanyId) && !string.IsNullOrEmpty(companyId)
                && scenario.CompanyId != companyId) return false;
        }
        else
        {
            if (string.IsNullOrEmpty(companyId)) return false;
            if (string.IsNullOrEmpty(scenario.CompanyId) || scenario.CompanyId != companyId) return false;
        }

        var update = Builders<Scenario>.Update.Set(s => s.Status, "Completed");
        var result = await _mongo.Collection<Scenario>("scenarios")
            .UpdateOneAsync(s => s.Id == scenarioId, update);
        return result.MatchedCount > 0;
    }
}
