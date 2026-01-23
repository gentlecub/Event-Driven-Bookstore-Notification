# Module 01 — Architecture Foundation

## Overview

This module establishes the foundational project structure for the Event-Driven Bookstore Notification System. It follows enterprise-grade practices for cloud-native .NET development on Azure.

## Learning Objectives

After completing this module, you will understand:

- How to structure a cloud-native .NET solution
- The purpose of each configuration file
- Clean Architecture principles applied to Azure projects
- Version control best practices for Azure development

---

## Architecture Applied

### Clean Architecture Pattern

```
┌─────────────────────────────────────────────────────────────────┐
│                         Presentation                             │
│                    (Azure Functions HTTP)                        │
├─────────────────────────────────────────────────────────────────┤
│                         Application                              │
│                   (Use Cases, Services)                          │
├─────────────────────────────────────────────────────────────────┤
│                           Domain                                 │
│              (Entities, Interfaces, Events)                      │
├─────────────────────────────────────────────────────────────────┤
│                       Infrastructure                             │
│            (Cosmos DB, Service Bus, External)                    │
└─────────────────────────────────────────────────────────────────┘
```

### Azure Well-Architected Framework Pillars Applied

| Pillar | Application |
|--------|-------------|
| **Operational Excellence** | Infrastructure as Code, consistent structure |
| **Security** | Secrets excluded from version control |
| **Reliability** | Reproducible builds via version pinning |
| **Cost Optimization** | Efficient project organization |

---

## Files Created

### 1. Folder Structure

```
Event-Driven-Bookstore-Notification/
├── infra/
│   ├── modules/        # Reusable Bicep modules
│   └── parameters/     # Environment configs (dev, test, prod)
├── src/                # C# source code
├── tests/
│   ├── Bookstore.UnitTests/
│   └── Bookstore.IntegrationTests/
├── scripts/            # Deployment scripts
├── docs/
│   └── adr/            # Architecture Decision Records
└── study/              # Learning modules
```

**Why this structure?**
- Separates concerns (infra vs app vs tests)
- Supports multiple environments
- Enables CI/CD automation
- Follows Microsoft's recommended patterns

---

### 2. .gitignore

**Purpose:** Prevents sensitive and unnecessary files from being committed.

**Key Sections:**

| Section | What it excludes | Why |
|---------|------------------|-----|
| Build artifacts | `bin/`, `obj/` | Generated, not source |
| IDE files | `.vs/`, `.idea/` | Developer-specific |
| Azure Functions | `local.settings.json` | Contains local secrets |
| Secrets | `*.pfx`, `*.env`, `secrets.json` | Security critical |
| Test results | `TestResults/`, `coverage/` | Generated output |

**Security Best Practice:**
```
# NEVER commit these
local.settings.json      # Contains connection strings
appsettings.Development.json  # May contain secrets
*.pfx, *.pem            # Certificates
```

---

### 3. global.json

**Purpose:** Pins the .NET SDK version across all environments.

```json
{
  "sdk": {
    "version": "8.0.100",
    "rollForward": "latestFeature",
    "allowPrerelease": false
  }
}
```

**Configuration Explained:**

| Property | Value | Meaning |
|----------|-------|---------|
| `version` | `8.0.100` | Base SDK version |
| `rollForward` | `latestFeature` | Allow patch updates within major.minor |
| `allowPrerelease` | `false` | Production stability |

**Why pin SDK version?**
- Reproducible builds
- Consistent behavior in CI/CD
- Avoids "works on my machine" issues

---

### 4. Directory.Build.props

**Purpose:** Shared MSBuild properties for all projects.

**Key Settings:**

```xml
<TargetFramework>net8.0</TargetFramework>
<Nullable>enable</Nullable>
<ImplicitUsings>enable</ImplicitUsings>
```

| Setting | Effect |
|---------|--------|
| `TargetFramework` | All projects target .NET 8 |
| `Nullable` | Nullable reference types enabled (safer code) |
| `ImplicitUsings` | Common namespaces auto-imported |
| `TreatWarningsAsErrors` (Release) | Quality gate for production |

**Why use Directory.Build.props?**
- Single source of truth for project settings
- No need to repeat in each `.csproj`
- Enforces consistency across solution

---

### 5. BookstoreNotification.sln

**Purpose:** Visual Studio solution file organizing all projects.

**Solution Folders:**
- `src/` — Application projects
- `tests/` — Test projects
- `infra/` — Infrastructure reference

**Projects to be added:**
```
src/Bookstore.Functions        # Azure Functions
src/Bookstore.Core             # Domain layer
src/Bookstore.Infrastructure   # Data access
src/Bookstore.Contracts        # DTOs, Events
tests/Bookstore.UnitTests      # Unit tests
tests/Bookstore.IntegrationTests  # Integration tests
```

---

### 6. README.md

**Purpose:** Project documentation and portfolio presentation.

**Sections included:**
- Architecture diagram (ASCII)
- Azure services overview
- Project structure
- Getting started guide
- Architecture patterns
- Learning module index

---

## CLI Commands Used

Since the `dotnet` CLI was not available in the environment, the solution file was created manually. In a normal setup:

```bash
# Create solution
dotnet new sln --name BookstoreNotification

# Add projects (future modules)
dotnet sln add src/Bookstore.Core/Bookstore.Core.csproj
dotnet sln add src/Bookstore.Functions/Bookstore.Functions.csproj

# Verify SDK version
dotnet --version

# Build solution
dotnet build
```

---

## Common Errors

| Error | Cause | Solution |
|-------|-------|----------|
| `dotnet: command not found` | .NET SDK not installed | Install .NET 8 SDK |
| `MSB4019: Microsoft.NET.Sdk not found` | Wrong SDK version | Check `global.json` |
| `CS8600: Converting null literal` | Nullable enabled | Add null checks or `?` |
| Solution won't load | Malformed `.sln` | Regenerate with `dotnet new sln` |

---

## Validation Steps

### Verify Folder Structure
```bash
find . -type d | head -20
```

### Verify .gitignore Works
```bash
# Create test file that should be ignored
touch local.settings.json
git status  # Should not show local.settings.json
```

### Verify Solution Loads
Open `BookstoreNotification.sln` in Visual Studio or Rider.

---

## Learning Summary

### Key Takeaways

1. **Structure matters** — A well-organized project is easier to maintain and scale
2. **Pin your versions** — `global.json` prevents environment drift
3. **Centralize configuration** — `Directory.Build.props` ensures consistency
4. **Security first** — `.gitignore` protects secrets from day one
5. **Document early** — README serves as both guide and portfolio

### Azure Well-Architected Alignment

| Pillar | What we did |
|--------|-------------|
| Operational Excellence | IaC structure, reproducible setup |
| Security | Secrets excluded, safe defaults |
| Reliability | Version pinning, consistent builds |
| Performance | Optimized Release configuration |
| Cost Optimization | Efficient structure, no waste |

### Enterprise Patterns Applied

- **Infrastructure as Code** — `infra/` folder for Bicep
- **Clean Architecture** — Separated concerns in `src/`
- **Test Pyramid** — Unit and Integration test folders
- **Documentation as Code** — README in repository

---

## Next Module

**Module 02 — Naming Convention & Resource Organization**

Will define:
- Azure resource naming patterns
- Resource group strategy
- Tagging standards
- Environment abbreviations

---

## References

- [.NET SDK global.json](https://docs.microsoft.com/dotnet/core/tools/global-json)
- [Directory.Build.props](https://docs.microsoft.com/visualstudio/msbuild/customize-your-build)
- [Azure Well-Architected Framework](https://docs.microsoft.com/azure/architecture/framework/)
- [Clean Architecture (.NET)](https://docs.microsoft.com/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures)
