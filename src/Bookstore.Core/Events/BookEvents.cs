using System.Text.Json.Serialization;
using Bookstore.Core.Entities;

namespace Bookstore.Core.Events;

/// <summary>
/// Base class for all book-related events.
/// Follows CloudEvents specification structure.
/// </summary>
public abstract class BookEventBase
{
    /// <summary>
    /// Event type identifier.
    /// Format: com.bookstore.{event-name}
    /// </summary>
    [JsonPropertyName("type")]
    public abstract string Type { get; }

    /// <summary>
    /// Event source identifier.
    /// </summary>
    [JsonPropertyName("source")]
    public string Source { get; set; } = "/bookstore/books";

    /// <summary>
    /// Unique event identifier.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Event timestamp.
    /// </summary>
    [JsonPropertyName("time")]
    public DateTime Time { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Content type of the data payload.
    /// </summary>
    [JsonPropertyName("datacontenttype")]
    public string DataContentType { get; set; } = "application/json";

    /// <summary>
    /// CloudEvents specification version.
    /// </summary>
    [JsonPropertyName("specversion")]
    public string SpecVersion { get; set; } = "1.0";

    /// <summary>
    /// Subject of the event (typically the resource ID).
    /// </summary>
    [JsonPropertyName("subject")]
    public string? Subject { get; set; }
}

/// <summary>
/// Event raised when a new book is created.
/// This triggers notifications to subscribers.
/// </summary>
public class BookCreatedEvent : BookEventBase
{
    /// <summary>
    /// Event type for book creation.
    /// </summary>
    public override string Type => "com.bookstore.book.created";

    /// <summary>
    /// Event payload containing book details.
    /// </summary>
    [JsonPropertyName("data")]
    public BookCreatedEventData Data { get; set; } = new();
}

/// <summary>
/// Data payload for BookCreatedEvent.
/// </summary>
public class BookCreatedEventData
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
    /// Book category (used for subscriber filtering).
    /// </summary>
    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Book price.
    /// </summary>
    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    /// <summary>
    /// Timestamp when the book was created.
    /// </summary>
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Creates event data from a Book entity.
    /// </summary>
    public static BookCreatedEventData FromBook(Book book)
    {
        return new BookCreatedEventData
        {
            BookId = book.Id,
            Title = book.Title,
            Author = book.Author,
            Isbn = book.Isbn,
            Category = book.Category,
            Price = book.Price,
            CreatedAt = book.CreatedAt
        };
    }
}

/// <summary>
/// Event raised when a book is updated.
/// </summary>
public class BookUpdatedEvent : BookEventBase
{
    /// <summary>
    /// Event type for book update.
    /// </summary>
    public override string Type => "com.bookstore.book.updated";

    /// <summary>
    /// Event payload containing update details.
    /// </summary>
    [JsonPropertyName("data")]
    public BookUpdatedEventData Data { get; set; } = new();
}

/// <summary>
/// Data payload for BookUpdatedEvent.
/// </summary>
public class BookUpdatedEventData
{
    /// <summary>
    /// Book unique identifier.
    /// </summary>
    [JsonPropertyName("bookId")]
    public string BookId { get; set; } = string.Empty;

    /// <summary>
    /// Book category.
    /// </summary>
    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// List of updated fields.
    /// </summary>
    [JsonPropertyName("updatedFields")]
    public List<string> UpdatedFields { get; set; } = [];

    /// <summary>
    /// Timestamp when the book was updated.
    /// </summary>
    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Event raised when a book is deleted.
/// </summary>
public class BookDeletedEvent : BookEventBase
{
    /// <summary>
    /// Event type for book deletion.
    /// </summary>
    public override string Type => "com.bookstore.book.deleted";

    /// <summary>
    /// Event payload containing deletion details.
    /// </summary>
    [JsonPropertyName("data")]
    public BookDeletedEventData Data { get; set; } = new();
}

/// <summary>
/// Data payload for BookDeletedEvent.
/// </summary>
public class BookDeletedEventData
{
    /// <summary>
    /// Book unique identifier.
    /// </summary>
    [JsonPropertyName("bookId")]
    public string BookId { get; set; } = string.Empty;

    /// <summary>
    /// Book category.
    /// </summary>
    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Book title (for reference).
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the book was deleted.
    /// </summary>
    [JsonPropertyName("deletedAt")]
    public DateTime DeletedAt { get; set; }
}
