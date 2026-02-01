using System.Net;
using Bookstore.Core.Entities;
using Bookstore.Core.Events;
using Bookstore.Core.Interfaces;
using Bookstore.Functions.DTOs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Bookstore.Functions.Functions;

/// <summary>
/// HTTP API endpoints for book operations.
/// </summary>
public class BooksApi
{
    private readonly IBookRepository _bookRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<BooksApi> _logger;

    public BooksApi(
        IBookRepository bookRepository,
        IEventPublisher eventPublisher,
        ILogger<BooksApi> logger)
    {
        _bookRepository = bookRepository;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    [Function("GetBooks")]
    public async Task<HttpResponseData> GetBooks(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "books")] HttpRequestData req,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting all books");

        var books = await _bookRepository.GetAvailableBooksAsync(cancellationToken);
        var response = books.Select(BookResponse.FromEntity).ToList();

        return await CreateJsonResponse(req, HttpStatusCode.OK, response);
    }

    [Function("GetBookById")]
    public async Task<HttpResponseData> GetBookById(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "books/{category}/{id}")] HttpRequestData req,
        string category,
        string id,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting book {BookId} in category {Category}", id, category);

        var book = await _bookRepository.GetByIdAsync(id, category, cancellationToken);

        if (book == null)
        {
            return await CreateJsonResponse(req, HttpStatusCode.NotFound,
                ApiResponse<BookResponse>.Fail("Book not found"));
        }

        return await CreateJsonResponse(req, HttpStatusCode.OK,
            ApiResponse<BookResponse>.Ok(BookResponse.FromEntity(book)));
    }

    [Function("GetBookByIsbn")]
    public async Task<HttpResponseData> GetBookByIsbn(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "books/isbn/{isbn}")] HttpRequestData req,
        string isbn,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting book by ISBN {Isbn}", isbn);

        var book = await _bookRepository.GetByIsbnAsync(isbn, cancellationToken);

        if (book == null)
        {
            return await CreateJsonResponse(req, HttpStatusCode.NotFound,
                ApiResponse<BookResponse>.Fail("Book not found"));
        }

        return await CreateJsonResponse(req, HttpStatusCode.OK,
            ApiResponse<BookResponse>.Ok(BookResponse.FromEntity(book)));
    }

    [Function("GetBooksByCategory")]
    public async Task<HttpResponseData> GetBooksByCategory(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "books/category/{category}")] HttpRequestData req,
        string category,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting books in category {Category}", category);

        var books = await _bookRepository.GetByCategoryAsync(category, cancellationToken);
        var response = books.Select(BookResponse.FromEntity).ToList();

        return await CreateJsonResponse(req, HttpStatusCode.OK, response);
    }

    [Function("SearchBooks")]
    public async Task<HttpResponseData> SearchBooks(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "books/search")] HttpRequestData req,
        CancellationToken cancellationToken)
    {
        var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
        var searchTerm = query["q"] ?? "";

        _logger.LogInformation("Searching books with term: {SearchTerm}", searchTerm);

        var books = await _bookRepository.SearchAsync(searchTerm, cancellationToken);
        var response = books.Select(BookResponse.FromEntity).ToList();

        return await CreateJsonResponse(req, HttpStatusCode.OK, response);
    }

    [Function("GetRecentBooks")]
    public async Task<HttpResponseData> GetRecentBooks(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "books/recent")] HttpRequestData req,
        CancellationToken cancellationToken)
    {
        var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
        var count = int.TryParse(query["count"], out var c) ? c : 10;

        _logger.LogInformation("Getting {Count} recent books", count);

        var books = await _bookRepository.GetRecentBooksAsync(count, cancellationToken);
        var response = books.Select(BookResponse.FromEntity).ToList();

        return await CreateJsonResponse(req, HttpStatusCode.OK, response);
    }

    [Function("GetCategories")]
    public async Task<HttpResponseData> GetCategories(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "books/categories")] HttpRequestData req,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting all categories");

        var categories = await _bookRepository.GetCategoriesAsync(cancellationToken);

        return await CreateJsonResponse(req, HttpStatusCode.OK, categories.ToList());
    }

    [Function("CreateBook")]
    public async Task<HttpResponseData> CreateBook(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "books")] HttpRequestData req,
        CancellationToken cancellationToken)
    {
        var createRequest = await req.ReadFromJsonAsync<CreateBookRequest>(cancellationToken);

        if (createRequest == null)
        {
            return await CreateJsonResponse(req, HttpStatusCode.BadRequest,
                ApiResponse<BookResponse>.Fail("Invalid request body"));
        }

        // Check if ISBN already exists
        var existing = await _bookRepository.GetByIsbnAsync(createRequest.Isbn, cancellationToken);
        if (existing != null)
        {
            return await CreateJsonResponse(req, HttpStatusCode.Conflict,
                ApiResponse<BookResponse>.Fail("Book with this ISBN already exists"));
        }

        var book = new Book
        {
            Title = createRequest.Title,
            Author = createRequest.Author,
            Isbn = createRequest.Isbn,
            Category = createRequest.Category,
            Description = createRequest.Description,
            Price = createRequest.Price,
            PublishedDate = createRequest.PublishedDate,
            Publisher = createRequest.Publisher,
            PageCount = createRequest.PageCount,
            CoverImageUrl = createRequest.CoverImageUrl,
            StockQuantity = createRequest.StockQuantity,
            IsAvailable = createRequest.StockQuantity > 0,
            Tags = createRequest.Tags ?? []
        };

        var created = await _bookRepository.CreateAsync(book, cancellationToken);

        _logger.LogInformation("Created book {BookId} ({Title})", created.Id, created.Title);

        // Publish event for notifications
        var bookEvent = new BookCreatedEvent
        {
            Subject = $"/books/{created.Id}",
            Data = BookCreatedEventData.FromBook(created)
        };

        await _eventPublisher.PublishBookCreatedAsync(bookEvent, cancellationToken);

        return await CreateJsonResponse(req, HttpStatusCode.Created,
            ApiResponse<BookResponse>.Ok(BookResponse.FromEntity(created)));
    }

    [Function("UpdateBook")]
    public async Task<HttpResponseData> UpdateBook(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "books/{category}/{id}")] HttpRequestData req,
        string category,
        string id,
        CancellationToken cancellationToken)
    {
        var updateRequest = await req.ReadFromJsonAsync<UpdateBookRequest>(cancellationToken);

        if (updateRequest == null)
        {
            return await CreateJsonResponse(req, HttpStatusCode.BadRequest,
                ApiResponse<BookResponse>.Fail("Invalid request body"));
        }

        var book = await _bookRepository.GetByIdAsync(id, category, cancellationToken);

        if (book == null)
        {
            return await CreateJsonResponse(req, HttpStatusCode.NotFound,
                ApiResponse<BookResponse>.Fail("Book not found"));
        }

        // Update fields
        if (updateRequest.Title != null) book.Title = updateRequest.Title;
        if (updateRequest.Author != null) book.Author = updateRequest.Author;
        if (updateRequest.Description != null) book.Description = updateRequest.Description;
        if (updateRequest.Price.HasValue) book.Price = updateRequest.Price.Value;
        if (updateRequest.PublishedDate.HasValue) book.PublishedDate = updateRequest.PublishedDate;
        if (updateRequest.Publisher != null) book.Publisher = updateRequest.Publisher;
        if (updateRequest.PageCount.HasValue) book.PageCount = updateRequest.PageCount;
        if (updateRequest.CoverImageUrl != null) book.CoverImageUrl = updateRequest.CoverImageUrl;
        if (updateRequest.StockQuantity.HasValue) book.StockQuantity = updateRequest.StockQuantity.Value;
        if (updateRequest.IsAvailable.HasValue) book.IsAvailable = updateRequest.IsAvailable.Value;
        if (updateRequest.Tags != null) book.Tags = updateRequest.Tags;

        var updated = await _bookRepository.UpdateAsync(book, cancellationToken);

        _logger.LogInformation("Updated book {BookId}", updated.Id);

        return await CreateJsonResponse(req, HttpStatusCode.OK,
            ApiResponse<BookResponse>.Ok(BookResponse.FromEntity(updated)));
    }

    [Function("DeleteBook")]
    public async Task<HttpResponseData> DeleteBook(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "books/{category}/{id}")] HttpRequestData req,
        string category,
        string id,
        CancellationToken cancellationToken)
    {
        var book = await _bookRepository.GetByIdAsync(id, category, cancellationToken);

        if (book == null)
        {
            return await CreateJsonResponse(req, HttpStatusCode.NotFound,
                ApiResponse<object>.Fail("Book not found"));
        }

        await _bookRepository.DeleteAsync(id, category, cancellationToken);

        _logger.LogInformation("Deleted book {BookId}", id);

        // Publish delete event
        var deleteEvent = new BookDeletedEvent
        {
            Subject = $"/books/{id}",
            Data = new BookDeletedEventData
            {
                BookId = id,
                Category = category,
                Title = book.Title,
                DeletedAt = DateTime.UtcNow
            }
        };

        await _eventPublisher.PublishBookDeletedAsync(deleteEvent, cancellationToken);

        return req.CreateResponse(HttpStatusCode.NoContent);
    }

    private static async Task<HttpResponseData> CreateJsonResponse<T>(
        HttpRequestData req,
        HttpStatusCode statusCode,
        T body)
    {
        var response = req.CreateResponse(statusCode);
        await response.WriteAsJsonAsync(body);
        return response;
    }
}
