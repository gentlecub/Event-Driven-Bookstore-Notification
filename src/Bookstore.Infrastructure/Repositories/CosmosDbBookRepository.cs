using Bookstore.Core.Entities;
using Bookstore.Core.Interfaces;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Logging;

namespace Bookstore.Infrastructure.Repositories;

/// <summary>
/// Cosmos DB implementation of IBookRepository.
/// </summary>
public class CosmosDbBookRepository : CosmosDbRepository<Book>, IBookRepository
{
    public CosmosDbBookRepository(Container container, ILogger<CosmosDbBookRepository> logger)
        : base(container, logger)
    {
    }

    public async Task<IEnumerable<Book>> GetByCategoryAsync(
        string category,
        CancellationToken cancellationToken = default)
    {
        return await GetByPartitionKeyAsync(category, cancellationToken);
    }

    public async Task<Book?> GetByIsbnAsync(string isbn, CancellationToken cancellationToken = default)
    {
        var query = Container.GetItemLinqQueryable<Book>()
            .Where(b => b.Isbn == isbn);

        var results = await ExecuteQueryAsync(query, cancellationToken);
        return results.FirstOrDefault();
    }

    public async Task<IEnumerable<Book>> SearchAsync(
        string searchTerm,
        CancellationToken cancellationToken = default)
    {
        var lowerSearch = searchTerm.ToLowerInvariant();

        var query = Container.GetItemLinqQueryable<Book>()
            .Where(b => b.Title.ToLower().Contains(lowerSearch) ||
                        b.Author.ToLower().Contains(lowerSearch) ||
                        (b.Description != null && b.Description.ToLower().Contains(lowerSearch)));

        return await ExecuteQueryAsync(query, cancellationToken);
    }

    public async Task<IEnumerable<Book>> GetAvailableBooksAsync(CancellationToken cancellationToken = default)
    {
        var query = Container.GetItemLinqQueryable<Book>()
            .Where(b => b.IsAvailable && b.StockQuantity > 0);

        return await ExecuteQueryAsync(query, cancellationToken);
    }

    public async Task<IEnumerable<Book>> GetRecentBooksAsync(
        int count = 10,
        CancellationToken cancellationToken = default)
    {
        var query = Container.GetItemLinqQueryable<Book>()
            .OrderByDescending(b => b.CreatedAt)
            .Take(count);

        return await ExecuteQueryAsync(query, cancellationToken);
    }

    public async Task<IEnumerable<string>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var query = Container.GetItemLinqQueryable<Book>()
            .Select(b => b.Category)
            .Distinct();

        return await ExecuteQueryAsync(query, cancellationToken);
    }
}
