namespace CashFlow.Infrastructure.Outbox;

public sealed class OutboxMessage
{
    private OutboxMessage()
    {
        Type = string.Empty;
        Payload = string.Empty;
    }

    private OutboxMessage(
        Guid id,
        string type,
        string payload,
        DateTimeOffset occurredAtUtc)
    {
        Id = id;
        Type = type;
        Payload = payload;
        OccurredAtUtc = occurredAtUtc.ToUniversalTime();
    }

    public Guid Id { get; private set; }

    public string Type { get; private set; }

    public string Payload { get; private set; }

    public DateTimeOffset OccurredAtUtc { get; private set; }

    public DateTimeOffset? ProcessedAtUtc { get; private set; }

    public int RetryCount { get; private set; }

    public string? LastError { get; private set; }

    public DateTimeOffset? NextAttemptAtUtc { get; private set; }

    internal static OutboxMessage Create(
        Guid id,
        string type,
        string payload,
        DateTimeOffset occurredAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Outbox message id must not be empty.", nameof(id));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(type);
        ArgumentException.ThrowIfNullOrWhiteSpace(payload);

        return new OutboxMessage(id, type, payload, occurredAtUtc);
    }

    internal void MarkAsProcessed(DateTimeOffset processedAtUtc)
    {
        ProcessedAtUtc = processedAtUtc.ToUniversalTime();
        NextAttemptAtUtc = null;
        LastError = null;
    }

    internal void MarkAsFailed(DateTimeOffset failedAtUtc, string error)
    {
        RetryCount++;
        LastError = string.IsNullOrWhiteSpace(error)
            ? "Falha desconhecida durante a publicação."
            : error[..Math.Min(error.Length, 2048)];

        var backoffSeconds = Math.Min(Math.Pow(2, Math.Min(RetryCount, 8)), 300);
        NextAttemptAtUtc = failedAtUtc.ToUniversalTime().AddSeconds(backoffSeconds);
    }
}
