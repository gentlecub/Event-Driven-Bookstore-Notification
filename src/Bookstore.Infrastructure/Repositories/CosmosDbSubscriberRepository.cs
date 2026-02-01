using Bookstore.Core.Entities;
using Bookstore.Core.Interfaces;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Logging;

namespace Bookstore.Infrastructure.Repositories;

/// <summary>
/// Cosmos DB implementation of ISubscriberRepository.
/// </summary>
public class CosmosDbSubscriberRepository : CosmosDbRepository<Subscriber>, ISubscriberRepository
{
    public CosmosDbSubscriberRepository(Container container, ILogger<CosmosDbSubscriberRepository> logger)
        : base(container, logger)
    {
    }

    public async Task<Subscriber?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.ToLowerInvariant();

        var query = Container.GetItemLinqQueryable<Subscriber>()
            .Where(s => s.Email.ToLower() == normalizedEmail);

        var results = await ExecuteQueryAsync(query, cancellationToken);
        return results.FirstOrDefault();
    }

    public async Task<IEnumerable<Subscriber>> GetActiveSubscribersAsync(CancellationToken cancellationToken = default)
    {
        var query = Container.GetItemLinqQueryable<Subscriber>()
            .Where(s => s.IsActive && s.ConfirmedAt != null);

        return await ExecuteQueryAsync(query, cancellationToken);
    }

    public async Task<IEnumerable<Subscriber>> GetByCategoryAsync(
        string category,
        CancellationToken cancellationToken = default)
    {
        var query = Container.GetItemLinqQueryable<Subscriber>()
            .Where(s => s.IsActive &&
                        s.ConfirmedAt != null &&
                        (s.SubscribedCategories.Count == 0 ||
                         s.SubscribedCategories.Contains(category)));

        return await ExecuteQueryAsync(query, cancellationToken);
    }

    public async Task<IEnumerable<Subscriber>> GetSubscribersForNotificationAsync(
        string bookCategory,
        CancellationToken cancellationToken = default)
    {
        // Get active, confirmed subscribers interested in this category
        var query = Container.GetItemLinqQueryable<Subscriber>()
            .Where(s => s.IsActive &&
                        s.ConfirmedAt != null &&
                        (s.SubscribedCategories.Count == 0 ||
                         s.SubscribedCategories.Contains(bookCategory)));

        return await ExecuteQueryAsync(query, cancellationToken);
    }

    public async Task UpdateLastNotificationAsync(string subscriberId, CancellationToken cancellationToken = default)
    {
        var subscriber = await GetByIdAsync(subscriberId, subscriberId, cancellationToken);
        if (subscriber == null)
        {
            Logger.LogWarning("Subscriber {SubscriberId} not found for notification update", subscriberId);
            return;
        }

        subscriber.LastNotificationAt = DateTime.UtcNow;
        subscriber.NotificationCount++;

        await UpdateAsync(subscriber, cancellationToken);
    }

    public async Task<Subscriber?> ConfirmEmailAsync(string subscriberId, CancellationToken cancellationToken = default)
    {
        var subscriber = await GetByIdAsync(subscriberId, subscriberId, cancellationToken);
        if (subscriber == null)
        {
            Logger.LogWarning("Subscriber {SubscriberId} not found for email confirmation", subscriberId);
            return null;
        }

        subscriber.ConfirmedAt = DateTime.UtcNow;

        return await UpdateAsync(subscriber, cancellationToken);
    }

    public async Task DeactivateAsync(string subscriberId, CancellationToken cancellationToken = default)
    {
        var subscriber = await GetByIdAsync(subscriberId, subscriberId, cancellationToken);
        if (subscriber == null)
        {
            Logger.LogWarning("Subscriber {SubscriberId} not found for deactivation", subscriberId);
            return;
        }

        subscriber.IsActive = false;

        await UpdateAsync(subscriber, cancellationToken);
        Logger.LogInformation("Deactivated subscriber {SubscriberId}", subscriberId);
    }
}
