using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Bookstore.Core.Interfaces;
using Bookstore.Core.Messages;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Bookstore.Functions.Functions;

/// <summary>
/// Service Bus triggered function for processing notifications.
/// </summary>
public class NotificationProcessor
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<NotificationProcessor> _logger;

    public NotificationProcessor(
        INotificationService notificationService,
        ILogger<NotificationProcessor> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    [Function("ProcessNotification")]
    public async Task ProcessNotification(
        [ServiceBusTrigger("notifications", Connection = "ServiceBus")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing notification message {MessageId}, delivery count: {DeliveryCount}",
            message.MessageId,
            message.DeliveryCount);

        try
        {
            // Deserialize message
            var notification = JsonSerializer.Deserialize<NewBookNotificationMessage>(
                message.Body.ToString());

            if (notification == null)
            {
                _logger.LogError("Failed to deserialize message {MessageId}", message.MessageId);
                await messageActions.DeadLetterMessageAsync(
                    message,
                    deadLetterReason: "DeserializationFailed",
                    deadLetterErrorDescription: "Could not deserialize message body",
                    cancellationToken: cancellationToken);
                return;
            }

            // Process notification
            var result = await _notificationService.DeliverNotificationAsync(notification, cancellationToken);

            if (result.Success)
            {
                _logger.LogInformation(
                    "Successfully delivered notification to {SubscriberId} via {Method}",
                    result.SubscriberId,
                    result.DeliveryMethod);

                await messageActions.CompleteMessageAsync(message, cancellationToken);
            }
            else
            {
                _logger.LogWarning(
                    "Failed to deliver notification to {SubscriberId}: {Error}",
                    result.SubscriberId,
                    result.ErrorMessage);

                // Retry transient failures
                if (message.DeliveryCount < 5)
                {
                    await messageActions.AbandonMessageAsync(message, cancellationToken: cancellationToken);
                }
                else
                {
                    await messageActions.DeadLetterMessageAsync(
                        message,
                        deadLetterReason: "MaxRetriesExceeded",
                        deadLetterErrorDescription: result.ErrorMessage ?? "Unknown error",
                        cancellationToken: cancellationToken);
                }
            }
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Invalid JSON in message {MessageId}", message.MessageId);
            await messageActions.DeadLetterMessageAsync(
                message,
                deadLetterReason: "InvalidJson",
                deadLetterErrorDescription: ex.Message,
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message {MessageId}", message.MessageId);

            if (message.DeliveryCount < 5)
            {
                await messageActions.AbandonMessageAsync(message, cancellationToken: cancellationToken);
            }
            else
            {
                await messageActions.DeadLetterMessageAsync(
                    message,
                    deadLetterReason: "ProcessingFailed",
                    deadLetterErrorDescription: ex.Message,
                    cancellationToken: cancellationToken);
            }
        }
    }
}
