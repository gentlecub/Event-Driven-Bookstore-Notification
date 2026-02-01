using System.Net;
using Bookstore.Core.Entities;
using Bookstore.Core.Interfaces;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Logging;

namespace Bookstore.Infrastructure.Repositories;

/// <summary>
/// Generic Cosmos DB repository implementation.
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public abstract class CosmosDbRepository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly Container Container;
    protected readonly ILogger Logger;

    protected CosmosDbRepository(Container container, ILogger logger)
    {
        Container = container;
        Logger = logger;
    }

    public virtual async Task<T?> GetByIdAsync(
        string id,
        string partitionKey,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await Container.ReadItemAsync<T>(
                id,
                new PartitionKey(partitionKey),
                cancellationToken: cancellationToken);

            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            Logger.LogDebug("Entity {Id} not found in partition {PartitionKey}", id, partitionKey);
            return null;
        }
    }

    public virtual async Task<IEnumerable<T>> GetByPartitionKeyAsync(
        string partitionKey,
        CancellationToken cancellationToken = default)
    {
        var query = Container.GetItemLinqQueryable<T>(
            requestOptions: new QueryRequestOptions
            {
                PartitionKey = new PartitionKey(partitionKey)
            });

        return await ExecuteQueryAsync(query, cancellationToken);
    }

    public virtual async Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default)
    {
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;

        var response = await Container.CreateItemAsync(
            entity,
            new PartitionKey(entity.PartitionKey),
            cancellationToken: cancellationToken);

        Logger.LogInformation("Created entity {Id} in partition {PartitionKey}", entity.Id, entity.PartitionKey);

        return response.Resource;
    }

    public virtual async Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;

        var options = new ItemRequestOptions();
        if (!string.IsNullOrEmpty(entity.ETag))
        {
            options.IfMatchEtag = entity.ETag;
        }

        var response = await Container.ReplaceItemAsync(
            entity,
            entity.Id,
            new PartitionKey(entity.PartitionKey),
            options,
            cancellationToken);

        Logger.LogInformation("Updated entity {Id} in partition {PartitionKey}", entity.Id, entity.PartitionKey);

        return response.Resource;
    }

    public virtual async Task DeleteAsync(
        string id,
        string partitionKey,
        CancellationToken cancellationToken = default)
    {
        await Container.DeleteItemAsync<T>(
            id,
            new PartitionKey(partitionKey),
            cancellationToken: cancellationToken);

        Logger.LogInformation("Deleted entity {Id} from partition {PartitionKey}", id, partitionKey);
    }

    public virtual async Task<bool> ExistsAsync(
        string id,
        string partitionKey,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await Container.ReadItemAsync<T>(
                id,
                new PartitionKey(partitionKey),
                cancellationToken: cancellationToken);
            return true;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    public virtual async Task<IEnumerable<T>> QueryAsync(
        Func<IQueryable<T>, IQueryable<T>> predicate,
        CancellationToken cancellationToken = default)
    {
        var queryable = Container.GetItemLinqQueryable<T>();
        var query = predicate(queryable);

        return await ExecuteQueryAsync(query, cancellationToken);
    }

    protected async Task<List<T>> ExecuteQueryAsync(
        IQueryable<T> query,
        CancellationToken cancellationToken = default)
    {
        var results = new List<T>();
        using var iterator = query.ToFeedIterator();

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync(cancellationToken);
            results.AddRange(response);
        }

        return results;
    }

    protected async Task<List<TResult>> ExecuteQueryAsync<TResult>(
        IQueryable<TResult> query,
        CancellationToken cancellationToken = default)
    {
        var results = new List<TResult>();
        using var iterator = query.ToFeedIterator();

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync(cancellationToken);
            results.AddRange(response);
        }

        return results;
    }
}
