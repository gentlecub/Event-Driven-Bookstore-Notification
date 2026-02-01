using System.Text.Json.Serialization;

namespace Bookstore.Core.Entities;

/// <summary>
/// Represents a book in the inventory.
/// Stored in the 'books' container with partition key '/category'.
/// </summary>
public class Book : BaseEntity
{
    /// <summary>
    /// Book title.
    /// </summary>
    [JsonPropertyName("title")]
    public required string Title { get; set; }

    /// <summary>
    /// Book author(s).
    /// </summary>
    [JsonPropertyName("author")]
    public required string Author { get; set; }

    /// <summary>
    /// International Standard Book Number.
    /// Unique key constraint in Cosmos DB.
    /// </summary>
    [JsonPropertyName("isbn")]
    public required string Isbn { get; set; }

    /// <summary>
    /// Book category (partition key).
    /// Examples: Fiction, Non-Fiction, Science, Technology, etc.
    /// </summary>
    [JsonPropertyName("category")]
    public required string Category { get; set; }

    /// <summary>
    /// Partition key implementation.
    /// </summary>
    [JsonIgnore]
    public override string PartitionKey => Category;

    /// <summary>
    /// Book description or summary.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Book price in USD.
    /// </summary>
    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    /// <summary>
    /// Publication date.
    /// </summary>
    [JsonPropertyName("publishedDate")]
    public DateTime? PublishedDate { get; set; }

    /// <summary>
    /// Publisher name.
    /// </summary>
    [JsonPropertyName("publisher")]
    public string? Publisher { get; set; }

    /// <summary>
    /// Number of pages.
    /// </summary>
    [JsonPropertyName("pageCount")]
    public int? PageCount { get; set; }

    /// <summary>
    /// Book cover image URL.
    /// </summary>
    [JsonPropertyName("coverImageUrl")]
    public string? CoverImageUrl { get; set; }

    /// <summary>
    /// Available quantity in stock.
    /// </summary>
    [JsonPropertyName("stockQuantity")]
    public int StockQuantity { get; set; }

    /// <summary>
    /// Indicates if the book is currently available for purchase.
    /// </summary>
    [JsonPropertyName("isAvailable")]
    public bool IsAvailable { get; set; } = true;

    /// <summary>
    /// Tags for additional categorization and search.
    /// </summary>
    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; } = [];
}
