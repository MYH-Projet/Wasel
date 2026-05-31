using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Wasel.Api.Modules.Wallets.DTOs;
using Wasel.Api.Modules.Wallets.Entities;
using Wasel.Api.Modules.Wallets.Enums;
using Wasel.Api.Shared.Database;
using Wasel.Api.Tests.Integration.Fixtures;
using Wasel.Api.Tests.Fixtures;

namespace Wasel.Api.Tests.Integration;

public class WalletEndpointsTests : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly HttpClient _client;
    private readonly IntegrationTestWebAppFactory _factory;

    public WalletEndpointsTests(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetClientWallet_ShouldReturnOk_AndCreateWalletIfNotExists()
    {
        // Arrange
        var clientUser = TestHelpers.CreateClientUser();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<WaselDbContext>();
            db.Users.Add(clientUser);
            await db.SaveChangesAsync();
        }

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/wallet/me");
        TestHelpers.SetAuthHeaders(request, clientUser.KeycloakId, "CLIENT", clientUser.Email);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var jsonOptions = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        jsonOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        var result = await response.Content.ReadFromJsonAsync<WalletBalanceResponseDto>(jsonOptions);
        result.Should().NotBeNull();
        result!.Currency.Should().Be("MAD");
        result.Status.Should().Be(WalletStatus.ACTIVE);
        
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<WaselDbContext>();
            var wallet = await db.ClientWallets.FirstOrDefaultAsync(w => w.UserId == clientUser.Id);
            wallet.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task GetDriverWallet_WithUserIdAndDriverIdBug_ShouldUseDriverId()
    {
        // Ce test s'assure du bug remonté : on utilise le DriverId et non le UserId pour les DriverWallet.
        // Arrange
        var (driverUser, driver) = TestHelpers.CreateDriverUser(); // UserId != DriverId

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<WaselDbContext>();
            db.Users.Add(driverUser);
            db.Drivers.Add(driver);
            await db.SaveChangesAsync();
        }

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/wallet/driver/me");
        TestHelpers.SetAuthHeaders(request, driverUser.KeycloakId, "DRIVER", driverUser.Email);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var jsonOptions = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        jsonOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        var result = await response.Content.ReadFromJsonAsync<WalletBalanceResponseDto>(jsonOptions);
        result.Should().NotBeNull();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<WaselDbContext>();
            
            // Le DriverWallet DOIT être lié au DriverId. S'il est lié au UserId, c'est le bug.
            var correctWallet = await db.DriverWallets.FirstOrDefaultAsync(w => w.DriverId == driver.Id);
            correctWallet.Should().NotBeNull();

            // Vérifier qu'aucun DriverWallet n'a été créé par erreur avec le UserId dans le champ DriverId
            var buggedWallet = await db.DriverWallets.FirstOrDefaultAsync(w => w.DriverId == driverUser.Id);
            buggedWallet.Should().BeNull("Le bug de confusion UserId/DriverId est présent !");
        }
    }

    [Fact]
    public async Task GetClientTransactions_ShouldReturnPaginatedListSortedByDate()
    {
        // Arrange
        var clientUser = TestHelpers.CreateClientUser();
        var clientWallet = TestHelpers.CreateClientWallet(clientUser.Id, 100);

        var tx1 = new WalletTransaction { Id = Guid.NewGuid(), WalletId = clientWallet.Id, Amount = 10, Currency = "MAD", Direction = WalletTransactionDirection.CREDIT, Reason = WalletTransactionReason.MANUAL_ADJUSTMENT, CreatedAt = DateTime.UtcNow.AddDays(-2) };
        var tx2 = new WalletTransaction { Id = Guid.NewGuid(), WalletId = clientWallet.Id, Amount = 5, Currency = "MAD", Direction = WalletTransactionDirection.DEBIT, Reason = WalletTransactionReason.DELIVERY_PAYMENT, CreatedAt = DateTime.UtcNow };

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<WaselDbContext>();
            db.Users.Add(clientUser);
            db.ClientWallets.Add(clientWallet);
            db.WalletTransactions.AddRange(tx1, tx2);
            await db.SaveChangesAsync();
        }

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/wallet/me/transactions?page=1&pageSize=10");
        TestHelpers.SetAuthHeaders(request, clientUser.KeycloakId, "CLIENT", clientUser.Email);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain(tx1.Id.ToString());
        json.Should().Contain(tx2.Id.ToString());
    }

    [Fact]
    public async Task WithdrawDriverFunds_WithSufficientBalance_ShouldCreateTransferAndTransaction()
    {
        // Arrange
        var (driverUser, driver) = TestHelpers.CreateDriverUser();
        var driverWallet = TestHelpers.CreateDriverWallet(driver.Id, 200.0m);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<WaselDbContext>();
            db.Users.Add(driverUser);
            db.Drivers.Add(driver);
            db.DriverWallets.Add(driverWallet);
            await db.SaveChangesAsync();
        }

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/wallet/driver/withdraw");
        TestHelpers.SetAuthHeaders(request, driverUser.KeycloakId, "DRIVER", driverUser.Email);
        request.Content = JsonContent.Create(new WithdrawRequestDto { Amount = 100.0m, Currency = "MAD" });

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<WaselDbContext>();
            
            var updatedWallet = await db.DriverWallets.FindAsync(driverWallet.Id);
            updatedWallet!.Balance.Should().Be(100.0m); // 200 - 100

            var tx = await db.WalletTransactions.FirstOrDefaultAsync(t => t.WalletId == driverWallet.Id);
            tx.Should().NotBeNull();
            tx!.Direction.Should().Be(WalletTransactionDirection.DEBIT);
            tx.Reason.Should().Be(WalletTransactionReason.DRIVER_WITHDRAWAL);
        }
    }

    [Fact]
    public async Task WithdrawDriverFunds_WithInsufficientBalance_ShouldReturnBadRequest()
    {
        // Arrange
        var (driverUser, driver) = TestHelpers.CreateDriverUser();
        var driverWallet = TestHelpers.CreateDriverWallet(driver.Id, 40.0m);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<WaselDbContext>();
            db.Users.Add(driverUser);
            db.Drivers.Add(driver);
            db.DriverWallets.Add(driverWallet);
            await db.SaveChangesAsync();
        }

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/wallet/driver/withdraw");
        TestHelpers.SetAuthHeaders(request, driverUser.KeycloakId, "DRIVER", driverUser.Email);
        request.Content = JsonContent.Create(new WithdrawRequestDto { Amount = 100.0m, Currency = "MAD" }); // > Balance

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<WaselDbContext>();
            
            var updatedWallet = await db.DriverWallets.FindAsync(driverWallet.Id);
            updatedWallet!.Balance.Should().Be(40.0m); // Inchangé

            var txCount = await db.WalletTransactions.CountAsync(t => t.WalletId == driverWallet.Id);
            txCount.Should().Be(0);
        }
    }

    [Fact]
    public async Task WithdrawDriverFunds_BelowMinimum_ShouldReturnBadRequest()
    {
        // Arrange
        var (driverUser, driver) = TestHelpers.CreateDriverUser();
        var driverWallet = TestHelpers.CreateDriverWallet(driver.Id, 200.0m);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<WaselDbContext>();
            db.Users.Add(driverUser);
            db.Drivers.Add(driver);
            db.DriverWallets.Add(driverWallet);
            await db.SaveChangesAsync();
        }

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/wallet/driver/withdraw");
        TestHelpers.SetAuthHeaders(request, driverUser.KeycloakId, "DRIVER", driverUser.Email);
        request.Content = JsonContent.Create(new WithdrawRequestDto { Amount = 10.0m, Currency = "MAD" }); // < 50 MAD

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<WaselDbContext>();
            var updatedWallet = await db.DriverWallets.FindAsync(driverWallet.Id);
            updatedWallet!.Balance.Should().Be(200.0m); // Inchangé
        }
    }
}
