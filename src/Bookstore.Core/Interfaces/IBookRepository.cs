using Bookstore.Core.Entities;

namespace Bookstore.Core.Interfaces;

/// <summary>
/// Repository interface for Book operations.
/// </summary>
public interface IBookRepository : IRepository<Book>
{
    /// <summary>
    /// Gets all books in a specific category.
    /// </summary>
    /// <param name="category">Book category</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Books in the category</returns>
    Task<IEnumerable<Book>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a book by its ISBN.
    /// </summary>
    /// <param name="isbn">ISBN number</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The book or null if not found</returns>
    Task<Book?> GetByIsbnAsync(string isbn, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches books by title or author.
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Matching books</returns>
    Task<IEnumerable<Book>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all available books.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Available books</returns>
    Task<IEnumerable<Book>> GetAvailableBooksAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets recently added books.
    /// </summary>
    /// <param name="count">Number of books to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Recent books</returns>
    Task<IEnumerable<Book>> GetRecentBooksAsync(int count = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all unique categories.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of categories</returns>
    Task<IEnumerable<string>> GetCategoriesAsync(CancellationToken cancellationToken = default);
}
