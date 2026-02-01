using Bookstore.Api.Demo.Repositories;
using Bookstore.Core.Interfaces;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Configure JSON serialization
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Bookstore Notification API - Demo",
        Version = "v1",
        Description = "Event-Driven Bookstore Notification System API Demo. " +
                      "This demo uses in-memory storage and simulates the Azure-based production system.",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Bookstore API",
            Url = new Uri("https://github.com/your-repo")
        }
    });
});

// Register in-memory repositories (singleton for data persistence during app lifetime)
builder.Services.AddSingleton<IBookRepository, InMemoryBookRepository>();
builder.Services.AddSingleton<ISubscriberRepository, InMemorySubscriberRepository>();

// Configure CORS for demo
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Configure port from environment variable (for Railway)
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

var app = builder.Build();

// Always enable Swagger for demo
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Bookstore API v1");
    c.RoutePrefix = "swagger";
});

app.UseCors();

// Serve static files (frontend)
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

// Root redirect to frontend
app.MapGet("/", () => Results.Redirect("/index.html"));

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

app.Run();
