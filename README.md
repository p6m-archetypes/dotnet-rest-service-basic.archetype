# .NET gRPC Service Archetype

## Usage

To get started, [install archetect](https://github.com/p6m-archetypes/development-handbook)
and render this template to your current working directory:

```bash
archetect render git@github.com:p6m-archetypes/dotnet-grpc-service.archetype.git
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
- Simple CRUD over [gRPC](https://grpc.io/)
- gRPC stub publication based on the project name
- Docker image publication to artifactory
- Load tests using [k6](https://k6.io/) for both HTTP and gRPC calls
- Application configuration through property files, environment variables, and CLI arguments.
- Integration with [Tilt](https://tilt.dev/) to support local k8s development
