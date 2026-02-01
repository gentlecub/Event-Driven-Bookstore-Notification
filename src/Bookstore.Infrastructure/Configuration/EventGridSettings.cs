namespace Bookstore.Infrastructure.Configuration;

/// <summary>
/// Configuration settings for Azure Event Grid.
/// </summary>
public class EventGridSettings
{
    public const string SectionName = "EventGrid";

    /// <summary>
    /// The Event Grid topic endpoint.
    /// </summary>
    public required string TopicEndpoint { get; set; }

    /// <summary>
    /// Optional: Topic key for local development.
    /// </summary>
    public string? TopicKey { get; set; }

    /// <summary>
    /// Whether to use Managed Identity for authentication.
    /// </summary>
    public bool UseManagedIdentity { get; set; } = true;

    /// <summary>
    /// Source identifier for events.
    /// </summary>
    public string EventSource { get; set; } = "/bookstore/api";
}
