# {{ PrefixName }} {{ SuffixName }}

**// TODO:** Add description of your project's business function.

Generated from the [.NET gRPC Service Archetype](https://github.com/p6m-archetypes/{{ prefix-name }}-{{ suffix-name }}.archetype).

## Table of Contents

- [Prereqs](#prereqs)
  - [1. .NET SDK](#1-net-sdk)
  - [2. NuGet Package Management](#2-nuget-package-management)
  - [3. Docker Installed and Running](#3-docker-installed-and-running)
- [Overview](#overview)
  - [Project Structure / Modules](#project-structure--modules)
  - [Build System](#build-system)
- [Build](#build)
- [Run Server](#run-server)
  - [Using your service's APIs](#using-your-services-apis)
- [Ephemeral Mode (Recommended for Development)](#ephemeral-mode-recommended-for-development)
  - [Running with Ephemeral Database](#running-with-ephemeral-database)
  - [Ephemeral Mode Configuration](#ephemeral-mode-configuration)
  - [Integration Tests](#integration-tests)
- [Management API](#management-api)
  - [Health Checks](#health-checks)
  - [Metrics](#metrics)
- [DB migrations](#db-migrations)
- [Contributions](#contributions)

## Prereqs

### 1. .NET SDK

- **Version:** 9.0 or higher
- **Verify:**
  ```bash
  dotnet --version # → 9.x.x or greater
  ```
- See https://developer.p6m.dev/docs/workstation/dotnet for instructions

### 2. NuGet Package Management

- **Verify** you've configured **Artifactory**
  ```bash
  echo $ARTIFACTORY_USERNAME
  echo $ARTIFACTORY_IDENTITY_TOKEN
  ```
- See https://developer.p6m.dev/docs/workstation/core/artifacts for instructions

### 3. Docker Installed and Running

- **Verify** you have installed docker
  ```bash
  docker --version # Should be version X.X.+
  docker info # Should list server info without any errors
  ```
- See https://developer.p6m.dev/docs/workstation/core/docker for instructions

# Overview

## Project Structure / Modules

| Directory                                                                          | Description                                                                                |
| ---------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------ |
| [{{ PrefixName }}{{ SuffixName }}.API]({{ PrefixName }}{{ SuffixName }}.API/README.md)                           | Service Interfaces with a gRPC model. gRPC/Protobuf spec.                                  |
| [{{ PrefixName }}{{ SuffixName }}.Client]({{ PrefixName }}{{ SuffixName }}.Client/README.md)                     | gRPC Client. Implements the API.                                                           |
| [{{ PrefixName }}{{ SuffixName }}.Core]({{ PrefixName }}{{ SuffixName }}.Core/README.md)                         | Business Logic. Abstracts Persistence, defines Transaction Boundaries. Implements the API. |
| [{{ PrefixName }}{{ SuffixName }}.IntegrationTests]({{ PrefixName }}{{ SuffixName }}.IntegrationTests/README.md) | Leverages the Client to test the Server and it's dependencies.                             |
| [{{ PrefixName }}{{ SuffixName }}.Persistence]({{ PrefixName }}{{ SuffixName }}.Persistence/README.md)           | Persistence Entities and Data Repositories. Wrapped by Core.                               |
| [{{ PrefixName }}{{ SuffixName }}.Server]({{ PrefixName }}{{ SuffixName }}.Server/README.md)                     | Transport/Protocol Host. Wraps Core.                                                       |

## Build System

This project uses [dotnet](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet#general) as its build system. Common goals include

| Goal    | Description                                        |
| ------- | -------------------------------------------------- |
| clean   | Clean build outputs.                               |
| build   | Builds a .NET application.                         |
| restore | Restores the dependencies for a given application. |
| run     | Runs the application from source                   |
| test    | Runs tests using a test runner                     |

## Build

```bash
dotnet build
```

## Run Server

Start the server locally or using Docker. You can run the server from either the project root or from within the server directory:

**Option 1: From project root (specify project):**

```bash
dotnet run --project {{ PrefixName }}{{ SuffixName }}.Server -- --ephemeral
```

**Option 2: From server directory (simpler syntax):**

```bash
cd {{ PrefixName }}{{ SuffixName }}.Server
dotnet run -- --ephemeral
```

This server accepts connections on the following ports:

- 5030: used for application gRPC Service traffic.
- 5031: used to monitor the application over HTTP.
- 26257: exposes the persistent database port (when using docker-compose)

### Using your service's APIs

Create, Read, Update and Delete an entity using a gRPC client, like [grpcurl](https://github.com/fullstorydev/grpcurl) (CLI) or [grpcui](https://github.com/fullstorydev/grpcui) (GUI).

Create{{ PrefixName }}

```bash
grpcurl -plaintext -d '{"name": "test"}' localhost:5030 \
    {{ org_name }}.{{ solution_name }}.{{ prefix_name }}.{{ suffix_name }}.grpc.{{ PrefixName }}{{ SuffixName }}/Create{{ PrefixName }}
```

Get{{ PrefixName }}s

```bash
grpcurl -plaintext -d '{"start_page": "1", "page_size": "5"}' localhost:5030 \
    {{ org_name }}.{{ solution_name }}.{{ prefix_name }}.{{ suffix_name }}.grpc.{{ PrefixName }}{{ SuffixName }}/Get{{ PrefixName }}s
```

## Ephemeral Mode (Recommended for Development)

The service supports an **ephemeral mode** that automatically starts a PostgreSQL database in a Docker container using [Testcontainers](https://dotnet.testcontainers.org/). This provides a real PostgreSQL database for development and testing without requiring any manual setup.

### Running with Ephemeral Database

Start the server with the `--ephemeral` flag:

**Option 1: From project root:**

```bash
dotnet run --project {{ PrefixName }}{{ SuffixName }}.Server -- --ephemeral
```

**Option 2: From server directory:**

```bash
cd {{ PrefixName }}{{ SuffixName }}.Server
dotnet run -- --ephemeral
```

**What happens:**

- Automatically downloads and starts a PostgreSQL 15 Alpine container
- Creates a fresh database with a random port
- Creates database schema from current Entity Framework model (clean state)
- Starts the gRPC service
- Cleans up the container when the service stops

**Benefits:**

- ✅ **Zero setup** - No need to install PostgreSQL or run docker-compose
- ✅ **Real database** - Uses actual PostgreSQL, not in-memory simulation
- ✅ **Clean state** - Fresh database on every startup
- ✅ **Automatic cleanup** - Container is removed when service stops
- ✅ **Perfect for development** - Matches production database behavior
- ✅ **Database connection info** - Displays connection details for DataGrip/psql

**Requirements:**

- Docker must be installed and running
- Internet connection (first time only, to download PostgreSQL image)

**Database Connection Information:**
When running in ephemeral mode, the service will display database connection information in the console:

```
================================================================================
🐘 EPHEMERAL POSTGRESQL DATABASE CONNECTION INFO
================================================================================

📋 Connection Details:
   Host:     localhost
   Port:     54321
   Database: {{ prefix-name }}-{{ suffix-name }}-ephemeral
   Username: postgres
   Password: testpassword

🔗 Connection Strings:
   .NET Connection String:
   Host=localhost;Port=54321;Database={{ prefix-name }}-{{ suffix-name }}-ephemeral;Username=postgres;Password=testpassword

   JDBC URL (for DataGrip/IntelliJ):
   jdbc:postgresql://localhost:54321/{{ prefix-name }}-{{ suffix-name }}-ephemeral

💻 Connect via psql:
   psql -h localhost -p 54321 -U postgres -d {{ prefix-name }}-{{ suffix-name }}-ephemeral
   Password: testpassword

🔧 DataGrip/Database Tool Settings:
   Type:     PostgreSQL
   Host:     localhost
   Port:     54321
   Database: {{ prefix-name }}-{{ suffix-name }}-ephemeral
   User:     postgres
   Password: testpassword

ℹ️  Note: This is an ephemeral database that will be destroyed when the application stops.
================================================================================
```

This makes it easy to connect with database tools like DataGrip, pgAdmin, or command-line psql during development.

### Ephemeral Mode Configuration

The ephemeral database can be configured via `appsettings.Ephemeral.json`:

```json
{
  "Ephemeral": {
    "Database": {
      "Image": "postgres:15-alpine",
      "DatabaseName": "{{ prefix-name }}-{{ suffix-name }}-ephemeral",
      "Username": "postgres",
      "Password": "testpassword",
      "Reuse": false
    }
  }
}
```

**Configuration Options:**

- `Image`: PostgreSQL Docker image to use
- `DatabaseName`: Name of the database to create
- `Username`: Database username
- `Password`: Database password
- `Reuse`: Whether to reuse existing containers (false = always create fresh)

### Integration Tests

Integration tests automatically use ephemeral mode and get their own isolated PostgreSQL container:

```bash
dotnet test {{ PrefixName }}{{ SuffixName }}.IntegrationTests
```

Each test run gets:

- Fresh PostgreSQL container on a random port
- Clean database state
- Real database transactions and constraints
- Automatic cleanup after tests complete

### Running DB locally with persistent state

Run Database dependencies with `docker-compose`

```bash
docker-compose up -d
```

Shutdown local database

```bash
docker-compose down
```

## Management API

### Health Checks

Verify things are up and running by looking at the [/health](http://localhost:5031/health) endpoint:

```bash
curl localhost:5031/health
```

## Metrics

Prometheus - [Prometheus](https://github.com/prometheus-net/prometheus-net)

[/metrics](http://localhost:5031/metrics) endpoint:

```bash
curl localhost:5031/metrics
```

## DB migrations

### Create DB Migration

```bash
dotnet ef migrations add InitialCreation  --project {{ PrefixName }}{{ SuffixName }}.Persistence -s {{ PrefixName }}{{ SuffixName }}.Server
```

### Apply DB migrations

```bash
dotnet ef database update --project {{ PrefixName }}{{ SuffixName }}.Persistence -s {{ PrefixName }}{{ SuffixName }}.Server
```

### Remove DB migrations

```bash
dotnet ef migrations remove --project {{ PrefixName }}{{ SuffixName }}.Persistence -s {{ PrefixName }}{{ SuffixName }}.Server
```

## Contributions

**// TODO:** Add description of how you would like issues to be reported and people to reach out.
