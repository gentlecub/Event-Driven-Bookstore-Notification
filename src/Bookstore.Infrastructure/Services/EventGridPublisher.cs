using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.Messaging;
using Azure.Messaging.EventGrid;
using Bookstore.Core.Events;
using Bookstore.Core.Interfaces;
using Bookstore.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bookstore.Infrastructure.Services;

/// <summary>
/// Azure Event Grid implementation of IEventPublisher.
/// </summary>
public class EventGridPublisher : IEventPublisher
{
    private readonly EventGridPublisherClient _client;
    private readonly EventGridSettings _settings;
    private readonly ILogger<EventGridPublisher> _logger;

    public EventGridPublisher(
        IOptions<EventGridSettings> settings,
        ILogger<EventGridPublisher> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        var endpoint = new Uri(_settings.TopicEndpoint);

        _client = _settings.UseManagedIdentity
            ? new EventGridPublisherClient(endpoint, new DefaultAzureCredential())
            : new EventGridPublisherClient(endpoint, new AzureKeyCredential(_settings.TopicKey!));
    }

    public async Task PublishBookCreatedAsync(BookCreatedEvent bookEvent, CancellationToken cancellationToken = default)
    {
        var cloudEvent = CreateCloudEvent(bookEvent, bookEvent.Data);
        cloudEvent.Subject = $"/books/{bookEvent.Data.BookId}";

        await _client.SendEventAsync(cloudEvent, cancellationToken);

        _logger.LogInformation(
            "Published BookCreatedEvent for book {BookId} ({Title})",
            bookEvent.Data.BookId,
            bookEvent.Data.Title);
    }

    public async Task PublishBookUpdatedAsync(BookUpdatedEvent bookEvent, CancellationToken cancellationToken = default)
    {
        var cloudEvent = CreateCloudEvent(bookEvent, bookEvent.Data);
        cloudEvent.Subject = $"/books/{bookEvent.Data.BookId}";

        await _client.SendEventAsync(cloudEvent, cancellationToken);

        _logger.LogInformation(
            "Published BookUpdatedEvent for book {BookId}",
            bookEvent.Data.BookId);
    }

    public async Task PublishBookDeletedAsync(BookDeletedEvent bookEvent, CancellationToken cancellationToken = default)
    {
        var cloudEvent = CreateCloudEvent(bookEvent, bookEvent.Data);
        cloudEvent.Subject = $"/books/{bookEvent.Data.BookId}";

        await _client.SendEventAsync(cloudEvent, cancellationToken);

        _logger.LogInformation(
            "Published BookDeletedEvent for book {BookId}",
            bookEvent.Data.BookId);
    }

    public async Task PublishBatchAsync<T>(IEnumerable<T> events, CancellationToken cancellationToken = default)
        where T : BookEventBase
    {
        var cloudEvents = events.Select(e => CreateCloudEvent(e, GetEventData(e))).ToList();

        if (cloudEvents.Count == 0)
        {
            _logger.LogDebug("No events to publish in batch");
            return;
        }

        await _client.SendEventsAsync(cloudEvents, cancellationToken);

        _logger.LogInformation("Published batch of {Count} events", cloudEvents.Count);
    }

    private CloudEvent CreateCloudEvent<TData>(BookEventBase baseEvent, TData data)
    {
        return new CloudEvent(
            source: _settings.EventSource,
            type: baseEvent.Type,
            jsonSerializableData: data)
        {
            Id = baseEvent.Id,
            Time = baseEvent.Time
        };
    }

    private static object GetEventData(BookEventBase @event)
    {
        return @event switch
        {
            BookCreatedEvent created => created.Data,
            BookUpdatedEvent updated => updated.Data,
            BookDeletedEvent deleted => deleted.Data,
            _ => throw new ArgumentException($"Unknown event type: {@event.GetType().Name}")
        };
    }
}
