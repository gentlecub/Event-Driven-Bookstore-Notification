# Event-Driven Bookstore Notification System

A cloud-native, event-driven architecture built on Microsoft Azure for managing bookstore inventory and notifying subscribers in real time when new books are added.

## Architecture Overview

```
┌─────────────┐     ┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│   Client    │────▶│    APIM     │────▶│   Azure     │────▶│  Cosmos DB  │
│  (API Call) │     │   Gateway   │     │  Functions  │     │  (Books/    │
└─────────────┘     └─────────────┘     └─────────────┘     │ Subscribers)│
                                               │            └─────────────┘
                                               │ Publish Event
                                               ▼
                                        ┌─────────────┐
                                        │ Event Grid  │
                                        │   Topic     │
                                        └──────┬──────┘
                                               │
                                               ▼
                                        ┌─────────────┐
                                        │ Service Bus │
                                        │   Queue     │
                                        └──────┬──────┘
                                               │
                                               ▼
                                        ┌─────────────┐
                                        │ Notification│
                                        │  Processor  │
                                        └─────────────┘
```

## Azure Services Used

| Service | Purpose |
|---------|---------|
| **Azure API Management** | API gateway, security, rate limiting |
| **Azure Cosmos DB** | NoSQL database for books and subscribers |
| **Azure Event Grid** | Event routing and pub/sub messaging |
| **Azure Service Bus** | Reliable message queuing |
| **Azure Functions** | Serverless compute for APIs and processors |
| **Azure Key Vault** | Secrets management |
| **Azure Monitor** | Observability and alerting |

## Technology Stack

- **Language:** C# / .NET 8 LTS
- **Runtime:** Azure Functions Isolated Worker
- **Infrastructure:** Bicep (Infrastructure as Code)
- **Testing:** xUnit, Moq

## Project Structure

```
├── infra/                    # Bicep templates
│   ├── modules/              # Reusable Bicep modules
│   └── parameters/           # Environment-specific parameters
├── src/
│   ├── Bookstore.Functions/  # Azure Functions (HTTP triggers, etc.)
│   ├── Bookstore.Core/       # Domain models, interfaces
│   ├── Bookstore.Infrastructure/  # Data access, external services
│   └── Bookstore.Contracts/  # DTOs, event schemas
├── tests/
│   ├── Bookstore.UnitTests/
│   └── Bookstore.IntegrationTests/
├── scripts/                  # Deployment and utility scripts
├── docs/                     # Architecture documentation
│   └── adr/                  # Architecture Decision Records
└── study/                    # Learning modules (portfolio)
```

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Azure CLI](https://docs.microsoft.com/cli/azure/install-azure-cli)
- [Azure Functions Core Tools v4](https://docs.microsoft.com/azure/azure-functions/functions-run-local)
- [Bicep CLI](https://docs.microsoft.com/azure/azure-resource-manager/bicep/install)
- An Azure subscription

### Local Development

```bash
# Clone the repository
git clone <repository-url>
cd Event-Driven-Bookstore-Notification

# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Run tests
dotnet test

# Run Azure Functions locally
cd src/Bookstore.Functions
func start
```

### Deployment

```bash
# Login to Azure
az login

# Deploy infrastructure (dev environment)
az deployment sub create \
  --location eastus \
  --template-file infra/main.bicep \
  --parameters infra/parameters/dev.parameters.json
```

## Architecture Patterns

- **Event-Driven Architecture** - Loose coupling via events
- **Pub/Sub Pattern** - Event Grid for event distribution
- **Competing Consumers** - Service Bus queue processing
- **API Gateway** - APIM for security and management
- **Change Data Capture** - Cosmos DB change feed

## Learning Modules

See the `study/` folder for detailed learning materials:

1. Architecture Foundation
2. Naming Convention & Resource Organization
3. Environment Parameterization
4. Security & Identity
5. Cosmos DB Data Layer
6. API Management
7. Event Grid Integration
8. Service Bus Messaging
9. Subscriber Management Service
10. Book Inventory Service
11. Notification Processor
12. Monitoring & Observability
13. Deployment Automation
14. Documentation & Portfolio

## References

- [Azure Well-Architected Framework](https://docs.microsoft.com/azure/architecture/framework/)
- [Cloud Adoption Framework](https://docs.microsoft.com/azure/cloud-adoption-framework/)
- [Azure Event-Driven Architecture](https://docs.microsoft.com/azure/architecture/guide/architecture-styles/event-driven)

## License

This project is for educational and portfolio purposes.
