using Bookstore.Core.Entities;

namespace Bookstore.Core.Interfaces;

/// <summary>
/// Generic repository interface for Cosmos DB operations.
/// </summary>
/// <typeparam name="T">Entity type derived from BaseEntity</typeparam>
public interface IRepository<T> where T : BaseEntity
{
    /// <summary>
    /// Gets an entity by its ID and partition key.
    /// </summary>
    /// <param name="id">Document ID</param>
    /// <param name="partitionKey">Partition key value</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The entity or null if not found</returns>
    Task<T?> GetByIdAsync(string id, string partitionKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all entities in a partition.
    /// </summary>
    /// <param name="partitionKey">Partition key value</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of entities</returns>
    Task<IEnumerable<T>> GetByPartitionKeyAsync(string partitionKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new entity.
    /// </summary>
    /// <param name="entity">Entity to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created entity</returns>
    Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing entity.
    /// </summary>
    /// <param name="entity">Entity to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated entity</returns>
    Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an entity.
    /// </summary>
    /// <param name="id">Document ID</param>
    /// <param name="partitionKey">Partition key value</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DeleteAsync(string id, string partitionKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an entity exists.
    /// </summary>
    /// <param name="id">Document ID</param>
    /// <param name="partitionKey">Partition key value</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if exists</returns>
    Task<bool> ExistsAsync(string id, string partitionKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Queries entities using a predicate.
    /// </summary>
    /// <param name="predicate">Filter predicate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Matching entities</returns>
    Task<IEnumerable<T>> QueryAsync(
        Func<IQueryable<T>, IQueryable<T>> predicate,
        CancellationToken cancellationToken = default);
}
