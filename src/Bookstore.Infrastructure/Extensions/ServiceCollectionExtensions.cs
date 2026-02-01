using Azure.Identity;
using Bookstore.Core.Interfaces;
using Bookstore.Infrastructure.Configuration;
using Bookstore.Infrastructure.Repositories;
using Bookstore.Infrastructure.Services;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Bookstore.Infrastructure.Extensions;

/// <summary>
/// Extension methods for registering infrastructure services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds all infrastructure services to the DI container.
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configuration
        services.Configure<CosmosDbSettings>(configuration.GetSection(CosmosDbSettings.SectionName));
        services.Configure<EventGridSettings>(configuration.GetSection(EventGridSettings.SectionName));
        services.Configure<ServiceBusSettings>(configuration.GetSection(ServiceBusSettings.SectionName));

        // Cosmos DB
        services.AddCosmosDb(configuration);

        // Repositories
        services.AddScoped<IBookRepository, CosmosDbBookRepository>();
        services.AddScoped<ISubscriberRepository, CosmosDbSubscriberRepository>();

        // Services
        services.AddSingleton<IEventPublisher, EventGridPublisher>();
        services.AddSingleton<IMessagePublisher, ServiceBusPublisher>();
        services.AddScoped<INotificationService, NotificationService>();

        return services;
    }

    /// <summary>
    /// Adds Cosmos DB client and containers.
    /// </summary>
    private static IServiceCollection AddCosmosDb(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<CosmosDbSettings>>().Value;

            var options = new CosmosClientOptions
            {
                SerializerOptions = new CosmosSerializationOptions
                {
                    PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                },
                ConnectionMode = ConnectionMode.Direct,
                ApplicationName = "BookstoreNotification"
            };

            return settings.UseManagedIdentity
                ? new CosmosClient(settings.Endpoint, new DefaultAzureCredential(), options)
                : new CosmosClient(settings.Endpoint, settings.PrimaryKey, options);
        });

        // Register containers
        services.AddScoped(sp =>
        {
            var client = sp.GetRequiredService<CosmosClient>();
            var settings = sp.GetRequiredService<IOptions<CosmosDbSettings>>().Value;
            return client.GetContainer(settings.DatabaseName, settings.BooksContainerName);
        });

        // Register Book repository with its container
        services.AddScoped<CosmosDbBookRepository>(sp =>
        {
            var client = sp.GetRequiredService<CosmosClient>();
            var settings = sp.GetRequiredService<IOptions<CosmosDbSettings>>().Value;
            var container = client.GetContainer(settings.DatabaseName, settings.BooksContainerName);
            var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<CosmosDbBookRepository>>();
            return new CosmosDbBookRepository(container, logger);
        });

        // Register Subscriber repository with its container
        services.AddScoped<CosmosDbSubscriberRepository>(sp =>
        {
            var client = sp.GetRequiredService<CosmosClient>();
            var settings = sp.GetRequiredService<IOptions<CosmosDbSettings>>().Value;
            var container = client.GetContainer(settings.DatabaseName, settings.SubscribersContainerName);
            var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<CosmosDbSubscriberRepository>>();
            return new CosmosDbSubscriberRepository(container, logger);
        });

        return services;
    }
}
