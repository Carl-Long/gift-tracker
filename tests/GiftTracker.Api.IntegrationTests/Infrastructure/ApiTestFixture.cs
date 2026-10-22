using GiftTracker.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;

namespace GiftTracker.Api.IntegrationTests.Infrastructure;

public sealed class ApiTestFixture : IAsyncLifetime
{
    private const string TestDatabaseName = "GiftTrackerIntegrationTests";
    private readonly MsSqlContainer _sqlServer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-CU14-ubuntu-22.04").Build();
    private GiftTrackerApiFactory? _factory;

    public GiftTrackerApiFactory Factory => _factory ?? throw new InvalidOperationException("The API test factory has not been initialised.");

    public async Task InitializeAsync()
    {
        await _sqlServer.StartAsync();

        var connectionString = new SqlConnectionStringBuilder(_sqlServer.GetConnectionString())
        { InitialCatalog = TestDatabaseName }.ConnectionString;

        _factory = new GiftTrackerApiFactory(connectionString);

        using var scope = Factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<GiftTrackerDbContext>();

        if (dbContext.Database.GetDbConnection().Database != TestDatabaseName)
        {
            throw new InvalidOperationException("Integration tests are not connected to the test database.");
        }

        await dbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        if (_factory is not null)
        {
            await _factory.DisposeAsync();
        }

        await _sqlServer.DisposeAsync();
    }
}
