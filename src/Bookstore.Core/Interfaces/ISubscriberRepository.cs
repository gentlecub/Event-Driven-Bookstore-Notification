using Bookstore.Core.Entities;

namespace Bookstore.Core.Interfaces;

/// <summary>
/// Repository interface for Subscriber operations.
/// </summary>
public interface ISubscriberRepository : IRepository<Subscriber>
{
    /// <summary>
    /// Gets a subscriber by email address.
    /// </summary>
    /// <param name="email">Email address</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The subscriber or null if not found</returns>
    Task<Subscriber?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active subscribers.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Active subscribers</returns>
    Task<IEnumerable<Subscriber>> GetActiveSubscribersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets subscribers interested in a specific category.
    /// </summary>
    /// <param name="category">Book category</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Matching subscribers</returns>
    Task<IEnumerable<Subscriber>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets subscribers who should receive a notification for a new book.
    /// Returns active, confirmed subscribers interested in the book's category.
    /// </summary>
    /// <param name="bookCategory">Category of the new book</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Subscribers to notify</returns>
    Task<IEnumerable<Subscriber>> GetSubscribersForNotificationAsync(
        string bookCategory,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the last notification timestamp for a subscriber.
    /// </summary>
    /// <param name="subscriberId">Subscriber ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task UpdateLastNotificationAsync(string subscriberId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Confirms a subscriber's email.
    /// </summary>
    /// <param name="subscriberId">Subscriber ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated subscriber</returns>
    Task<Subscriber?> ConfirmEmailAsync(string subscriberId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deactivates a subscriber (unsubscribe).
    /// </summary>
    /// <param name="subscriberId">Subscriber ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DeactivateAsync(string subscriberId, CancellationToken cancellationToken = default);
}
