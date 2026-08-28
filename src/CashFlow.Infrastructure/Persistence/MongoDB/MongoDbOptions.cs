namespace CashFlow.Infrastructure.Persistence.MongoDB;

public sealed class MongoDbOptions
{
    public const string SectionName = "MongoDb";

    public string ConnectionString { get; init; } = "mongodb://localhost:27017/?replicaSet=rs0";

    public string DatabaseName { get; init; } = "cashflow";
}
