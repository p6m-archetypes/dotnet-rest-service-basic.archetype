# .NET REST Service Archetype

![Latest Release](https://img.shields.io/github/v/release/p6m-archetypes/dotnet-rest-service-basic.archetype?style=flat-square&label=Latest%20Release&color=blue)

Production-ready archetype for generating modular .NET REST services with Entity Framework Core, OpenAPI documentation, flexible persistence options, and modern observability.

## 🎯 What This Generates

This archetype creates a complete, production-ready REST service with:

- **🏗️ Modular Architecture**: Namespace-organized, service-oriented design with separate API, Core, and Persistence layers
- **⚡ Modern .NET Stack**: .NET 8+ with Entity Framework Core and ASP.NET Core
- **🔌 REST API**: HTTP-based REST APIs with OpenAPI/Swagger documentation
- **💾 Flexible Persistence**: Choose from PostgreSQL, MySQL, MSSQL, or no database
- **🐳 Container-Ready**: Docker and Kubernetes deployment manifests
- **📊 Built-in Monitoring**: Health checks, metrics, and observability endpoints
- **🧪 Comprehensive Testing**: Unit and integration tests with Testcontainers
- **⚡ Load Testing**: k6 performance tests for HTTP endpoints
- **🔧 Local Development**: Tilt integration for Kubernetes development

## 📦 Generated Project Structure

```
my-shopping-cart-service/
├── src/
│   ├── ShoppingCart.Api/          # REST API controllers and DTOs
│   ├── ShoppingCart.Core/         # Business logic and domain models
│   ├── ShoppingCart.Persistence/  # Entity Framework data layer
│   └── ShoppingCart.Server/       # ASP.NET Core server implementation
├── tests/
│   ├── ShoppingCart.UnitTests/
│   └── ShoppingCart.IntegrationTests/
├── k6/                            # Load testing scripts
├── k8s/                            # Kubernetes manifests
├── Dockerfile
└── docker-compose.yml
```

## 🚀 Quick Start

### Prerequisites

- [Archetect](https://archetect.github.io/) CLI tool
- .NET 8 SDK or later
- Docker Desktop (for containerized development and testing)

### Generate a New Service

```bash
# Using SSH
archetect render git@github.com:p6m-archetypes/dotnet-rest-service-basic.archetype.git

# Using HTTPS
archetect render https://github.com/p6m-archetypes/dotnet-rest-service-basic.archetype.git

# Example prompt answers:
# project: Shopping Cart
# suffix: Service
# group-prefix: com.example
# team-name: Platform
# persistence: PostgreSQL
# service-port: 5030
```

### Development Workflow

```bash
cd shopping-cart-service

# 1. Restore dependencies
dotnet restore

# 2. Run tests
dotnet test

# 3. Start the service
dotnet run --project src/ShoppingCart.Server

# 4. Access endpoints
# - REST API: http://localhost:5030/api/shoppingcart
# - Swagger UI: http://localhost:5030/swagger
# - Health Check: http://localhost:5031/health
# - Health Live: http://localhost:5031/health/live
# - Health Ready: http://localhost:5031/health/ready
# - Metrics: http://localhost:5031/metrics
```

## 📋 Configuration Prompts

When rendering the archetype, you'll be prompted for the following values:

| Property | Description | Example | Required |
|----------|-------------|---------|----------|
| `project` | Service domain name used for namespaces and entities | Shopping Cart | Yes |
| `suffix` | Appended to project name for package naming | Service | Yes |
| `group-prefix` | Namespace prefix (reverse domain notation) | com.example | Yes |
| `team-name` | Owning team identifier for artifacts and documentation | Platform | Yes |
| `persistence` | Database type for data persistence | PostgreSQL | Yes |
| `service-port` | Port for REST HTTP traffic | 5030 | Yes |

**Derived Properties:**
- `management-port`: Automatically set to `service-port + 1` for health/metrics endpoints
- `database-port`: Set to 5432 for PostgreSQL-based services
- `debug-port`: Set to `service-port + 9` for debugging

For complete property relationships, see [archetype.yaml](./archetype.yaml).

## ✨ Key Features

### 🏛️ Architecture & Design

- **Modular Structure**: Clean separation of API, Core, Persistence, and Server concerns
- **Domain-Driven Design**: Entity-centric business logic organization
- **Dependency Injection**: Built-in ASP.NET Core DI container configuration
- **Clean Architecture**: Dependencies flow toward domain core
- **RESTful Principles**: Resource-based routing with standard HTTP verbs

### 🔧 Technology Stack

- **.NET 8+**: Latest LTS framework with performance improvements
- **ASP.NET Core**: Modern web framework for REST APIs
- **Entity Framework Core**: Modern ORM with migration support and async operations
- **OpenAPI/Swagger**: Automatic API documentation and interactive testing UI
- **Testcontainers**: Containerized integration testing with real databases
- **k6**: High-performance load testing for HTTP endpoints
- **Tilt**: Local Kubernetes development workflow with hot reload

### 📊 Observability & Monitoring

- **Health Checks**: Liveness and readiness endpoints for Kubernetes probes
- **Metrics**: Prometheus-compatible metrics endpoint
- **Structured Logging**: Configurable log levels with structured output
- **Request Tracing**: Distributed tracing support (OpenTelemetry-ready)
- **Performance Monitoring**: Built-in request/response timing and logging

### 🧪 Testing & Quality

- **Unit Tests**: xUnit test projects for business logic validation
- **Integration Tests**: Full service testing with Testcontainers and real databases
- **Load Tests**: k6 scripts for HTTP performance and stress testing
- **API Testing**: HTTP endpoint testing with realistic scenarios
- **Test Coverage**: Configured coverage reporting

### 🚢 DevOps & Deployment

- **Docker**: Multi-stage Dockerfile for optimized production images
- **Kubernetes**: Complete deployment manifests with ConfigMaps and Secrets
- **Tilt**: Hot-reload development in local Kubernetes clusters
- **Artifactory**: Docker image publication configuration included
- **Health Probes**: Kubernetes liveness and readiness probe configuration

## 🎯 Use Cases

This archetype is ideal for:

1. **Microservices**: Building HTTP-based microservices with standard REST patterns
2. **CRUD Applications**: Services requiring Create, Read, Update, Delete operations over HTTP
3. **Public APIs**: External-facing APIs with OpenAPI documentation for easy integration
4. **Internal Services**: Backend services for web and mobile applications

## 📚 What's Inside

### Core Components

#### REST API Controllers
ASP.NET Core controllers with attribute routing, model validation, and automatic OpenAPI documentation generation. Includes CRUD operations with proper HTTP status codes.

#### Entity Framework Persistence
Database access layer with migrations, connection pooling, and async operations. Supports PostgreSQL, MySQL, MSSQL, or no persistence for stateless services.

#### OpenAPI/Swagger Documentation
Automatically generated API documentation with interactive Swagger UI for testing endpoints directly in the browser.

#### Health & Metrics
Built-in health check endpoints for Kubernetes liveness/readiness probes and Prometheus metrics for monitoring API performance and resource usage.

### Development Tools

- **Tilt Configuration**: Auto-reload development in Kubernetes with live updates
- **Docker Compose**: Local development stack with database services
- **k6 Load Tests**: Performance testing scripts for HTTP endpoints
- **Swagger UI**: Interactive API documentation and testing interface

### Configuration Management

- **appsettings.json**: Environment-specific configuration files
- **Environment Variables**: 12-factor app configuration support
- **CLI Arguments**: Runtime configuration overrides
- **Connection Strings**: Secure database connection management

## 🔧 REST-Specific Features

### API Design

- **Resource-Based Routing**: RESTful endpoint structure (/api/resource)
- **HTTP Verbs**: Proper use of GET, POST, PUT, PATCH, DELETE
- **Status Codes**: Semantic HTTP status codes (200, 201, 400, 404, 500, etc.)
- **Content Negotiation**: JSON request/response with proper Content-Type headers
- **API Versioning**: Support for versioning strategies (URL, header, or query parameter)

### OpenAPI/Swagger

- **Automatic Documentation**: API documentation generated from code and attributes
- **Interactive UI**: Swagger UI for testing endpoints without external tools
- **Schema Definition**: Request/response models with validation rules
- **Try It Out**: Execute API calls directly from the documentation

### HTTP Features

- **CORS Support**: Configurable Cross-Origin Resource Sharing policies
- **Compression**: Response compression for reduced bandwidth
- **Rate Limiting**: Built-in support for rate limiting (configurable)
- **Request Validation**: Model validation with automatic 400 Bad Request responses
- **Exception Handling**: Global exception middleware with consistent error responses

### Performance

- **Async/Await**: Fully asynchronous controllers and data access
- **Response Caching**: Configurable HTTP response caching
- **Connection Pooling**: Database connection pooling for optimal performance
- **Static File Serving**: Efficient static file serving with caching headers

## 📋 Validation & Quality Assurance

Run the validation suite to ensure the archetype generates correctly:

```bash
./validate_archetype.sh
```

This validates:
- ✅ Successful .NET build and compilation
- ✅ All unit and integration tests pass
- ✅ Docker image builds successfully
- ✅ Service starts and responds to health checks
- ✅ REST endpoints are accessible and return correct responses
- ✅ Database migrations execute successfully
- ✅ OpenAPI documentation is generated correctly

### Manual Testing

Test generated service endpoints:

```bash
# Test health endpoints
curl http://localhost:5031/health
curl http://localhost:5031/health/live
curl http://localhost:5031/health/ready

# Test REST API endpoints
curl http://localhost:5030/api/shoppingcart
curl -X POST http://localhost:5030/api/shoppingcart \
  -H "Content-Type: application/json" \
  -d '{"name":"Test Item","quantity":1}'

# View API documentation
open http://localhost:5030/swagger
```

### Load Testing

Run k6 performance tests:

```bash
k6 run k6/load-test.js
```

## 🔗 Related Archetypes

- **[.NET gRPC Service](../dotnet-grpc-service-basic.archetype)** - For high-performance RPC communication
- **[.NET GraphQL Service](../dotnet-graphql-service-basic.archetype)** - For flexible GraphQL APIs
- **[Python REST Service](../python-rest-service-uv-basic.archetype)** - Python alternative with FastAPI
- **[Java Spring Boot gRPC](../java-spring-boot-grpc-service.archetype)** - Java-based service alternative

## 🤝 Contributing

This archetype is actively maintained. For issues or enhancements:

1. Check existing issues in the repository
2. Create detailed bug reports or feature requests
3. Follow the contribution guidelines
4. Test changes with the validation suite

## 📄 License

This archetype is released under the MIT License. Generated services inherit this license but can be changed as needed for your organization.

---

**Ready to build production-grade REST services with .NET?** Generate your first service and start building in minutes! 🚀
