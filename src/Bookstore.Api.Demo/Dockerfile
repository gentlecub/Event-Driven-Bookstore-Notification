# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY *.sln ./
COPY Directory.Build.props ./
COPY src/Bookstore.Core/*.csproj ./src/Bookstore.Core/
COPY src/Bookstore.Infrastructure/*.csproj ./src/Bookstore.Infrastructure/
COPY src/Bookstore.Api.Demo/*.csproj ./src/Bookstore.Api.Demo/

# Restore dependencies
RUN dotnet restore src/Bookstore.Api.Demo/Bookstore.Api.Demo.csproj

# Copy source code
COPY src/Bookstore.Core/ ./src/Bookstore.Core/
COPY src/Bookstore.Infrastructure/ ./src/Bookstore.Infrastructure/
COPY src/Bookstore.Api.Demo/ ./src/Bookstore.Api.Demo/

# Build and publish
WORKDIR /src/src/Bookstore.Api.Demo
RUN dotnet publish -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Create non-root user for security
RUN adduser --disabled-password --gecos "" appuser

# Copy published app
COPY --from=build /app/publish .

# Set ownership
RUN chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

# Expose port (Railway uses PORT env variable)
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Entry point
ENTRYPOINT ["dotnet", "Bookstore.Api.Demo.dll"]
