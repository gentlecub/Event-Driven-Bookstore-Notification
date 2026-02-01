using System.Text.Json.Serialization;

namespace Bookstore.Core.Messages;

/// <summary>
/// Base class for all Service Bus messages.
/// </summary>
public abstract class MessageBase
{
    /// <summary>
    /// Unique message identifier for deduplication.
    /// </summary>
    [JsonPropertyName("messageId")]
    public string MessageId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Correlation ID for distributed tracing.
    /// </summary>
    [JsonPropertyName("correlationId")]
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Message type discriminator.
    /// </summary>
    [JsonPropertyName("messageType")]
    public abstract string MessageType { get; }

    /// <summary>
    /// Timestamp when the message was created.
    /// </summary>
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Number of times this message has been attempted.
    /// </summary>
    [JsonPropertyName("attemptCount")]
    public int AttemptCount { get; set; }
}

/// <summary>
/// Message for notifying subscribers about a new book.
/// Sent to the notifications queue for processing.
/// </summary>
public class NewBookNotificationMessage : MessageBase
{
    /// <summary>
    /// Message type identifier.
    /// </summary>
    public override string MessageType => "NewBookNotification";

    /// <summary>
    /// Book details for the notification.
    /// </summary>
    [JsonPropertyName("book")]
    public BookNotificationData Book { get; set; } = new();

    /// <summary>
    /// Subscriber to notify.
    /// </summary>
    [JsonPropertyName("subscriber")]
    public SubscriberNotificationData Subscriber { get; set; } = new();
}

/// <summary>
/// Book data included in notification messages.
/// </summary>
public class BookNotificationData
{
    /// <summary>
    /// Book unique identifier.
    /// </summary>
    [JsonPropertyName("bookId")]
    public string BookId { get; set; } = string.Empty;

    /// <summary>
    /// Book title.
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Book author.
    /// </summary>
    [JsonPropertyName("author")]
    public string Author { get; set; } = string.Empty;

    /// <summary>
    /// Book ISBN.
    /// </summary>
    [JsonPropertyName("isbn")]
    public string Isbn { get; set; } = string.Empty;

    /// <summary>
    /// Book category.
    /// </summary>
    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Book description.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Book price.
    /// </summary>
    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    /// <summary>
    /// URL to view the book (optional).
    /// </summary>
    [JsonPropertyName("bookUrl")]
    public string? BookUrl { get; set; }
}

/// <summary>
/// Subscriber data included in notification messages.
/// </summary>
public class SubscriberNotificationData
{
    /// <summary>
    /// Subscriber unique identifier.
    /// </summary>
    [JsonPropertyName("subscriberId")]
    public string SubscriberId { get; set; } = string.Empty;

    /// <summary>
    /// Subscriber email address.
    /// </summary>
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Subscriber display name.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Notification delivery preference.
    /// </summary>
    [JsonPropertyName("notificationPreference")]
    public string NotificationPreference { get; set; } = "Email";

    /// <summary>
    /// Webhook URL (if preference includes webhook).
    /// </summary>
    [JsonPropertyName("webhookUrl")]
    public string? WebhookUrl { get; set; }
}

/// <summary>
/// Message for batch notification processing.
/// Contains multiple subscribers to notify about the same book.
/// </summary>
public class BatchNotificationMessage : MessageBase
{
    /// <summary>
    /// Message type identifier.
    /// </summary>
    public override string MessageType => "BatchNotification";

    /// <summary>
    /// Book details for the notification.
    /// </summary>
    [JsonPropertyName("book")]
    public BookNotificationData Book { get; set; } = new();

    /// <summary>
    /// List of subscribers to notify.
    /// </summary>
    [JsonPropertyName("subscribers")]
    public List<SubscriberNotificationData> Subscribers { get; set; } = [];
}

/// <summary>
/// Result of a notification delivery attempt.
/// </summary>
public class NotificationResult
{
    /// <summary>
    /// Whether the notification was delivered successfully.
    /// </summary>
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    /// <summary>
    /// Subscriber ID that was notified.
    /// </summary>
    [JsonPropertyName("subscriberId")]
    public string SubscriberId { get; set; } = string.Empty;

    /// <summary>
    /// Delivery method used.
    /// </summary>
    [JsonPropertyName("deliveryMethod")]
    public string DeliveryMethod { get; set; } = string.Empty;

    /// <summary>
    /// Error message if delivery failed.
    /// </summary>
    [JsonPropertyName("errorMessage")]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Timestamp of the delivery attempt.
    /// </summary>
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
