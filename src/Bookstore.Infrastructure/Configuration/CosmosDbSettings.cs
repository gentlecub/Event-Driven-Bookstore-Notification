namespace Bookstore.Infrastructure.Configuration;

/// <summary>
/// Configuration settings for Azure Cosmos DB connection.
/// </summary>
public class CosmosDbSettings
{
    public const string SectionName = "CosmosDb";

    /// <summary>
    /// The Cosmos DB account endpoint URL.
    /// </summary>
    public required string Endpoint { get; set; }

    /// <summary>
    /// The database name within the Cosmos DB account.
    /// </summary>
    public string DatabaseName { get; set; } = "bookstore-db";

    /// <summary>
    /// Container name for books collection.
    /// </summary>
    public string BooksContainerName { get; set; } = "books";

    /// <summary>
    /// Container name for subscribers collection.
    /// </summary>
    public string SubscribersContainerName { get; set; } = "subscribers";

    /// <summary>
    /// Optional: Primary key for local development.
    /// </summary>
    public string? PrimaryKey { get; set; }

    /// <summary>
    /// Whether to use Managed Identity for authentication.
    /// </summary>
    public bool UseManagedIdentity { get; set; } = true;
}
