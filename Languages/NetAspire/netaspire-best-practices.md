# .NET Aspire Best Practices

A comprehensive guide for building cloud-native applications with .NET Aspire. This document covers architectural patterns, tooling, deployment strategies, and best practices for creating distributed applications with .NET Aspire's cloud-native stack.

.NET Aspire is an opinionated, cloud-ready stack for building observable, production-ready, distributed applications. It provides a curated collection of components, tools, and templates designed to help developers build cloud-native applications with confidence.

## When to use .NET Aspire in projects

- **Microservices and distributed applications**: When building applications composed of multiple interconnected services
- **Cloud-native development**: Applications designed for cloud deployment with observability, resilience, and scalability in mind
- **Container-based architectures**: Projects using Docker containers and Kubernetes orchestration
- **Development teams requiring standardization**: When you need consistent patterns across distributed application components
- **Applications with complex dependencies**: Systems requiring databases, message queues, caches, and external APIs
- **Production-ready distributed systems**: Applications requiring built-in health checks, telemetry, and service discovery

## Tooling & ecosystem

### Core Tools
- **.NET 8+ SDK**: Minimum requirement for Aspire development
- **Docker Desktop**: For containerization and local development
- **Visual Studio 2022 17.9+** or **Visual Studio Code**: IDEs with Aspire tooling support
- **Aspire Workload**: Install via `dotnet workload install aspire`

### Package Managers
- **NuGet**: Primary package manager for .NET Aspire components
- **Container registries**: Docker Hub, Azure Container Registry, or similar for container images

### Orchestration
- **Aspire App Host**: Built-in orchestrator for local development
- **Kubernetes**: Production orchestration platform
- **Azure Container Apps**: Managed container service optimized for Aspire

## Recommended formatting & linters with config snippets

Use standard .NET formatting and analysis tools:

```xml
<!-- In your .csproj files -->
<PropertyGroup>
  <AnalysisMode>AllEnabledByDefault</AnalysisMode>
  <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
  <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
</PropertyGroup>
```

**.editorconfig** for consistent styling:
```ini
[*.cs]
dotnet_diagnostic.CA1063.severity = warning
dotnet_diagnostic.CA2007.severity = warning
dotnet_diagnostic.CA1031.severity = warning
```

**Format command**: `dotnet format`

## Testing & CI recommendations with example commands

### Unit Testing
```bash
# Run unit tests for individual services
dotnet test --logger "trx;LogFileName=test-results.trx"

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults
```

### Integration Testing
```bash
# Test service-to-service communication
dotnet test --filter "Category=Integration"

# Test with test containers
dotnet test --filter "Category=ContainerTest"
```

### CI Pipeline Example
```yaml
# GitHub Actions snippet
- name: Test Aspire Application
  run: |
    dotnet restore
    dotnet build --no-restore
    dotnet test --no-build --verbosity normal
    dotnet test --collect:"XPlat Code Coverage"
```

## Packaging & release guidance

### Container Strategy
- Use minimal base images (e.g., `mcr.microsoft.com/dotnet/aspnet:8.0-alpine`)
- Multi-stage Dockerfiles for optimized production images
- Version containers with semantic versioning

### Deployment Manifests
```yaml
# Example Kubernetes deployment
apiVersion: v1
kind: Service
metadata:
  name: my-aspire-service
spec:
  selector:
    app: my-aspire-service
  ports:
  - port: 8080
    targetPort: 8080
---
apiVersion: apps/v1
kind: Deployment
metadata:
  name: my-aspire-service
spec:
  replicas: 3
  selector:
    matchLabels:
      app: my-aspire-service
  template:
    metadata:
      labels:
        app: my-aspire-service
    spec:
      containers:
      - name: service
        image: myregistry/my-aspire-service:1.0.0
        ports:
        - containerPort: 8080
```

### Release Commands
```bash
# Build and publish container
dotnet publish --os linux --arch x64 -c Release
docker build -t myapp:latest .
docker push myregistry/myapp:latest
```

## Security & secrets best practices

- **Use managed identities**: Leverage Azure Managed Identity or similar for cloud authentication
- **Secrets management**: Store secrets in Azure Key Vault, Kubernetes secrets, or similar secure stores
- **Configuration**: Use `IConfiguration` with hierarchical configuration sources
- **Transport security**: Always use HTTPS/TLS for service-to-service communication
- **Principle of least privilege**: Grant minimal permissions to services and users

### Example Secret Configuration
```csharp
// In Program.cs
builder.Configuration.AddAzureKeyVault(
    new Uri("https://myvault.vault.azure.net/"),
    new DefaultAzureCredential());

// Access secrets
var connectionString = builder.Configuration["DatabaseConnectionString"];
```

## Recommended libraries

### Essential Aspire Components
- **Aspire.Hosting**: For application host and orchestration
- **Aspire.Hosting.Redis**: Redis integration for caching and pub/sub
- **Aspire.EntityFrameworkCore.SqlServer**: Entity Framework integration
- **Aspire.Hosting.PostgreSQL**: PostgreSQL database integration

### Observability
- **OpenTelemetry**: Built-in telemetry and distributed tracing
- **Aspire Dashboard**: Local development dashboard for monitoring

### Communication
- **HTTP clients**: Use `HttpClientFactory` with Aspire service discovery
- **gRPC**: For high-performance service-to-service communication

## Minimal example

### App Host Project (MyApp.AppHost)
```csharp
// Program.cs
var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");
var apiService = builder.AddProject<Projects.MyApp_ApiService>("apiservice");

builder.AddProject<Projects.MyApp_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(cache)
    .WithReference(apiService);

builder.Build().Run();
```

### API Service (MyApp.ApiService)
```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.MapDefaultEndpoints();

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
});

app.Run();
```

### CI Pipeline Snippet
```yaml
name: Build and Test
on: [push, pull_request]
jobs:
  test:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v4
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '8.0.x'
    - name: Install Aspire workload
      run: dotnet workload install aspire
    - name: Restore dependencies
      run: dotnet restore
    - name: Build
      run: dotnet build --no-restore
    - name: Test
      run: dotnet test --no-build --verbosity normal
```

## Resources

- Official .NET Aspire documentation: https://learn.microsoft.com/en-us/dotnet/aspire/
- .NET Aspire FAQ: https://learn.microsoft.com/en-us/dotnet/aspire/reference/aspire-faq
- .NET Aspire blog posts: https://devblogs.microsoft.com/dotnet/category/dotnet-aspire/
- .NET Aspire GitHub repository: https://github.com/dotnet/aspire
- Aspire samples and templates: https://github.com/dotnet/aspire-samples
- OpenTelemetry .NET documentation: https://opentelemetry.io/docs/languages/net/
- Container security best practices: https://learn.microsoft.com/en-us/azure/container-instances/container-instances-image-security
- Azure Container Apps with Aspire: https://learn.microsoft.com/en-us/azure/container-apps/aspire-overview

---

This document provides foundational guidance for .NET Aspire development. Combine these practices with your specific cloud platform guidance and organizational standards for complete application architecture.