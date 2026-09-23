namespace SurakshaAR.Backend.Config;

public class MongoConfig
{
    public string ConnectionString { get; set; } = "mongodb://localhost:27017";
    public string DatabaseName { get; set; } = "SurakshaAR";
}
