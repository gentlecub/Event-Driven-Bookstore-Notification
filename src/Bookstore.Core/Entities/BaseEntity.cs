using System.Text.Json.Serialization;

namespace Bookstore.Core.Entities;

/// <summary>
/// Base entity for all Cosmos DB documents.
/// Contains common properties required by Cosmos DB.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Unique identifier for the document.
    /// Maps to Cosmos DB 'id' property.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Partition key value.
    /// Must be implemented by derived classes.
    /// </summary>
    [JsonIgnore]
    public abstract string PartitionKey { get; }

    /// <summary>
    /// Document type discriminator for polymorphic queries.
    /// </summary>
    [JsonPropertyName("type")]
    public virtual string Type => GetType().Name;

    /// <summary>
    /// Timestamp when the document was created.
    /// </summary>
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the document was last updated.
    /// </summary>
    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// ETag for optimistic concurrency control.
    /// Managed by Cosmos DB.
    /// </summary>
    [JsonPropertyName("_etag")]
    public string? ETag { get; set; }
}
