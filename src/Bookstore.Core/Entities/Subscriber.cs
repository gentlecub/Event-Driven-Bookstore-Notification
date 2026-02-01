using System.Text.Json.Serialization;

namespace Bookstore.Core.Entities;

/// <summary>
/// Represents a subscriber who receives notifications about new books.
/// Stored in the 'subscribers' container with partition key '/id'.
/// </summary>
public class Subscriber : BaseEntity
{
    /// <summary>
    /// Partition key implementation.
    /// Uses Id for even distribution across partitions.
    /// </summary>
    [JsonIgnore]
    public override string PartitionKey => Id;

    /// <summary>
    /// Subscriber's email address.
    /// Unique key constraint in Cosmos DB.
    /// </summary>
    [JsonPropertyName("email")]
    public required string Email { get; set; }

    /// <summary>
    /// Subscriber's display name.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    /// <summary>
    /// Indicates if the subscription is currently active.
    /// </summary>
    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Categories the subscriber is interested in.
    /// Empty list means all categories.
    /// </summary>
    [JsonPropertyName("subscribedCategories")]
    public List<string> SubscribedCategories { get; set; } = [];

    /// <summary>
    /// Preferred notification method.
    /// </summary>
    [JsonPropertyName("notificationPreference")]
    public NotificationPreference NotificationPreference { get; set; } = NotificationPreference.Email;

    /// <summary>
    /// Webhook URL for webhook notifications.
    /// Required if NotificationPreference is Webhook.
    /// </summary>
    [JsonPropertyName("webhookUrl")]
    public string? WebhookUrl { get; set; }

    /// <summary>
    /// Last notification sent timestamp.
    /// </summary>
    [JsonPropertyName("lastNotificationAt")]
    public DateTime? LastNotificationAt { get; set; }

    /// <summary>
    /// Total number of notifications sent to this subscriber.
    /// </summary>
    [JsonPropertyName("notificationCount")]
    public int NotificationCount { get; set; }

    /// <summary>
    /// Date when the subscriber confirmed their email.
    /// Null if not yet confirmed.
    /// </summary>
    [JsonPropertyName("confirmedAt")]
    public DateTime? ConfirmedAt { get; set; }

    /// <summary>
    /// Indicates if the email has been confirmed.
    /// </summary>
    [JsonIgnore]
    public bool IsConfirmed => ConfirmedAt.HasValue;
}

/// <summary>
/// Notification delivery preferences.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum NotificationPreference
{
    /// <summary>
    /// Send notifications via email.
    /// </summary>
    Email,

    /// <summary>
    /// Send notifications via webhook.
    /// </summary>
    Webhook,

    /// <summary>
    /// Send notifications via both email and webhook.
    /// </summary>
    Both
}
