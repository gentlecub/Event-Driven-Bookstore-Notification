using Bookstore.Core.Entities;
using Bookstore.Core.Interfaces;
using System.Collections.Concurrent;

namespace Bookstore.Api.Demo.Repositories;

public class InMemorySubscriberRepository : ISubscriberRepository
{
    private readonly ConcurrentDictionary<string, Subscriber> _subscribers = new();

    public InMemorySubscriberRepository()
    {
        SeedData();
    }

    private void SeedData()
    {
        var sampleSubscribers = new List<Subscriber>
        {
            new()
            {
                Id = Guid.NewGuid().ToString(),
                Email = "tech.enthusiast@example.com",
                Name = "Alex Developer",
                IsActive = true,
                SubscribedCategories = ["Technology"],
                NotificationPreference = NotificationPreference.Email,
                ConfirmedAt = DateTime.UtcNow.AddDays(-30)
            },
            new()
            {
                Id = Guid.NewGuid().ToString(),
                Email = "bookworm@example.com",
                Name = "Sarah Reader",
                IsActive = true,
                SubscribedCategories = ["Fiction", "Non-Fiction"],
                NotificationPreference = NotificationPreference.Email,
                ConfirmedAt = DateTime.UtcNow.AddDays(-15)
            },
            new()
            {
                Id = Guid.NewGuid().ToString(),
                Email = "all.books@example.com",
                Name = "Chris Librarian",
                IsActive = true,
                SubscribedCategories = [], // Empty = all categories
                NotificationPreference = NotificationPreference.Webhook,
                WebhookUrl = "https://webhook.site/example",
                ConfirmedAt = DateTime.UtcNow.AddDays(-60)
            }
        };

        foreach (var subscriber in sampleSubscribers)
        {
            _subscribers.TryAdd(subscriber.Id, subscriber);
        }
    }

    public Task<Subscriber?> GetByIdAsync(string id, string partitionKey, CancellationToken cancellationToken = default)
    {
        _subscribers.TryGetValue(id, out var subscriber);
        return Task.FromResult(subscriber);
    }

    public Task<IEnumerable<Subscriber>> GetByPartitionKeyAsync(string partitionKey, CancellationToken cancellationToken = default)
    {
        // For subscribers, partition key is the id
        var subscribers = _subscribers.Values.Where(s => s.Id == partitionKey);
        return Task.FromResult(subscribers);
    }

    public Task<Subscriber> CreateAsync(Subscriber entity, CancellationToken cancellationToken = default)
    {
        entity.Id = Guid.NewGuid().ToString();
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        _subscribers.TryAdd(entity.Id, entity);
        return Task.FromResult(entity);
    }

    public Task<Subscriber> UpdateAsync(Subscriber entity, CancellationToken cancellationToken = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _subscribers[entity.Id] = entity;
        return Task.FromResult(entity);
    }

    public Task DeleteAsync(string id, string partitionKey, CancellationToken cancellationToken = default)
    {
        _subscribers.TryRemove(id, out _);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string id, string partitionKey, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_subscribers.ContainsKey(id));
    }

    public Task<IEnumerable<Subscriber>> QueryAsync(Func<IQueryable<Subscriber>, IQueryable<Subscriber>> predicate, CancellationToken cancellationToken = default)
    {
        var result = predicate(_subscribers.Values.AsQueryable()).AsEnumerable();
        return Task.FromResult(result);
    }

    public Task<Subscriber?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var subscriber = _subscribers.Values.FirstOrDefault(s =>
            s.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(subscriber);
    }

    public Task<IEnumerable<Subscriber>> GetActiveSubscribersAsync(CancellationToken cancellationToken = default)
    {
        var subscribers = _subscribers.Values.Where(s => s.IsActive);
        return Task.FromResult(subscribers);
    }

    public Task<IEnumerable<Subscriber>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default)
    {
        var subscribers = _subscribers.Values.Where(s =>
            s.SubscribedCategories.Count == 0 || // Empty = all categories
            s.SubscribedCategories.Contains(category, StringComparer.OrdinalIgnoreCase));
        return Task.FromResult(subscribers);
    }

    public Task<IEnumerable<Subscriber>> GetSubscribersForNotificationAsync(string bookCategory, CancellationToken cancellationToken = default)
    {
        var subscribers = _subscribers.Values.Where(s =>
            s.IsActive &&
            s.IsConfirmed &&
            (s.SubscribedCategories.Count == 0 ||
             s.SubscribedCategories.Contains(bookCategory, StringComparer.OrdinalIgnoreCase)));
        return Task.FromResult(subscribers);
    }

    public Task UpdateLastNotificationAsync(string subscriberId, CancellationToken cancellationToken = default)
    {
        if (_subscribers.TryGetValue(subscriberId, out var subscriber))
        {
            subscriber.LastNotificationAt = DateTime.UtcNow;
            subscriber.NotificationCount++;
            subscriber.UpdatedAt = DateTime.UtcNow;
        }
        return Task.CompletedTask;
    }

    public Task<Subscriber?> ConfirmEmailAsync(string subscriberId, CancellationToken cancellationToken = default)
    {
        if (_subscribers.TryGetValue(subscriberId, out var subscriber))
        {
            subscriber.ConfirmedAt = DateTime.UtcNow;
            subscriber.UpdatedAt = DateTime.UtcNow;
            return Task.FromResult<Subscriber?>(subscriber);
        }
        return Task.FromResult<Subscriber?>(null);
    }

    public Task DeactivateAsync(string subscriberId, CancellationToken cancellationToken = default)
    {
        if (_subscribers.TryGetValue(subscriberId, out var subscriber))
        {
            subscriber.IsActive = false;
            subscriber.UpdatedAt = DateTime.UtcNow;
        }
        return Task.CompletedTask;
    }
}
