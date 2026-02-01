using Bookstore.Core.Entities;
using Bookstore.Core.Interfaces;
using Bookstore.Core.Messages;
using Microsoft.Extensions.Logging;

namespace Bookstore.Infrastructure.Services;

/// <summary>
/// Orchestrates notification delivery to subscribers.
/// </summary>
public class NotificationService : INotificationService
{
    private readonly IBookRepository _bookRepository;
    private readonly ISubscriberRepository _subscriberRepository;
    private readonly IMessagePublisher _messagePublisher;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IBookRepository bookRepository,
        ISubscriberRepository subscriberRepository,
        IMessagePublisher messagePublisher,
        ILogger<NotificationService> logger)
    {
        _bookRepository = bookRepository;
        _subscriberRepository = subscriberRepository;
        _messagePublisher = messagePublisher;
        _logger = logger;
    }

    public async Task<int> NotifySubscribersAsync(
        string bookId,
        string category,
        CancellationToken cancellationToken = default)
    {
        // Get book details
        var book = await _bookRepository.GetByIdAsync(bookId, category, cancellationToken);
        if (book == null)
        {
            _logger.LogWarning("Book {BookId} not found for notification", bookId);
            return 0;
        }

        // Get subscribers interested in this category
        var subscribers = await _subscriberRepository
            .GetSubscribersForNotificationAsync(category, cancellationToken);

        var subscriberList = subscribers.ToList();
        if (subscriberList.Count == 0)
        {
            _logger.LogInformation("No subscribers found for category {Category}", category);
            return 0;
        }

        // Create notification messages
        var messages = subscriberList.Select(s => CreateNotificationMessage(book, s)).ToList();

        // Send messages to queue
        await _messagePublisher.SendNotificationBatchAsync(messages, cancellationToken);

        _logger.LogInformation(
            "Queued {Count} notifications for book {BookId} ({Title})",
            messages.Count,
            bookId,
            book.Title);

        return messages.Count;
    }

    public async Task<NotificationResult> DeliverNotificationAsync(
        NewBookNotificationMessage message,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Here you would integrate with actual email/webhook delivery
            // For now, we'll simulate successful delivery
            _logger.LogInformation(
                "Delivering notification to {Email} for book {BookTitle}",
                message.Subscriber.Email,
                message.Book.Title);

            // Update subscriber's last notification timestamp
            await _subscriberRepository.UpdateLastNotificationAsync(
                message.Subscriber.SubscriberId,
                cancellationToken);

            // Simulate delivery based on preference
            var result = message.Subscriber.NotificationPreference switch
            {
                "Email" => await DeliverEmailAsync(message, cancellationToken),
                "Webhook" => await DeliverWebhookAsync(message, cancellationToken),
                "Both" => await DeliverBothAsync(message, cancellationToken),
                _ => new NotificationResult
                {
                    Success = false,
                    ErrorMessage = $"Unknown notification preference: {message.Subscriber.NotificationPreference}"
                }
            };

            result.SubscriberId = message.Subscriber.SubscriberId;
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to deliver notification to {SubscriberId}",
                message.Subscriber.SubscriberId);

            return new NotificationResult
            {
                Success = false,
                SubscriberId = message.Subscriber.SubscriberId,
                ErrorMessage = ex.Message
            };
        }
    }

    private static NewBookNotificationMessage CreateNotificationMessage(Book book, Subscriber subscriber)
    {
        return new NewBookNotificationMessage
        {
            CorrelationId = book.Id,
            Book = new BookNotificationData
            {
                BookId = book.Id,
                Title = book.Title,
                Author = book.Author,
                Isbn = book.Isbn,
                Category = book.Category,
                Description = book.Description,
                Price = book.Price
            },
            Subscriber = new SubscriberNotificationData
            {
                SubscriberId = subscriber.Id,
                Email = subscriber.Email,
                Name = subscriber.Name,
                NotificationPreference = subscriber.NotificationPreference.ToString(),
                WebhookUrl = subscriber.WebhookUrl
            }
        };
    }

    private Task<NotificationResult> DeliverEmailAsync(
        NewBookNotificationMessage message,
        CancellationToken cancellationToken)
    {
        // TODO: Integrate with email service (SendGrid, Azure Communication Services, etc.)
        _logger.LogInformation(
            "Simulating email delivery to {Email}",
            message.Subscriber.Email);

        return Task.FromResult(new NotificationResult
        {
            Success = true,
            DeliveryMethod = "Email"
        });
    }

    private Task<NotificationResult> DeliverWebhookAsync(
        NewBookNotificationMessage message,
        CancellationToken cancellationToken)
    {
        // TODO: Implement webhook delivery
        if (string.IsNullOrEmpty(message.Subscriber.WebhookUrl))
        {
            return Task.FromResult(new NotificationResult
            {
                Success = false,
                DeliveryMethod = "Webhook",
                ErrorMessage = "Webhook URL not configured"
            });
        }

        _logger.LogInformation(
            "Simulating webhook delivery to {WebhookUrl}",
            message.Subscriber.WebhookUrl);

        return Task.FromResult(new NotificationResult
        {
            Success = true,
            DeliveryMethod = "Webhook"
        });
    }

    private async Task<NotificationResult> DeliverBothAsync(
        NewBookNotificationMessage message,
        CancellationToken cancellationToken)
    {
        var emailResult = await DeliverEmailAsync(message, cancellationToken);
        var webhookResult = await DeliverWebhookAsync(message, cancellationToken);

        return new NotificationResult
        {
            Success = emailResult.Success && webhookResult.Success,
            DeliveryMethod = "Both",
            ErrorMessage = !emailResult.Success ? emailResult.ErrorMessage :
                          !webhookResult.Success ? webhookResult.ErrorMessage : null
        };
    }
}
