using Bookstore.Core.Messages;

namespace Bookstore.Core.Interfaces;

/// <summary>
/// Interface for sending messages to Service Bus queues.
/// </summary>
public interface IMessagePublisher
{
    /// <summary>
    /// Sends a single notification message to the queue.
    /// </summary>
    /// <param name="message">Message to send</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SendNotificationAsync(
        NewBookNotificationMessage message,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends multiple notification messages in a batch.
    /// </summary>
    /// <param name="messages">Messages to send</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SendNotificationBatchAsync(
        IEnumerable<NewBookNotificationMessage> messages,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a message with a scheduled delivery time.
    /// </summary>
    /// <param name="message">Message to send</param>
    /// <param name="scheduledTime">When to deliver the message</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task ScheduleNotificationAsync(
        NewBookNotificationMessage message,
        DateTimeOffset scheduledTime,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface for handling incoming messages from Service Bus.
/// </summary>
/// <typeparam name="TMessage">Type of message to handle</typeparam>
public interface IMessageHandler<in TMessage> where TMessage : MessageBase
{
    /// <summary>
    /// Processes a message from the queue.
    /// </summary>
    /// <param name="message">Message to process</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of the processing</returns>
    Task<MessageProcessingResult> HandleAsync(
        TMessage message,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of message processing.
/// </summary>
public class MessageProcessingResult
{
    /// <summary>
    /// Whether processing was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Error message if processing failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Whether to dead-letter the message on failure.
    /// </summary>
    public bool ShouldDeadLetter { get; set; }

    /// <summary>
    /// Whether to abandon the message for retry.
    /// </summary>
    public bool ShouldRetry { get; set; }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    public static MessageProcessingResult Completed() => new() { Success = true };

    /// <summary>
    /// Creates a failed result that should be retried.
    /// </summary>
    public static MessageProcessingResult Retry(string error) => new()
    {
        Success = false,
        ErrorMessage = error,
        ShouldRetry = true
    };

    /// <summary>
    /// Creates a failed result that should be dead-lettered.
    /// </summary>
    public static MessageProcessingResult DeadLetter(string error) => new()
    {
        Success = false,
        ErrorMessage = error,
        ShouldDeadLetter = true
    };
}

/// <summary>
/// Interface for the notification service that orchestrates notifications.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Sends notifications about a new book to all interested subscribers.
    /// </summary>
    /// <param name="bookId">ID of the new book</param>
    /// <param name="category">Category of the book</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of notifications queued</returns>
    Task<int> NotifySubscribersAsync(
        string bookId,
        string category,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes a notification message and delivers it to the subscriber.
    /// </summary>
    /// <param name="message">Notification message to process</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of the delivery</returns>
    Task<NotificationResult> DeliverNotificationAsync(
        NewBookNotificationMessage message,
        CancellationToken cancellationToken = default);
}
