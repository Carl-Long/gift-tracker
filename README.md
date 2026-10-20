# Gift Tracker API

Gift Tracker is a personal learning project focused on building a .NET 10 Web API, exploring API design, and testing endpoints using tools such as Postman, cURL, and HTTP request files.

## Tech Stack

- **.NET 10** and ASP.NET Core Web API
- **SQL Server 2022**, hosted locally using Docker
- **Entity Framework Core** (planned)
- **FluentValidation** (planned)

The solution follows a Clean Architecture-style separation of responsibilities.

Database integration, validation, and application-specific endpoints will be introduced as the project develops.

## Repository Structure

```text
GiftTracker/
├── docker/
│   ├── docker-compose.yml
│   └── .env.example
├── docs/
│   └── local-database-setup.md
├── src/
│   ├── GiftTracker.Api/
│   ├── GiftTracker.Application/
│   ├── GiftTracker.Domain/
│   └── GiftTracker.Infrastructure/
├── GiftTracker.slnx
├── .gitignore
└── README.md
```

A local `docker/.env` file is required for Docker but is excluded from source control.

Unit tests will be added under `tests/` as the project develops.

## Getting Started

### Prerequisites

Ensure the following are installed:

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) or Docker Engine with Compose
- A suitable IDE, such as Visual Studio, Rider, or VS Code

### 1. Configure the Local Database

The project uses SQL Server 2022 running in a Docker container.

Follow the [Local Database Setup Guide](docs/local-database-setup.md) to configure the environment and start SQL Server.

### 2. Restore and Build

From the repository root, restore dependencies:

```bash
dotnet restore GiftTracker.slnx
```

Build the solution:

```bash
dotnet build GiftTracker.slnx
```

### 3. Run the API

Start the API using the HTTPS launch profile:

```bash
dotnet run --project src/GiftTracker.Api --launch-profile https
```

The API is configured to listen on:

| Protocol | URL |
| --- | --- |
| HTTPS | `https://localhost:7000` |
| HTTP | `http://localhost:5000` |

HTTPS is the recommended endpoint for local development and API testing.

The application includes HTTPS redirection middleware.

If required, trust the local .NET development certificate:

```bash
dotnet dev-certs https --trust
```

### 4. API Documentation

The development environment exposes the generated OpenAPI specification at:

```text
https://localhost:7000/openapi/v1.json
```

Application-specific endpoints will be added as development progresses.

## Documentation

- [Local Database Setup](docs/local-database-setup.md) — SQL Server Docker configuration, environment variables, database connections, common commands, and troubleshooting.

## Development Notes

- Do not commit passwords, connection strings containing credentials, or other secrets.
- Local environment files such as `docker/.env` must remain excluded from source control.
- When database integration is introduced, .NET User Secrets will be used for local API credentials.
- The SQL Server container runs independently of the API during development.