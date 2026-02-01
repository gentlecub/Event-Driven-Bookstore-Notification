using System.Text.Json;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Bookstore.Core.Interfaces;
using Bookstore.Core.Messages;
using Bookstore.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bookstore.Infrastructure.Services;

/// <summary>
/// Azure Service Bus implementation of IMessagePublisher.
/// </summary>
public class ServiceBusPublisher : IMessagePublisher, IAsyncDisposable
{
    private readonly ServiceBusClient _client;
    private readonly ServiceBusSender _sender;
    private readonly ILogger<ServiceBusPublisher> _logger;

    public ServiceBusPublisher(
        IOptions<ServiceBusSettings> settings,
        ILogger<ServiceBusPublisher> logger)
    {
        _logger = logger;
        var config = settings.Value;

        _client = config.UseManagedIdentity
            ? new ServiceBusClient(config.FullyQualifiedNamespace, new DefaultAzureCredential())
            : new ServiceBusClient(config.ConnectionString);

        _sender = _client.CreateSender(config.NotificationQueueName);
    }

    public async Task SendNotificationAsync(
        NewBookNotificationMessage message,
        CancellationToken cancellationToken = default)
    {
        var sbMessage = CreateServiceBusMessage(message);

        await _sender.SendMessageAsync(sbMessage, cancellationToken);

        _logger.LogInformation(
            "Sent notification message {MessageId} for subscriber {SubscriberId}",
            message.MessageId,
            message.Subscriber.SubscriberId);
    }

    public async Task SendNotificationBatchAsync(
        IEnumerable<NewBookNotificationMessage> messages,
        CancellationToken cancellationToken = default)
    {
        var messageList = messages.ToList();
        if (messageList.Count == 0)
        {
            _logger.LogDebug("No messages to send in batch");
            return;
        }

        using var messageBatch = await _sender.CreateMessageBatchAsync(cancellationToken);

        foreach (var message in messageList)
        {
            var sbMessage = CreateServiceBusMessage(message);

            if (!messageBatch.TryAddMessage(sbMessage))
            {
                // Batch is full, send it and create a new one
                await _sender.SendMessagesAsync(messageBatch, cancellationToken);
                _logger.LogInformation("Sent batch of {Count} messages", messageBatch.Count);

                // Create new batch and add current message
                using var newBatch = await _sender.CreateMessageBatchAsync(cancellationToken);
                if (!newBatch.TryAddMessage(sbMessage))
                {
                    throw new InvalidOperationException($"Message {message.MessageId} is too large for the batch");
                }
            }
        }

        if (messageBatch.Count > 0)
        {
            await _sender.SendMessagesAsync(messageBatch, cancellationToken);
            _logger.LogInformation("Sent final batch of {Count} messages", messageBatch.Count);
        }
    }

    public async Task ScheduleNotificationAsync(
        NewBookNotificationMessage message,
        DateTimeOffset scheduledTime,
        CancellationToken cancellationToken = default)
    {
        var sbMessage = CreateServiceBusMessage(message);

        var sequenceNumber = await _sender.ScheduleMessageAsync(
            sbMessage,
            scheduledTime,
            cancellationToken);

        _logger.LogInformation(
            "Scheduled notification message {MessageId} for {ScheduledTime}, sequence: {SequenceNumber}",
            message.MessageId,
            scheduledTime,
            sequenceNumber);
    }

    private static ServiceBusMessage CreateServiceBusMessage(NewBookNotificationMessage message)
    {
        var body = JsonSerializer.SerializeToUtf8Bytes(message);

        return new ServiceBusMessage(body)
        {
            MessageId = message.MessageId,
            CorrelationId = message.CorrelationId,
            ContentType = "application/json",
            Subject = message.Book.Category,
            ApplicationProperties =
            {
                ["messageType"] = message.MessageType,
                ["subscriberId"] = message.Subscriber.SubscriberId,
                ["bookId"] = message.Book.BookId
            }
        };
    }

    public async ValueTask DisposeAsync()
    {
        await _sender.DisposeAsync();
        await _client.DisposeAsync();
    }
}
