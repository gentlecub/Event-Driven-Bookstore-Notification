# Event-Driven Bookstore Notification System

A cloud-native, event-driven architecture built on Microsoft Azure for managing bookstore inventory and notifying subscribers in real-time when new books are added.

## Architecture

```
┌──────────┐     ┌──────────┐     ┌──────────────────────────────────┐
│  Client  │────▶│   APIM   │────▶│         Azure Functions          │
│          │     │ Gateway  │     │  ┌─────────┐    ┌─────────────┐  │
└──────────┘     └──────────┘     │  │Book API │    │Subscriber   │  │
                                  │  │         │    │API          │  │
                                  │  └────┬────┘    └──────┬──────┘  │
                                  └───────┼────────────────┼─────────┘
                                          │                │
                         ┌────────────────┘                └───────────┐
                         ▼                                             ▼
                  ┌────────────┐                               ┌────────────┐
                  │ Event Grid │                               │ Cosmos DB  │
                  │   Topic    │                               │  (NoSQL)   │
                  └─────┬──────┘                               └────────────┘
                        ▼                                             │
                  ┌────────────┐     ┌─────────────────────┐          │
                  │Service Bus │────▶│Notification         │──────────┘
                  │   Queue    │     │Processor (Function) │
                  └────────────┘     └─────────────────────┘
```

## Azure Services

| Service | Purpose |
|---------|---------|
| **Azure Functions** | Serverless APIs and processors (.NET 8 Isolated) |
| **Azure Cosmos DB** | NoSQL database for books and subscribers |
| **Azure Event Grid** | Event routing with CloudEvents schema |
| **Azure Service Bus** | Reliable message queuing with dead-letter |
| **Azure API Management** | API gateway, security, rate limiting |
| **Azure Key Vault** | Secrets management |
| **Application Insights** | Monitoring and observability |

## Technology Stack

- **Language:** C# 12 / .NET 8 LTS
- **Runtime:** Azure Functions Isolated Worker Model
- **Infrastructure:** Bicep (Infrastructure as Code)
- **CI/CD:** GitHub Actions
- **Testing:** xUnit, Moq

## Project Structure

```
├── .github/workflows/          # CI/CD pipelines
├── docs/
│   ├── architecture/           # System architecture & ADRs
│   ├── api/                    # API documentation
│   └── monitoring/             # KQL queries
├── infra/
│   ├── main.bicep              # Main orchestration
│   ├── modules/                # Bicep modules
│   └── parameters/             # Environment configs
├── scripts/                    # Deployment scripts
├── src/
│   ├── Bookstore.Core/         # Domain layer
│   ├── Bookstore.Infrastructure/  # Data access & services
│   ├── Bookstore.Functions.BookApi/
│   ├── Bookstore.Functions.SubscriberApi/
│   └── Bookstore.Functions.NotificationProcessor/
├── tests/                      # Unit, Integration, Smoke tests
└── study/                      # Learning modules (1-14)
```

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Azure CLI](https://docs.microsoft.com/cli/azure/install-azure-cli)
- [Azure Functions Core Tools v4](https://docs.microsoft.com/azure/azure-functions/functions-run-local)
- [Bicep CLI](https://docs.microsoft.com/azure/azure-resource-manager/bicep/install)

### Build and Test

```bash
dotnet restore
dotnet build
dotnet test
```

### Local Development

```bash
cd src/Bookstore.Functions.BookApi
func start --port 7071
```

### Deploy Infrastructure

```bash
az login
az deployment sub create \
  --location eastus \
  --template-file infra/main.bicep \
  --parameters infra/parameters/dev.parameters.json
```

## API Endpoints

### Book API

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/books` | List all books |
| GET | `/api/books/{id}` | Get book by ID |
| POST | `/api/books` | Create book (triggers notification) |
| PUT | `/api/books/{id}` | Update book |
| DELETE | `/api/books/{id}` | Delete book |

### Subscriber API

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/subscribers` | List all subscribers |
| POST | `/api/subscribers` | Create subscriber |
| PUT | `/api/subscribers/{id}` | Update preferences |
| DELETE | `/api/subscribers/{id}` | Unsubscribe |

## Documentation

- [System Architecture](docs/architecture/ARCHITECTURE.md)
- [API Documentation](docs/api/README.md)
- [ADR-001: Event-Driven Architecture](docs/architecture/adr/ADR-001-event-driven-architecture.md)
- [ADR-002: Cosmos DB Partitioning](docs/architecture/adr/ADR-002-cosmos-db-partitioning.md)
- [ADR-003: Managed Identity](docs/architecture/adr/ADR-003-managed-identity.md)

# Documentation |

## References

- [Azure Well-Architected Framework](https://docs.microsoft.com/azure/architecture/framework/)
- [Cloud Adoption Framework](https://docs.microsoft.com/azure/cloud-adoption-framework/)
- [Event-Driven Architecture](https://docs.microsoft.com/azure/architecture/guide/architecture-styles/event-driven)

## License

MIT License - Educational and portfolio purposes.
