using Bookstore.Core.Entities;

namespace Bookstore.Functions.DTOs;

/// <summary>
/// Request DTO for creating a book.
/// </summary>
public record CreateBookRequest(
    string Title,
    string Author,
    string Isbn,
    string Category,
    string? Description,
    decimal Price,
    DateTime? PublishedDate,
    string? Publisher,
    int? PageCount,
    string? CoverImageUrl,
    int StockQuantity = 0,
    List<string>? Tags = null);

/// <summary>
/// Request DTO for updating a book.
/// </summary>
public record UpdateBookRequest(
    string? Title,
    string? Author,
    string? Description,
    decimal? Price,
    DateTime? PublishedDate,
    string? Publisher,
    int? PageCount,
    string? CoverImageUrl,
    int? StockQuantity,
    bool? IsAvailable,
    List<string>? Tags);

/// <summary>
/// Response DTO for book data.
/// </summary>
public record BookResponse(
    string Id,
    string Title,
    string Author,
    string Isbn,
    string Category,
    string? Description,
    decimal Price,
    DateTime? PublishedDate,
    string? Publisher,
    int? PageCount,
    string? CoverImageUrl,
    int StockQuantity,
    bool IsAvailable,
    List<string> Tags,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    public static BookResponse FromEntity(Book book) => new(
        book.Id,
        book.Title,
        book.Author,
        book.Isbn,
        book.Category,
        book.Description,
        book.Price,
        book.PublishedDate,
        book.Publisher,
        book.PageCount,
        book.CoverImageUrl,
        book.StockQuantity,
        book.IsAvailable,
        book.Tags,
        book.CreatedAt,
        book.UpdatedAt);
}

/// <summary>
/// Generic API response wrapper.
/// </summary>
public record ApiResponse<T>(
    bool Success,
    T? Data,
    string? Error = null)
{
    public static ApiResponse<T> Ok(T data) => new(true, data);
    public static ApiResponse<T> Fail(string error) => new(false, default, error);
}

/// <summary>
/// Paginated list response.
/// </summary>
public record PagedResponse<T>(
    List<T> Items,
    int TotalCount,
    int Page,
    int PageSize);
