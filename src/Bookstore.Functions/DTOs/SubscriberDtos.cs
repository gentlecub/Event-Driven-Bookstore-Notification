using Bookstore.Core.Entities;

namespace Bookstore.Functions.DTOs;

/// <summary>
/// Request DTO for creating a subscriber.
/// </summary>
public record CreateSubscriberRequest(
    string Email,
    string Name,
    List<string>? SubscribedCategories = null,
    string NotificationPreference = "Email",
    string? WebhookUrl = null);

/// <summary>
/// Request DTO for updating a subscriber.
/// </summary>
public record UpdateSubscriberRequest(
    string? Name,
    List<string>? SubscribedCategories,
    string? NotificationPreference,
    string? WebhookUrl);

/// <summary>
/// Request DTO for updating subscriber categories.
/// </summary>
public record UpdateCategoriesRequest(List<string> Categories);

/// <summary>
/// Response DTO for subscriber data.
/// </summary>
public record SubscriberResponse(
    string Id,
    string Email,
    string Name,
    bool IsActive,
    List<string> SubscribedCategories,
    string NotificationPreference,
    string? WebhookUrl,
    DateTime? LastNotificationAt,
    int NotificationCount,
    bool IsConfirmed,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    public static SubscriberResponse FromEntity(Subscriber subscriber) => new(
        subscriber.Id,
        subscriber.Email,
        subscriber.Name,
        subscriber.IsActive,
        subscriber.SubscribedCategories,
        subscriber.NotificationPreference.ToString(),
        subscriber.WebhookUrl,
        subscriber.LastNotificationAt,
        subscriber.NotificationCount,
        subscriber.IsConfirmed,
        subscriber.CreatedAt,
        subscriber.UpdatedAt);
}
