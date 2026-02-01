using Bookstore.Core.Events;

namespace Bookstore.Core.Interfaces;

/// <summary>
/// Interface for publishing events to the event grid.
/// </summary>
public interface IEventPublisher
{
    /// <summary>
    /// Publishes a book created event.
    /// </summary>
    /// <param name="bookEvent">The event to publish</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task PublishBookCreatedAsync(BookCreatedEvent bookEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Publishes a book updated event.
    /// </summary>
    /// <param name="bookEvent">The event to publish</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task PublishBookUpdatedAsync(BookUpdatedEvent bookEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Publishes a book deleted event.
    /// </summary>
    /// <param name="bookEvent">The event to publish</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task PublishBookDeletedAsync(BookDeletedEvent bookEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Publishes multiple events in a batch.
    /// </summary>
    /// <typeparam name="T">Event type</typeparam>
    /// <param name="events">Events to publish</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task PublishBatchAsync<T>(IEnumerable<T> events, CancellationToken cancellationToken = default)
        where T : BookEventBase;
}
