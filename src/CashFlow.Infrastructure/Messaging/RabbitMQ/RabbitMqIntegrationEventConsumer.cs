using System.Text.Json;
using CashFlow.Contracts.Events;
using CashFlow.Infrastructure.Persistence.MongoDB;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CashFlow.Infrastructure.Messaging.RabbitMQ;

internal sealed class RabbitMqIntegrationEventConsumer(
    IOptions<RabbitMqOptions> options,
    IRabbitMqTopology topology,
    IServiceScopeFactory scopeFactory,
    ILogger<RabbitMqIntegrationEventConsumer> logger) : IIntegrationEventConsumer
{
    private static readonly JsonSerializerOptions SerializerOptions =
        new(JsonSerializerDefaults.Web);

    private readonly RabbitMqOptions _options = options.Value;

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password,
            VirtualHost = _options.VirtualHost,
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(5),
            RequestedHeartbeat = TimeSpan.FromSeconds(30)
        };

        await using var connection = await factory.CreateConnectionAsync(
            "cashflow-daily-balance-consumer",
            cancellationToken);
        await using var channel = await connection.CreateChannelAsync(
            cancellationToken: cancellationToken);

        await topology.DeclareAsync(channel, cancellationToken);
        await channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: 20,
            global: false,
            cancellationToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, eventArgs) =>
        {
            try
            {
                var integrationEvent = JsonSerializer.Deserialize<CashEntryCreatedIntegrationEvent>(
                    eventArgs.Body.Span,
                    SerializerOptions);

                if (integrationEvent is null || integrationEvent.SchemaVersion != 1)
                {
                    logger.LogWarning(
                        "Evento inválido ou com versão não suportada. MessageId: {MessageId}",
                        eventArgs.BasicProperties.MessageId);
                    await channel.BasicNackAsync(
                        eventArgs.DeliveryTag,
                        multiple: false,
                        requeue: false,
                        cancellationToken);
                    return;
                }

                await using var scope = scopeFactory.CreateAsyncScope();
                var projector = scope.ServiceProvider.GetRequiredService<IDailyBalanceProjector>();
                var processed = await projector.ProjectAsync(integrationEvent, cancellationToken);

                await channel.BasicAckAsync(
                    eventArgs.DeliveryTag,
                    multiple: false,
                    cancellationToken);

                if (processed)
                {
                    logger.LogInformation(
                        "Evento {EventId} processado para o consolidado de {AccountingDate}",
                        integrationEvent.EventId,
                        integrationEvent.AccountingDate);
                }
                else
                {
                    logger.LogInformation(
                        "Evento duplicado {EventId} ignorado",
                        integrationEvent.EventId);
                }
            }
            catch (JsonException exception)
            {
                logger.LogError(exception, "Mensagem inválida enviada para a DLQ");
                await channel.BasicNackAsync(
                    eventArgs.DeliveryTag,
                    multiple: false,
                    requeue: false,
                    cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // O encerramento do host fecha o canal e o RabbitMQ reenviará mensagens sem ACK.
            }
            catch (Exception exception)
            {
                logger.LogError(
                    exception,
                    "Falha ao processar mensagem {MessageId}; ela será reenfileirada",
                    eventArgs.BasicProperties.MessageId);
                await channel.BasicNackAsync(
                    eventArgs.DeliveryTag,
                    multiple: false,
                    requeue: true,
                    cancellationToken);
            }
        };

        await channel.BasicConsumeAsync(
            queue: _options.Queue,
            autoAck: false,
            consumer: consumer,
            cancellationToken);

        await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
    }
}
