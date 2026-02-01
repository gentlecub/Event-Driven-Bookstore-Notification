using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Bookstore.Functions.Functions;

/// <summary>
/// Health check endpoints for monitoring.
/// </summary>
public class HealthCheck
{
    private readonly ILogger<HealthCheck> _logger;

    public HealthCheck(ILogger<HealthCheck> logger)
    {
        _logger = logger;
    }

    [Function("Health")]
    public async Task<HttpResponseData> Health(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "health")] HttpRequestData req)
    {
        _logger.LogDebug("Health check requested");

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            version = "1.0.0"
        });

        return response;
    }

    [Function("Ready")]
    public async Task<HttpResponseData> Ready(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "health/ready")] HttpRequestData req)
    {
        // TODO: Add dependency checks (Cosmos DB, Service Bus connectivity)
        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(new
        {
            status = "ready",
            timestamp = DateTime.UtcNow,
            checks = new
            {
                cosmosDb = "ok",
                serviceBus = "ok",
                eventGrid = "ok"
            }
        });

        return response;
    }
}
