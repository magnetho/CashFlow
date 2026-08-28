using MongoDB.Bson.Serialization.Attributes;

namespace CashFlow.Infrastructure.Persistence.MongoDB;

internal sealed class DailyBalanceDocument
{
    [BsonId]
    public required string Date { get; init; }

    [BsonRepresentation(global::MongoDB.Bson.BsonType.Decimal128)]
    public decimal TotalCredits { get; init; }

    [BsonRepresentation(global::MongoDB.Bson.BsonType.Decimal128)]
    public decimal TotalDebits { get; init; }

    [BsonRepresentation(global::MongoDB.Bson.BsonType.Decimal128)]
    public decimal Balance { get; init; }

    public DateTime UpdatedAtUtc { get; init; }
}
