namespace Bookstore.Infrastructure.Configuration;

/// <summary>
/// Configuration settings for Azure Service Bus.
/// </summary>
public class ServiceBusSettings
{
    public const string SectionName = "ServiceBus";

    /// <summary>
    /// The fully qualified Service Bus namespace.
    /// </summary>
    public required string FullyQualifiedNamespace { get; set; }

    /// <summary>
    /// Optional: Connection string for local development.
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Queue name for notification messages.
    /// </summary>
    public string NotificationQueueName { get; set; } = "notifications";

    /// <summary>
    /// Whether to use Managed Identity for authentication.
    /// </summary>
    public bool UseManagedIdentity { get; set; } = true;
}
