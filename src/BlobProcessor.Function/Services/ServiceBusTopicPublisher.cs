using System.Text.Json;
using Azure.Messaging.ServiceBus;
using BlobProcessor.Function.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BlobProcessor.Function.Services;

public sealed class ServiceBusTopicPublisher : IMessagePublisher, IAsyncDisposable
{
    private readonly ServiceBusSender _sender;
    private readonly ILogger<ServiceBusTopicPublisher> _logger;

    public ServiceBusTopicPublisher(
        ServiceBusClient serviceBusClient,
        IOptions<ServiceBusOptions> options,
        ILogger<ServiceBusTopicPublisher> logger)
    {
        _logger = logger;

        var topicName = options.Value.TopicName;
        if (string.IsNullOrWhiteSpace(topicName))
        {
            throw new InvalidOperationException("ServiceBus:TopicName is required.");
        }

        _sender = serviceBusClient.CreateSender(topicName);
    }

    public async Task PublishAsync(ProcessedBlobMessage message, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message, nameof(message));

        try
        {
            var payload = JsonSerializer.Serialize(message);
            var busMessage = new ServiceBusMessage(payload)
            {
                Subject = "blob.processed",
                ContentType = "application/json"
            };

            busMessage.ApplicationProperties["blobName"] = message.BlobName;
            busMessage.ApplicationProperties["processedAtUtc"] = message.ProcessedAtUtc.ToString("O");

            await _sender.SendMessageAsync(busMessage, cancellationToken);
            _logger.LogInformation("Processed message sent to Service Bus topic for blob {BlobName}", message.BlobName);
        }
        catch (ServiceBusException ex)
        {
            _logger.LogError(ex,
                "Service Bus error publishing message for blob {BlobName}. Reason: {Reason}",
                message.BlobName, ex.Reason);
            throw;
        }
    }

    public ValueTask DisposeAsync()
    {
        return _sender.DisposeAsync();
    }
}
