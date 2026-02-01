using System.Net;
using Bookstore.Core.Entities;
using Bookstore.Core.Interfaces;
using Bookstore.Functions.DTOs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Bookstore.Functions.Functions;

/// <summary>
/// HTTP API endpoints for subscriber operations.
/// </summary>
public class SubscribersApi
{
    private readonly ISubscriberRepository _subscriberRepository;
    private readonly ILogger<SubscribersApi> _logger;

    public SubscribersApi(
        ISubscriberRepository subscriberRepository,
        ILogger<SubscribersApi> logger)
    {
        _subscriberRepository = subscriberRepository;
        _logger = logger;
    }

    [Function("GetSubscribers")]
    public async Task<HttpResponseData> GetSubscribers(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "subscribers")] HttpRequestData req,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting all active subscribers");

        var subscribers = await _subscriberRepository.GetActiveSubscribersAsync(cancellationToken);
        var response = subscribers.Select(SubscriberResponse.FromEntity).ToList();

        return await CreateJsonResponse(req, HttpStatusCode.OK, response);
    }

    [Function("GetSubscriberById")]
    public async Task<HttpResponseData> GetSubscriberById(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "subscribers/{id}")] HttpRequestData req,
        string id,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting subscriber {SubscriberId}", id);

        var subscriber = await _subscriberRepository.GetByIdAsync(id, id, cancellationToken);

        if (subscriber == null)
        {
            return await CreateJsonResponse(req, HttpStatusCode.NotFound,
                ApiResponse<SubscriberResponse>.Fail("Subscriber not found"));
        }

        return await CreateJsonResponse(req, HttpStatusCode.OK,
            ApiResponse<SubscriberResponse>.Ok(SubscriberResponse.FromEntity(subscriber)));
    }

    [Function("GetSubscriberByEmail")]
    public async Task<HttpResponseData> GetSubscriberByEmail(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "subscribers/email/{email}")] HttpRequestData req,
        string email,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting subscriber by email");

        var subscriber = await _subscriberRepository.GetByEmailAsync(email, cancellationToken);

        if (subscriber == null)
        {
            return await CreateJsonResponse(req, HttpStatusCode.NotFound,
                ApiResponse<SubscriberResponse>.Fail("Subscriber not found"));
        }

        return await CreateJsonResponse(req, HttpStatusCode.OK,
            ApiResponse<SubscriberResponse>.Ok(SubscriberResponse.FromEntity(subscriber)));
    }

    [Function("CreateSubscriber")]
    public async Task<HttpResponseData> CreateSubscriber(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "subscribers")] HttpRequestData req,
        CancellationToken cancellationToken)
    {
        var createRequest = await req.ReadFromJsonAsync<CreateSubscriberRequest>(cancellationToken);

        if (createRequest == null)
        {
            return await CreateJsonResponse(req, HttpStatusCode.BadRequest,
                ApiResponse<SubscriberResponse>.Fail("Invalid request body"));
        }

        // Check if email already exists
        var existing = await _subscriberRepository.GetByEmailAsync(createRequest.Email, cancellationToken);
        if (existing != null)
        {
            return await CreateJsonResponse(req, HttpStatusCode.Conflict,
                ApiResponse<SubscriberResponse>.Fail("Email already subscribed"));
        }

        // Parse notification preference
        if (!Enum.TryParse<NotificationPreference>(createRequest.NotificationPreference, true, out var preference))
        {
            preference = NotificationPreference.Email;
        }

        var subscriber = new Subscriber
        {
            Email = createRequest.Email.ToLowerInvariant(),
            Name = createRequest.Name,
            SubscribedCategories = createRequest.SubscribedCategories ?? [],
            NotificationPreference = preference,
            WebhookUrl = createRequest.WebhookUrl,
            IsActive = true
        };

        var created = await _subscriberRepository.CreateAsync(subscriber, cancellationToken);

        _logger.LogInformation("Created subscriber {SubscriberId} ({Email})", created.Id, created.Email);

        return await CreateJsonResponse(req, HttpStatusCode.Created,
            ApiResponse<SubscriberResponse>.Ok(SubscriberResponse.FromEntity(created)));
    }

    [Function("UpdateSubscriber")]
    public async Task<HttpResponseData> UpdateSubscriber(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "subscribers/{id}")] HttpRequestData req,
        string id,
        CancellationToken cancellationToken)
    {
        var updateRequest = await req.ReadFromJsonAsync<UpdateSubscriberRequest>(cancellationToken);

        if (updateRequest == null)
        {
            return await CreateJsonResponse(req, HttpStatusCode.BadRequest,
                ApiResponse<SubscriberResponse>.Fail("Invalid request body"));
        }

        var subscriber = await _subscriberRepository.GetByIdAsync(id, id, cancellationToken);

        if (subscriber == null)
        {
            return await CreateJsonResponse(req, HttpStatusCode.NotFound,
                ApiResponse<SubscriberResponse>.Fail("Subscriber not found"));
        }

        // Update fields
        if (updateRequest.Name != null) subscriber.Name = updateRequest.Name;
        if (updateRequest.SubscribedCategories != null) subscriber.SubscribedCategories = updateRequest.SubscribedCategories;
        if (updateRequest.WebhookUrl != null) subscriber.WebhookUrl = updateRequest.WebhookUrl;

        if (updateRequest.NotificationPreference != null &&
            Enum.TryParse<NotificationPreference>(updateRequest.NotificationPreference, true, out var preference))
        {
            subscriber.NotificationPreference = preference;
        }

        var updated = await _subscriberRepository.UpdateAsync(subscriber, cancellationToken);

        _logger.LogInformation("Updated subscriber {SubscriberId}", updated.Id);

        return await CreateJsonResponse(req, HttpStatusCode.OK,
            ApiResponse<SubscriberResponse>.Ok(SubscriberResponse.FromEntity(updated)));
    }

    [Function("UpdateSubscriberCategories")]
    public async Task<HttpResponseData> UpdateSubscriberCategories(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "subscribers/{id}/categories")] HttpRequestData req,
        string id,
        CancellationToken cancellationToken)
    {
        var updateRequest = await req.ReadFromJsonAsync<UpdateCategoriesRequest>(cancellationToken);

        if (updateRequest == null)
        {
            return await CreateJsonResponse(req, HttpStatusCode.BadRequest,
                ApiResponse<SubscriberResponse>.Fail("Invalid request body"));
        }

        var subscriber = await _subscriberRepository.GetByIdAsync(id, id, cancellationToken);

        if (subscriber == null)
        {
            return await CreateJsonResponse(req, HttpStatusCode.NotFound,
                ApiResponse<SubscriberResponse>.Fail("Subscriber not found"));
        }

        subscriber.SubscribedCategories = updateRequest.Categories;

        var updated = await _subscriberRepository.UpdateAsync(subscriber, cancellationToken);

        _logger.LogInformation("Updated categories for subscriber {SubscriberId}", updated.Id);

        return await CreateJsonResponse(req, HttpStatusCode.OK,
            ApiResponse<SubscriberResponse>.Ok(SubscriberResponse.FromEntity(updated)));
    }

    [Function("ConfirmSubscriber")]
    public async Task<HttpResponseData> ConfirmSubscriber(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "subscribers/{id}/confirm")] HttpRequestData req,
        string id,
        CancellationToken cancellationToken)
    {
        var subscriber = await _subscriberRepository.ConfirmEmailAsync(id, cancellationToken);

        if (subscriber == null)
        {
            return await CreateJsonResponse(req, HttpStatusCode.NotFound,
                ApiResponse<SubscriberResponse>.Fail("Subscriber not found"));
        }

        _logger.LogInformation("Confirmed subscriber {SubscriberId}", id);

        return await CreateJsonResponse(req, HttpStatusCode.OK,
            ApiResponse<SubscriberResponse>.Ok(SubscriberResponse.FromEntity(subscriber)));
    }

    [Function("UnsubscribeSubscriber")]
    public async Task<HttpResponseData> UnsubscribeSubscriber(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "subscribers/{id}/unsubscribe")] HttpRequestData req,
        string id,
        CancellationToken cancellationToken)
    {
        var subscriber = await _subscriberRepository.GetByIdAsync(id, id, cancellationToken);

        if (subscriber == null)
        {
            return await CreateJsonResponse(req, HttpStatusCode.NotFound,
                ApiResponse<object>.Fail("Subscriber not found"));
        }

        await _subscriberRepository.DeactivateAsync(id, cancellationToken);

        _logger.LogInformation("Unsubscribed subscriber {SubscriberId}", id);

        return req.CreateResponse(HttpStatusCode.NoContent);
    }

    [Function("DeleteSubscriber")]
    public async Task<HttpResponseData> DeleteSubscriber(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "subscribers/{id}")] HttpRequestData req,
        string id,
        CancellationToken cancellationToken)
    {
        var exists = await _subscriberRepository.ExistsAsync(id, id, cancellationToken);

        if (!exists)
        {
            return await CreateJsonResponse(req, HttpStatusCode.NotFound,
                ApiResponse<object>.Fail("Subscriber not found"));
        }

        await _subscriberRepository.DeleteAsync(id, id, cancellationToken);

        _logger.LogInformation("Deleted subscriber {SubscriberId}", id);

        return req.CreateResponse(HttpStatusCode.NoContent);
    }

    private static async Task<HttpResponseData> CreateJsonResponse<T>(
        HttpRequestData req,
        HttpStatusCode statusCode,
        T body)
    {
        var response = req.CreateResponse(statusCode);
        await response.WriteAsJsonAsync(body);
        return response;
    }
}
