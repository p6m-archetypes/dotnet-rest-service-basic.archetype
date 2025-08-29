# .NET REST Service Archetype

![Latest Release](https://img.shields.io/github/v/release/p6m-archetypes/dotnet-rest-service-basic.archetype?style=flat-square&label=Latest%20Release&color=blue)

## Usage

To get started, [install archetect](https://github.com/p6m-archetypes/development-handbook)
and render this template to your current working directory:

```bash
archetect render git@github.com:p6m-archetypes/dotnet-rest-service-basic.archetype.git
```

For information about interacting with the service, refer to the README at the generated
project's root.

## Prompts

When rendering the archetype, you'll be prompted for the following values:

| Property          | Description                                                                                                         | Example               |
| ----------------- | ------------------------------------------------------------------------------------------------------------------- | --------------------- |
| `project`         | General name that represents the service domain that is used to set the entity, service, and RPC stub names.        | Shopping Cart         |
| `suffix`          | Used in conjunction with `project` to set package names.                                                            | Service               |
| `group-prefix`    | Used in conjunction with `project` to set package names.                                                            | {{ group-id }}        |
| `team-name`       | Identifies the team that owns the generated project. Used to label published artifacts and in the generated README. | Growth                |
| `service-port`    | Sets the port used for gRPC traffic                                                                                 | {{ service-port }}    |
| `management-port` | Sets the port used to monitor the application over HTTP                                                             | {{ management-port }} |

For a list of all derived properties and examples of the property relationships, see [archetype.yml](./archetype.yml).

## What's Inside

Features include:

- Entity Framework Core [EF](https://learn.microsoft.com/en-us/ef/core/)
- Testcontainers .NET (https://dotnet.testcontainers.org/)
- Simple CRUD over REST
- Docker image publication to artifactory
- Load tests using [k6](https://k6.io/) for both HTTP and gRPC calls
- Application configuration through property files, environment variables, and CLI arguments.
- Integration with [Tilt](https://tilt.dev/) to support local k8s development

## Testing & Validation

### Archetype Validation

To validate the archetype generates correctly and all features work:

```bash
./validate_archetype.sh
```

This will generate a test service and run comprehensive validation including:
- Template substitution validation
- .NET build and test execution
- Docker containerization
- Service connectivity and health checks
- REST endpoint functionality
- Monitoring infrastructure

### Manual Endpoint Testing

For manual testing of generated services, you can use standard HTTP tools:

```bash
# Test health endpoints
curl http://localhost:8081/health
curl http://localhost:8081/health/live
curl http://localhost:8081/health/ready

# Test auth endpoint
curl -X POST http://localhost:8080/api/auth/token \
  -H "Content-Type: application/json" \
  -d '{"clientId":"admin-client","clientSecret":"admin-secret"}'

# Test API endpoints (requires auth token)
curl http://localhost:8080/api/[service-name]
```
