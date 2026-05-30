using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Wasel.Api.Modules.Deliveries.Enums;
using Wasel.Api.Modules.Payments.DTOs;
using Wasel.Api.Modules.Payments.Enums;
using Wasel.Api.Modules.Wallets.Enums;
using Wasel.Api.Shared.Database;
using Wasel.Api.Tests.Integration.Fixtures;
using Wasel.Api.Tests.Fixtures;

namespace Wasel.Api.Tests.Integration;

public class PaymentsEndpointsTests : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly HttpClient _client;
    private readonly IntegrationTestWebAppFactory _factory;

    public PaymentsEndpointsTests(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task InitiatePayment_WithCash_ShouldCreatePendingPayment()
    {
        // Arrange
        var clientUser = TestHelpers.CreateClientUser();
        var delivery = TestHelpers.CreateDelivery(clientUser.Id, null, 50.0m, PaymentMethod.CASH, DeliveryStatus.DELIVERED);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<WaselDbContext>();
            db.Users.Add(clientUser);
            db.Deliveries.Add(delivery);
            await db.SaveChangesAsync();
        }

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/payments/initiate");
        TestHelpers.SetAuthHeaders(request, clientUser.KeycloakId, "CLIENT", clientUser.Email);
        request.Content = JsonContent.Create(new InitiatePaymentRequestDto
        {
            DeliveryId = delivery.Id,
            PaymentMethod = "CASH"
        });

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var jsonOptions = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        jsonOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        var result = await response.Content.ReadFromJsonAsync<InitiatePaymentResponseDto>(jsonOptions);
        
        result.Should().NotBeNull();
        result!.Status.Should().Be(PaymentStatus.PENDING);
        result.PaymentUrl.Should().BeNull();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<WaselDbContext>();
            var payment = await db.Payments.FirstOrDefaultAsync(p => p.DeliveryId == delivery.Id);
            payment.Should().NotBeNull();
            payment!.Method.Should().Be(PaymentMethod.CASH);
            payment.Amount.Should().Be(50.0m);
            payment.Currency.Should().Be("MAD");
            payment.Status.Should().Be(PaymentStatus.PENDING);

            var txCount = await db.WalletTransactions.CountAsync(t => t.DeliveryId == delivery.Id);
            txCount.Should().Be(0);
        }
    }

    [Fact]
    public async Task ConfirmCash_ByAssignedDriver_ShouldMarkPaidAndCreateTransaction()
    {
        // Arrange
        var clientUser = TestHelpers.CreateClientUser();
        var (driverUser, driver) = TestHelpers.CreateDriverUser();
        var delivery = TestHelpers.CreateDelivery(clientUser.Id, driver.Id, 75.0m, PaymentMethod.CASH, DeliveryStatus.DELIVERED);
        var payment = TestHelpers.CreatePayment(delivery.Id, 75.0m, PaymentMethod.CASH, PaymentStatus.PENDING);
        var driverWallet = TestHelpers.CreateDriverWallet(driver.Id, 0);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<WaselDbContext>();
            db.Users.Add(clientUser);
            db.Users.Add(driverUser);
            db.Drivers.Add(driver);
            db.DriverWallets.Add(driverWallet);
            db.Deliveries.Add(delivery);
            db.Payments.Add(payment);
            await db.SaveChangesAsync();
        }

        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/payments/{payment.Id}/confirm-cash");
        TestHelpers.SetAuthHeaders(request, driverUser.KeycloakId, "DRIVER", driverUser.Email);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<WaselDbContext>();
            var updatedPayment = await db.Payments.FindAsync(payment.Id);
            updatedPayment!.Status.Should().Be(PaymentStatus.PAID);

            var tx = await db.WalletTransactions.FirstOrDefaultAsync(t => t.DeliveryId == delivery.Id);
            tx.Should().NotBeNull();
            tx!.Direction.Should().Be(WalletTransactionDirection.CREDIT);
            tx.Reason.Should().Be(WalletTransactionReason.CASH_COLLECTION);
            tx.Amount.Should().Be(75.0m);
        }
    }

    [Fact]
    public async Task ConfirmCash_ByUnassignedDriver_ShouldReturnForbidden()
    {
        // Arrange
        var clientUser = TestHelpers.CreateClientUser();
        var (assignedDriverUser, assignedDriver) = TestHelpers.CreateDriverUser();
        var (unassignedDriverUser, unassignedDriver) = TestHelpers.CreateDriverUser(); // The one doing the request
        var delivery = TestHelpers.CreateDelivery(clientUser.Id, assignedDriver.Id, 75.0m, PaymentMethod.CASH, DeliveryStatus.DELIVERED);
        var payment = TestHelpers.CreatePayment(delivery.Id, 75.0m, PaymentMethod.CASH, PaymentStatus.PENDING);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<WaselDbContext>();
            db.Users.Add(clientUser);
            db.Users.Add(assignedDriverUser);
            db.Users.Add(unassignedDriverUser);
            db.Drivers.Add(assignedDriver);
            db.Drivers.Add(unassignedDriver);
            db.Deliveries.Add(delivery);
            db.Payments.Add(payment);
            await db.SaveChangesAsync();
        }

        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/payments/{payment.Id}/confirm-cash");
        TestHelpers.SetAuthHeaders(request, unassignedDriverUser.KeycloakId, "DRIVER", unassignedDriverUser.Email);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<WaselDbContext>();
            var updatedPayment = await db.Payments.FindAsync(payment.Id);
            updatedPayment!.Status.Should().Be(PaymentStatus.PENDING); // Not changed

            var txCount = await db.WalletTransactions.CountAsync(t => t.DeliveryId == delivery.Id);
            txCount.Should().Be(0);
        }
    }

    [Fact]
    public async Task ConfirmCash_WhenPaymentAlreadyPaid_ShouldReturnBadRequest()
    {
        // Arrange
        var clientUser = TestHelpers.CreateClientUser();
        var (driverUser, driver) = TestHelpers.CreateDriverUser();
        var delivery = TestHelpers.CreateDelivery(clientUser.Id, driver.Id, 75.0m, PaymentMethod.CASH, DeliveryStatus.DELIVERED);
        var payment = TestHelpers.CreatePayment(delivery.Id, 75.0m, PaymentMethod.CASH, PaymentStatus.PAID); // Already PAID
        var driverWallet = TestHelpers.CreateDriverWallet(driver.Id, 75.0m);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<WaselDbContext>();
            db.Users.Add(clientUser);
            db.Users.Add(driverUser);
            db.Drivers.Add(driver);
            db.DriverWallets.Add(driverWallet);
            db.Deliveries.Add(delivery);
            db.Payments.Add(payment);
            await db.SaveChangesAsync();
        }

        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/payments/{payment.Id}/confirm-cash");
        TestHelpers.SetAuthHeaders(request, driverUser.KeycloakId, "DRIVER", driverUser.Email);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<WaselDbContext>();
            var txCount = await db.WalletTransactions.CountAsync(t => t.DeliveryId == delivery.Id);
            txCount.Should().Be(0); // Pas de nouvelle transaction
        }
    }

    [Fact]
    public async Task GetPaymentDetails_ByClientOwner_ShouldReturnOk()
    {
        // Arrange
        var clientUser = TestHelpers.CreateClientUser();
        var delivery = TestHelpers.CreateDelivery(clientUser.Id, null, 120.0m, PaymentMethod.CASH);
        var payment = TestHelpers.CreatePayment(delivery.Id, 120.0m, PaymentMethod.CASH, PaymentStatus.PENDING);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<WaselDbContext>();
            db.Users.Add(clientUser);
            db.Deliveries.Add(delivery);
            db.Payments.Add(payment);
            await db.SaveChangesAsync();
        }

        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/payments/{delivery.Id}");
        TestHelpers.SetAuthHeaders(request, clientUser.KeycloakId, "CLIENT", clientUser.Email);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var jsonOptions = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        jsonOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        var result = await response.Content.ReadFromJsonAsync<PaymentDetailsResponseDto>(jsonOptions);
        result.Should().NotBeNull();
        result!.Amount.Should().Be(120.0m);
        result.Currency.Should().Be("MAD");
        result.Method.Should().Be("CASH");
        result.Status.Should().Be(PaymentStatus.PENDING);
    }

    [Fact]
    public async Task GetPaymentDetails_ByUnrelatedUser_ShouldReturnForbidden()
    {
        // Arrange
        var clientUser = TestHelpers.CreateClientUser();
        var unrelatedUser = TestHelpers.CreateClientUser();
        var delivery = TestHelpers.CreateDelivery(clientUser.Id, null, 120.0m, PaymentMethod.CASH);
        var payment = TestHelpers.CreatePayment(delivery.Id, 120.0m, PaymentMethod.CASH, PaymentStatus.PENDING);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<WaselDbContext>();
            db.Users.Add(clientUser);
            db.Users.Add(unrelatedUser);
            db.Deliveries.Add(delivery);
            db.Payments.Add(payment);
            await db.SaveChangesAsync();
        }

        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/payments/{delivery.Id}");
        TestHelpers.SetAuthHeaders(request, unrelatedUser.KeycloakId, "CLIENT", unrelatedUser.Email);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task InitiatePayment_WithWallet_InsufficientBalance_ShouldReturnBadRequest()
    {
        // Arrange
        var clientUser = TestHelpers.CreateClientUser();
        var (driverUser, driver) = TestHelpers.CreateDriverUser();
        var delivery = TestHelpers.CreateDelivery(clientUser.Id, driver.Id, 50.0m, PaymentMethod.WALLET, DeliveryStatus.DELIVERED);
        var clientWallet = TestHelpers.CreateClientWallet(clientUser.Id, 0.0m); // Balance = 0
        var driverWallet = TestHelpers.CreateDriverWallet(driver.Id, 0.0m);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<WaselDbContext>();
            db.Users.Add(clientUser);
            db.Users.Add(driverUser);
            db.Drivers.Add(driver);
            db.ClientWallets.Add(clientWallet);
            db.DriverWallets.Add(driverWallet);
            db.Deliveries.Add(delivery);
            await db.SaveChangesAsync();
        }

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/payments/initiate");
        TestHelpers.SetAuthHeaders(request, clientUser.KeycloakId, "CLIENT", clientUser.Email);
        request.Content = JsonContent.Create(new InitiatePaymentRequestDto
        {
            DeliveryId = delivery.Id,
            PaymentMethod = "WALLET"
        });

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<WaselDbContext>();
            var paymentCount = await db.Payments.CountAsync(p => p.DeliveryId == delivery.Id);
            paymentCount.Should().Be(0); // Pas d'insertion partielle

            var txCount = await db.WalletTransactions.CountAsync(t => t.DeliveryId == delivery.Id);
            txCount.Should().Be(0);

            var updatedClientWallet = await db.ClientWallets.FindAsync(clientWallet.Id);
            updatedClientWallet!.Balance.Should().Be(0.0m);
        }
    }

    [Fact]
    public async Task InitiatePayment_WithWallet_SufficientBalance_ShouldCreatePaidPaymentAndTransactions()
    {
        // Arrange
        var clientUser = TestHelpers.CreateClientUser();
        var (driverUser, driver) = TestHelpers.CreateDriverUser();
        var delivery = TestHelpers.CreateDelivery(clientUser.Id, driver.Id, 50.0m, PaymentMethod.WALLET, DeliveryStatus.DELIVERED);
        var clientWallet = TestHelpers.CreateClientWallet(clientUser.Id, 100.0m);
        var driverWallet = TestHelpers.CreateDriverWallet(driver.Id, 50.0m);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<WaselDbContext>();
            db.Users.Add(clientUser);
            db.Users.Add(driverUser);
            db.Drivers.Add(driver);
            db.ClientWallets.Add(clientWallet);
            db.DriverWallets.Add(driverWallet);
            db.Deliveries.Add(delivery);
            await db.SaveChangesAsync();
        }

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/payments/initiate");
        TestHelpers.SetAuthHeaders(request, clientUser.KeycloakId, "CLIENT", clientUser.Email);
        request.Content = JsonContent.Create(new InitiatePaymentRequestDto
        {
            DeliveryId = delivery.Id,
            PaymentMethod = "WALLET"
        });

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var jsonOptions = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        jsonOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        var result = await response.Content.ReadFromJsonAsync<InitiatePaymentResponseDto>(jsonOptions);
        result.Should().NotBeNull();
        result!.Status.Should().Be(PaymentStatus.PAID);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<WaselDbContext>();
            var payment = await db.Payments.FirstOrDefaultAsync(p => p.DeliveryId == delivery.Id);
            payment.Should().NotBeNull();
            payment!.Status.Should().Be(PaymentStatus.PAID);

            var updatedClientWallet = await db.ClientWallets.FindAsync(clientWallet.Id);
            updatedClientWallet!.Balance.Should().Be(50.0m); // 100 - 50

            var updatedDriverWallet = await db.DriverWallets.FindAsync(driverWallet.Id);
            updatedDriverWallet!.Balance.Should().Be(100.0m); // 50 + 50

            var transferCount = await db.WalletTransfers.CountAsync(t => t.Type == WalletTransferType.DELIVERY_PAYMENT_SPLIT);
            transferCount.Should().Be(1);

            var transfer = await db.WalletTransfers.FirstAsync();
            transfer.Status.Should().Be(WalletTransferStatus.COMPLETED);
            transfer.Amount.Should().Be(50.0m);

            var transactions = await db.WalletTransactions.Where(t => t.DeliveryId == delivery.Id).ToListAsync();
            transactions.Should().HaveCount(2);
            
            var clientTx = transactions.First(t => t.Direction == WalletTransactionDirection.DEBIT);
            clientTx.Reason.Should().Be(WalletTransactionReason.DELIVERY_PAYMENT);
            clientTx.WalletTransferId.Should().Be(transfer.Id);
            clientTx.WalletId.Should().Be(clientWallet.Id);

            var driverTx = transactions.First(t => t.Direction == WalletTransactionDirection.CREDIT);
            driverTx.Reason.Should().Be(WalletTransactionReason.DELIVERY_EARNING);
            driverTx.WalletTransferId.Should().Be(transfer.Id);
            driverTx.WalletId.Should().Be(driverWallet.Id);
        }
    }

    [Fact]
    public async Task InitiatePayment_Wallet_Twice_ShouldBlockDoublePayment()
    {
        // Arrange
        var clientUser = TestHelpers.CreateClientUser();
        var (driverUser, driver) = TestHelpers.CreateDriverUser();
        var delivery = TestHelpers.CreateDelivery(clientUser.Id, driver.Id, 50.0m, PaymentMethod.WALLET, DeliveryStatus.DELIVERED);
        var clientWallet = TestHelpers.CreateClientWallet(clientUser.Id, 100.0m);
        var driverWallet = TestHelpers.CreateDriverWallet(driver.Id, 50.0m);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<WaselDbContext>();
            db.Users.Add(clientUser);
            db.Users.Add(driverUser);
            db.Drivers.Add(driver);
            db.ClientWallets.Add(clientWallet);
            db.DriverWallets.Add(driverWallet);
            db.Deliveries.Add(delivery);
            await db.SaveChangesAsync();
        }

        var request1 = new HttpRequestMessage(HttpMethod.Post, "/api/payments/initiate");
        TestHelpers.SetAuthHeaders(request1, clientUser.KeycloakId, "CLIENT", clientUser.Email);
        request1.Content = JsonContent.Create(new InitiatePaymentRequestDto { DeliveryId = delivery.Id, PaymentMethod = "WALLET" });

        var request2 = new HttpRequestMessage(HttpMethod.Post, "/api/payments/initiate");
        TestHelpers.SetAuthHeaders(request2, clientUser.KeycloakId, "CLIENT", clientUser.Email);
        request2.Content = JsonContent.Create(new InitiatePaymentRequestDto { DeliveryId = delivery.Id, PaymentMethod = "WALLET" });

        // Act
        var response1 = await _client.SendAsync(request1);
        var response2 = await _client.SendAsync(request2);

        // Assert
        response1.StatusCode.Should().Be(HttpStatusCode.OK);
        response2.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<WaselDbContext>();
            var payments = await db.Payments.Where(p => p.DeliveryId == delivery.Id).ToListAsync();
            payments.Should().HaveCount(1); // Seulement 1

            var txs = await db.WalletTransactions.Where(t => t.DeliveryId == delivery.Id).ToListAsync();
            txs.Should().HaveCount(2); // Pas de double facturation
        }
    }

    [Fact]
    public async Task InitiatePayment_WithCard_ShouldCreatePendingPaymentWithUrl()
    {
        // Arrange
        var clientUser = TestHelpers.CreateClientUser();
        var delivery = TestHelpers.CreateDelivery(clientUser.Id, null, 100.0m, PaymentMethod.CARD, DeliveryStatus.DELIVERED);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<WaselDbContext>();
            db.Users.Add(clientUser);
            db.Deliveries.Add(delivery);
            await db.SaveChangesAsync();
        }

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/payments/initiate");
        TestHelpers.SetAuthHeaders(request, clientUser.KeycloakId, "CLIENT", clientUser.Email);
        request.Content = JsonContent.Create(new InitiatePaymentRequestDto
        {
            DeliveryId = delivery.Id,
            PaymentMethod = "CARD"
        });

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var jsonOptions = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        jsonOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        var result = await response.Content.ReadFromJsonAsync<InitiatePaymentResponseDto>(jsonOptions);
        result.Should().NotBeNull();
        result!.Status.Should().Be(PaymentStatus.PENDING);
        result.PaymentUrl.Should().NotBeNull();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<WaselDbContext>();
            var payment = await db.Payments.FirstOrDefaultAsync(p => p.DeliveryId == delivery.Id);
            payment.Should().NotBeNull();
            payment!.Method.Should().Be(PaymentMethod.CARD);
            payment.TransactionReference.Should().NotBeNull();
            
            var txCount = await db.WalletTransactions.CountAsync(t => t.DeliveryId == delivery.Id);
            txCount.Should().Be(0);
        }
    }
}
