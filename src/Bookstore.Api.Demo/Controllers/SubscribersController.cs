using Bookstore.Core.Entities;
using Bookstore.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Bookstore.Api.Demo.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class SubscribersController : ControllerBase
{
    private readonly ISubscriberRepository _subscriberRepository;
    private readonly ILogger<SubscribersController> _logger;

    public SubscribersController(ISubscriberRepository subscriberRepository, ILogger<SubscribersController> logger)
    {
        _subscriberRepository = subscriberRepository;
        _logger = logger;
    }

    /// <summary>
    /// Gets all subscribers.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Subscriber>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Subscriber>>> GetSubscribers()
    {
        var subscribers = await _subscriberRepository.QueryAsync(q => q);
        return Ok(subscribers);
    }

    /// <summary>
    /// Gets a subscriber by ID.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Subscriber), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Subscriber>> GetSubscriberById(string id)
    {
        var subscriber = await _subscriberRepository.GetByIdAsync(id, id);
        if (subscriber == null)
            return NotFound(new { message = "Subscriber not found" });

        return Ok(subscriber);
    }

    /// <summary>
    /// Gets a subscriber by email.
    /// </summary>
    [HttpGet("email/{email}")]
    [ProducesResponseType(typeof(Subscriber), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Subscriber>> GetSubscriberByEmail(string email)
    {
        var subscriber = await _subscriberRepository.GetByEmailAsync(email);
        if (subscriber == null)
            return NotFound(new { message = "Subscriber not found" });

        return Ok(subscriber);
    }

    /// <summary>
    /// Gets active subscribers.
    /// </summary>
    [HttpGet("active")]
    [ProducesResponseType(typeof(IEnumerable<Subscriber>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Subscriber>>> GetActiveSubscribers()
    {
        var subscribers = await _subscriberRepository.GetActiveSubscribersAsync();
        return Ok(subscribers);
    }

    /// <summary>
    /// Gets subscribers interested in a category.
    /// </summary>
    [HttpGet("category/{category}")]
    [ProducesResponseType(typeof(IEnumerable<Subscriber>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Subscriber>>> GetSubscribersByCategory(string category)
    {
        var subscribers = await _subscriberRepository.GetByCategoryAsync(category);
        return Ok(subscribers);
    }

    /// <summary>
    /// Creates a new subscriber.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Subscriber), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Subscriber>> CreateSubscriber([FromBody] CreateSubscriberRequest request)
    {
        // Check for duplicate email
        var existing = await _subscriberRepository.GetByEmailAsync(request.Email);
        if (existing != null)
            return BadRequest(new { message = "A subscriber with this email already exists" });

        var subscriber = new Subscriber
        {
            Email = request.Email,
            Name = request.Name,
            IsActive = true,
            SubscribedCategories = request.Categories ?? [],
            NotificationPreference = request.NotificationPreference,
            WebhookUrl = request.WebhookUrl
        };

        var created = await _subscriberRepository.CreateAsync(subscriber);
        _logger.LogInformation("Subscriber created: {SubscriberId} - {Email}", created.Id, created.Email);

        return CreatedAtAction(nameof(GetSubscriberById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Updates a subscriber.
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(Subscriber), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Subscriber>> UpdateSubscriber(string id, [FromBody] UpdateSubscriberRequest request)
    {
        var subscriber = await _subscriberRepository.GetByIdAsync(id, id);
        if (subscriber == null)
            return NotFound(new { message = "Subscriber not found" });

        if (request.Name != null) subscriber.Name = request.Name;
        if (request.Categories != null) subscriber.SubscribedCategories = request.Categories;
        if (request.NotificationPreference.HasValue) subscriber.NotificationPreference = request.NotificationPreference.Value;
        if (request.WebhookUrl != null) subscriber.WebhookUrl = request.WebhookUrl;

        var updated = await _subscriberRepository.UpdateAsync(subscriber);
        _logger.LogInformation("Subscriber updated: {SubscriberId}", updated.Id);

        return Ok(updated);
    }

    /// <summary>
    /// Updates subscriber categories.
    /// </summary>
    [HttpPut("{id}/categories")]
    [ProducesResponseType(typeof(Subscriber), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Subscriber>> UpdateSubscriberCategories(string id, [FromBody] List<string> categories)
    {
        var subscriber = await _subscriberRepository.GetByIdAsync(id, id);
        if (subscriber == null)
            return NotFound(new { message = "Subscriber not found" });

        subscriber.SubscribedCategories = categories;
        var updated = await _subscriberRepository.UpdateAsync(subscriber);

        return Ok(updated);
    }

    /// <summary>
    /// Confirms a subscriber's email.
    /// </summary>
    [HttpPost("{id}/confirm")]
    [ProducesResponseType(typeof(Subscriber), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Subscriber>> ConfirmSubscriber(string id)
    {
        var subscriber = await _subscriberRepository.ConfirmEmailAsync(id);
        if (subscriber == null)
            return NotFound(new { message = "Subscriber not found" });

        _logger.LogInformation("Subscriber confirmed: {SubscriberId}", id);
        return Ok(subscriber);
    }

    /// <summary>
    /// Unsubscribes (deactivates) a subscriber.
    /// </summary>
    [HttpPost("{id}/unsubscribe")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnsubscribeSubscriber(string id)
    {
        var exists = await _subscriberRepository.ExistsAsync(id, id);
        if (!exists)
            return NotFound(new { message = "Subscriber not found" });

        await _subscriberRepository.DeactivateAsync(id);
        _logger.LogInformation("Subscriber unsubscribed: {SubscriberId}", id);

        return Ok(new { message = "Successfully unsubscribed" });
    }

    /// <summary>
    /// Deletes a subscriber.
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSubscriber(string id)
    {
        var exists = await _subscriberRepository.ExistsAsync(id, id);
        if (!exists)
            return NotFound(new { message = "Subscriber not found" });

        await _subscriberRepository.DeleteAsync(id, id);
        _logger.LogInformation("Subscriber deleted: {SubscriberId}", id);

        return NoContent();
    }
}

public record CreateSubscriberRequest(
    string Email,
    string Name,
    List<string>? Categories = null,
    NotificationPreference NotificationPreference = NotificationPreference.Email,
    string? WebhookUrl = null
);

public record UpdateSubscriberRequest(
    string? Name = null,
    List<string>? Categories = null,
    NotificationPreference? NotificationPreference = null,
    string? WebhookUrl = null
);
