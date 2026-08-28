using MongoDB.Bson.Serialization.Attributes;

namespace CashFlow.Infrastructure.Persistence.MongoDB;

internal sealed class InboxMessageDocument
{
    [BsonId]
    [BsonGuidRepresentation(global::MongoDB.Bson.GuidRepresentation.Standard)]
    public Guid EventId { get; init; }

    public DateTime ProcessedAtUtc { get; init; }
}
