using System.Data.Common;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;
using Wasel.Api.Shared.Database;
using Wasel.Api.Tests.Security;

namespace Wasel.Api.Tests.Fixtures;

public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer;

    public IntegrationTestWebAppFactory()
    {
        _dbContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15")
            .WithDatabase("wasel_integration_test")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Set environment to testing
        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            // Remove the existing DbContext configuration
            services.RemoveAll(typeof(DbContextOptions<WaselDbContext>));
            services.RemoveAll(typeof(DbConnection));

            // Add the new DbContext connected to the Testcontainer
            services.AddDbContext<WaselDbContext>(options =>
            {
                options.UseNpgsql(_dbContainer.GetConnectionString());
            });

            // Mock Authentication to bypass Keycloak for API integration tests
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthHandler.AuthenticationScheme;
                options.DefaultScheme = TestAuthHandler.AuthenticationScheme;
                options.DefaultChallengeScheme = TestAuthHandler.AuthenticationScheme;
            })
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                TestAuthHandler.AuthenticationScheme, options => { });
        });
    }

    public async Task InitializeAsync()
    {
        // Start the PostgreSQL Testcontainer before any tests run
        await _dbContainer.StartAsync();

        // Apply migrations automatically to the Testcontainer DB
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<WaselDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        // Stop and remove the Testcontainer after tests finish
        await _dbContainer.StopAsync();
        await _dbContainer.DisposeAsync();
    }
}
