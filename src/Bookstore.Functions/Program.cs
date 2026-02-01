using Bookstore.Infrastructure.Extensions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices((context, services) =>
    {
        // Application Insights
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        // Infrastructure services (Cosmos DB, Event Grid, Service Bus)
        services.AddInfrastructure(context.Configuration);

        // Add HTTP client for webhook delivery
        services.AddHttpClient();
    })
    .Build();

host.Run();
