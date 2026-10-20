# Local Database Setup

This guide explains how to configure and run Microsoft SQL Server 2022 locally using Docker for the Gift Tracker API.

SQL Server runs in a Docker container, while the .NET API runs directly on the host machine.

All commands in this guide should be executed from the **repository root** unless otherwise specified.

## Prerequisites

Before starting, ensure the following are installed:

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) or Docker Engine with Compose
- A SQL Server-compatible database client, such as VS Code with the MSSQL extension or SQL Server Management Studio on Windows

Ensure Docker is running before continuing.

> **Apple Silicon:** The Docker Compose configuration specifies `platform: linux/amd64`. ARM-based Macs require x86-64 emulation, and SQL Server container compatibility may vary. A supported x86-64 environment is recommended if compatibility issues occur.

## 1. Configure Environment Variables

The Docker Compose configuration reads the SQL Server administrator password from an environment variable named `SQL_PASSWORD`.

The repository contains an example configuration file:

```text
docker/.env.example
```

Create your local environment file by running:

```bash
cp docker/.env.example docker/.env
```

Open `docker/.env` and configure a strong password:

```dotenv
SQL_PASSWORD=YourStrongLocalPassword123!
```

SQL Server passwords must meet its complexity requirements, including:

- At least 8 characters
- Characters from at least three of the following groups: uppercase letters, lowercase letters, numbers, and symbols

If using special characters such as `$` or `#`, be aware that Docker Compose applies interpolation and quoting rules to `.env` files.

**Important:** Never commit `docker/.env` to source control.

The repository's `.gitignore` excludes this file. Only `.env.example`, without real credentials, should be committed.

## 2. Validate the Docker Configuration

Before starting SQL Server, validate the Docker Compose configuration:

```bash
docker compose -f docker/docker-compose.yml config --quiet
```

This checks the configuration and resolves environment variables without starting any containers.

If validation fails, confirm that:

- `docker/.env` exists
- `SQL_PASSWORD` has been configured
- The Docker Compose YAML is valid

## 3. Start SQL Server

From the repository root, run:

```bash
docker compose -f docker/docker-compose.yml up -d
```

Docker will:

1. Download the SQL Server 2022 image if required.
2. Create the SQL Server container.
3. Initialise SQL Server using the configured password.
4. Create the persistent database volume.
5. Start the container in the background.

The first startup may take longer while SQL Server initialises.

### Verify the Container

Check the container status:

```bash
docker compose -f docker/docker-compose.yml ps
```

View SQL Server startup logs:

```bash
docker compose -f docker/docker-compose.yml logs -f sql
```

Wait until SQL Server reports that it is ready for client connections.

Press `Ctrl+C` to stop following the logs. This does not stop the container.

## 4. Connect to SQL Server

Use your preferred SQL Server database client with the following connection details:

| Setting | Value |
| --- | --- |
| Server | `localhost` |
| Port | `14333` |
| SQL Server address | `localhost,14333` |
| Authentication | SQL Server Authentication |
| Username | `sa` |
| Password | Value configured in `docker/.env` |
| Initial database | `master` |
| Certificate | Trust the server certificate for local development if required |

### Port Configuration

The Docker Compose file contains:

```yaml
ports:
  - "14333:1433"
```

This maps:

- `14333` — SQL Server port exposed on the host machine
- `1433` — SQL Server port inside the Docker container

The non-default host port avoids conflicts with other local SQL Server installations.

**Note:** The application database named `GiftTracker` is not automatically created by Docker Compose.

It will be created separately once database integration and Entity Framework Core migrations are introduced.

## 5. Configure the .NET API Connection String

The API currently contains the project skeleton and does not yet connect to SQL Server.

When database integration is implemented, local connection strings should be stored using **.NET User Secrets** rather than committing passwords to configuration files.

### Initialise User Secrets

From the repository root:

```bash
dotnet user-secrets init --project src/GiftTracker.Api
```

This only needs to be done once per project.

### Configure the Connection String

Run:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,14333;Database=GiftTracker;User Id=sa;Password=YOUR_LOCAL_PASSWORD;TrustServerCertificate=True" --project src/GiftTracker.Api
```

Replace `YOUR_LOCAL_PASSWORD` with the password configured in `docker/.env`.

User Secrets stores the connection string outside the repository and makes it available to the application in the Development environment.

These commands only configure the connection string. They do not create the database or configure Entity Framework Core.

> **Security:** The `sa` account and `TrustServerCertificate=True` are intended for this isolated local development environment. Other environments should use appropriately scoped credentials and proper certificate validation.

## 6. Common Docker Commands

All commands should be run from the repository root.

### Start SQL Server

```bash
docker compose -f docker/docker-compose.yml up -d
```

### Stop SQL Server

```bash
docker compose -f docker/docker-compose.yml down
```

Stops and removes the container while preserving database data.

### Restart SQL Server

```bash
docker compose -f docker/docker-compose.yml restart sql
```

### Check Container Status

```bash
docker compose -f docker/docker-compose.yml ps
```

### View Container Logs

```bash
docker compose -f docker/docker-compose.yml logs -f sql
```

### Reset the Local Database

```bash
docker compose -f docker/docker-compose.yml down -v
```

**Warning:** This command removes the database volume and permanently deletes the locally stored database data.

Only use this when intentionally resetting the development database.

## 7. Data Persistence

SQL Server stores database files inside a named Docker volume:

```yaml
volumes:
  gift_tracker_sql_data:
    name: gift_tracker_sql_data
```

The volume is mounted inside the SQL Server container at:

```text
/var/opt/mssql
```

This ensures database data is retained even when the container is stopped or recreated.

| Command | Database data |
| --- | --- |
| `docker compose ... stop` | Preserved |
| `docker compose ... down` | Preserved |
| `docker compose ... up -d` | Existing data reused |
| `docker compose ... down -v` | Deleted |

Changing the SQL Server password in `.env` does not update the password of an already-initialised SQL Server database.

To change it, update the password within SQL Server or deliberately recreate the database volume if the data can be discarded.

## 8. Troubleshooting

### SQL Server Fails to Start

View container logs:

```bash
docker compose -f docker/docker-compose.yml logs sql
```

Check that:

- Docker is running.
- The SQL Server password meets the complexity requirements.
- Docker has sufficient memory and CPU resources.
- The configured host port is available.

The Compose configuration currently limits SQL Server to 2 GB RAM and 1 CPU.

These limits may need adjustment for larger workloads.

### Cannot Connect to SQL Server

Confirm that:

- The SQL Server container is running.
- SQL Server has completed its startup process.
- The server address is `localhost,14333`.
- SQL Server Authentication is selected.
- The password is correct.
- The client trusts the local development certificate if required.

### Port 14333 Is Already in Use

Update the host port in `docker/docker-compose.yml`:

```yaml
ports:
  - "14334:1433"
```

Recreate the container to apply the change:

```bash
docker compose -f docker/docker-compose.yml up -d
```

Update database connections to use `localhost,14334`.

### Password Changed but Login Fails

Changing `SQL_PASSWORD` in `.env` does not reset the password stored by an existing SQL Server database.

Change the password using SQL Server tooling.

Alternatively, if the local database data is no longer required, delete the Docker volume and recreate the container.

### Docker Cannot Find SQL_PASSWORD

Confirm that `docker/.env` exists and contains:

```dotenv
SQL_PASSWORD=YourStrongLocalPassword123!
```

Validate the configuration:

```bash
docker compose -f docker/docker-compose.yml config --quiet
```

## Additional Notes

- Docker is only used to host the local SQL Server instance.
- The .NET API runs independently using the .NET SDK.
- Database creation and migrations will be documented once Entity Framework Core integration is introduced.
- Docker volumes should not be committed or copied into the repository.