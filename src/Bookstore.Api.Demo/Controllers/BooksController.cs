using Bookstore.Core.Entities;
using Bookstore.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Bookstore.Api.Demo.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class BooksController : ControllerBase
{
    private readonly IBookRepository _bookRepository;
    private readonly ILogger<BooksController> _logger;

    public BooksController(IBookRepository bookRepository, ILogger<BooksController> logger)
    {
        _bookRepository = bookRepository;
        _logger = logger;
    }

    /// <summary>
    /// Gets all books.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Book>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Book>>> GetBooks()
    {
        var books = await _bookRepository.QueryAsync(q => q);
        return Ok(books);
    }

    /// <summary>
    /// Gets a book by ID.
    /// </summary>
    [HttpGet("{category}/{id}")]
    [ProducesResponseType(typeof(Book), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Book>> GetBookById(string category, string id)
    {
        var book = await _bookRepository.GetByIdAsync(id, category);
        if (book == null)
            return NotFound(new { message = "Book not found" });

        return Ok(book);
    }

    /// <summary>
    /// Gets a book by ISBN.
    /// </summary>
    [HttpGet("isbn/{isbn}")]
    [ProducesResponseType(typeof(Book), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Book>> GetBookByIsbn(string isbn)
    {
        var book = await _bookRepository.GetByIsbnAsync(isbn);
        if (book == null)
            return NotFound(new { message = "Book not found" });

        return Ok(book);
    }

    /// <summary>
    /// Gets books by category.
    /// </summary>
    [HttpGet("category/{category}")]
    [ProducesResponseType(typeof(IEnumerable<Book>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Book>>> GetBooksByCategory(string category)
    {
        var books = await _bookRepository.GetByCategoryAsync(category);
        return Ok(books);
    }

    /// <summary>
    /// Searches books by title, author, or description.
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(IEnumerable<Book>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Book>>> SearchBooks([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q))
            return Ok(Array.Empty<Book>());

        var books = await _bookRepository.SearchAsync(q);
        return Ok(books);
    }

    /// <summary>
    /// Gets recently added books.
    /// </summary>
    [HttpGet("recent")]
    [ProducesResponseType(typeof(IEnumerable<Book>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Book>>> GetRecentBooks([FromQuery] int count = 10)
    {
        var books = await _bookRepository.GetRecentBooksAsync(count);
        return Ok(books);
    }

    /// <summary>
    /// Gets all unique categories.
    /// </summary>
    [HttpGet("categories")]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<string>>> GetCategories()
    {
        var categories = await _bookRepository.GetCategoriesAsync();
        return Ok(categories);
    }

    /// <summary>
    /// Creates a new book.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Book), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Book>> CreateBook([FromBody] CreateBookRequest request)
    {
        // Check for duplicate ISBN
        var existing = await _bookRepository.GetByIsbnAsync(request.Isbn);
        if (existing != null)
            return BadRequest(new { message = "A book with this ISBN already exists" });

        var book = new Book
        {
            Title = request.Title,
            Author = request.Author,
            Isbn = request.Isbn,
            Category = request.Category,
            Description = request.Description,
            Price = request.Price,
            PublishedDate = request.PublishedDate,
            Publisher = request.Publisher,
            PageCount = request.PageCount,
            CoverImageUrl = request.CoverImageUrl,
            StockQuantity = request.StockQuantity,
            IsAvailable = request.IsAvailable,
            Tags = request.Tags ?? []
        };

        var created = await _bookRepository.CreateAsync(book);
        _logger.LogInformation("Book created: {BookId} - {Title}", created.Id, created.Title);

        return CreatedAtAction(nameof(GetBookById),
            new { category = created.Category, id = created.Id }, created);
    }

    /// <summary>
    /// Updates a book.
    /// </summary>
    [HttpPut("{category}/{id}")]
    [ProducesResponseType(typeof(Book), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Book>> UpdateBook(string category, string id, [FromBody] UpdateBookRequest request)
    {
        var book = await _bookRepository.GetByIdAsync(id, category);
        if (book == null)
            return NotFound(new { message = "Book not found" });

        // Update fields
        if (request.Title != null) book.Title = request.Title;
        if (request.Author != null) book.Author = request.Author;
        if (request.Description != null) book.Description = request.Description;
        if (request.Price.HasValue) book.Price = request.Price.Value;
        if (request.StockQuantity.HasValue) book.StockQuantity = request.StockQuantity.Value;
        if (request.IsAvailable.HasValue) book.IsAvailable = request.IsAvailable.Value;
        if (request.Tags != null) book.Tags = request.Tags;

        var updated = await _bookRepository.UpdateAsync(book);
        _logger.LogInformation("Book updated: {BookId}", updated.Id);

        return Ok(updated);
    }

    /// <summary>
    /// Deletes a book.
    /// </summary>
    [HttpDelete("{category}/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteBook(string category, string id)
    {
        var exists = await _bookRepository.ExistsAsync(id, category);
        if (!exists)
            return NotFound(new { message = "Book not found" });

        await _bookRepository.DeleteAsync(id, category);
        _logger.LogInformation("Book deleted: {BookId}", id);

        return NoContent();
    }
}

public record CreateBookRequest(
    string Title,
    string Author,
    string Isbn,
    string Category,
    string? Description = null,
    decimal Price = 0,
    DateTime? PublishedDate = null,
    string? Publisher = null,
    int? PageCount = null,
    string? CoverImageUrl = null,
    int StockQuantity = 0,
    bool IsAvailable = true,
    List<string>? Tags = null
);

public record UpdateBookRequest(
    string? Title = null,
    string? Author = null,
    string? Description = null,
    decimal? Price = null,
    int? StockQuantity = null,
    bool? IsAvailable = null,
    List<string>? Tags = null
);
