using FluentAssertions;
using Moq;
using Wasel.Api.Modules.Wallets.DTOs;
using Wasel.Api.Modules.Wallets.Entities;
using Wasel.Api.Modules.Wallets.Enums;
using Wasel.Api.Modules.Wallets.Repositories;
using Wasel.Api.Modules.Wallets.Services;
using Wasel.Api.Shared.Database;
using Wasel.Api.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Wasel.Api.Tests.Unit.Wallets;

public class WalletServiceTests
{
    private readonly Mock<IWalletRepository> _walletRepoMock;
    private readonly WalletService _sut;

    public WalletServiceTests()
    {
        _walletRepoMock = new Mock<IWalletRepository>();
        var options = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<WaselDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        var context = new WaselDbContext(options);
        _sut = new WalletService(_walletRepoMock.Object, context);
    }

    [Fact]
    public async Task ProcessDeliveryPaymentAsync_Should_Throw_BadRequest_When_ClientWallet_Balance_Is_Insufficient()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var driverId = Guid.NewGuid();
        var deliveryId = Guid.NewGuid();
        var clientWallet = new ClientWallet { Id = Guid.NewGuid(), UserId = clientId, Balance = 20.0m };
        var driverWallet = new DriverWallet { Id = Guid.NewGuid(), DriverId = driverId, Balance = 50.0m };

        _walletRepoMock.Setup(x => x.GetClientWalletByUserIdAsync(clientId)).ReturnsAsync(clientWallet);
        _walletRepoMock.Setup(x => x.GetDriverWalletByDriverIdAsync(driverId)).ReturnsAsync(driverWallet);

        // Act
        var act = async () => await _sut.ProcessDeliveryPaymentAsync(clientId, driverId, 50.0m, "MAD", deliveryId);

        // Assert
        var ex = await act.Should().ThrowAsync<ApiException>();
        ex.Which.StatusCode.Should().Be(400); // Bad Request
        
        // Aucun débit ni crédit, ni sauvegarde
        clientWallet.Balance.Should().Be(20.0m);
        driverWallet.Balance.Should().Be(50.0m);
        
        _walletRepoMock.Verify(x => x.UpdateWalletAsync(It.IsAny<ClientWallet>()), Times.Never);
        _walletRepoMock.Verify(x => x.UpdateWalletAsync(It.IsAny<DriverWallet>()), Times.Never);
        _walletRepoMock.Verify(x => x.AddTransferAsync(It.IsAny<WalletTransfer>()), Times.Never);
        _walletRepoMock.Verify(x => x.AddTransactionAsync(It.IsAny<WalletTransaction>()), Times.Never);
    }

    [Fact]
    public async Task ProcessDeliveryPaymentAsync_Should_Debit_Client_And_Credit_Driver_When_Balance_Is_Sufficient()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var driverId = Guid.NewGuid();
        var deliveryId = Guid.NewGuid();
        var clientWallet = new ClientWallet { Id = Guid.NewGuid(), UserId = clientId, Balance = 100.0m };
        var driverWallet = new DriverWallet { Id = Guid.NewGuid(), DriverId = driverId, Balance = 50.0m };

        _walletRepoMock.Setup(x => x.GetClientWalletByUserIdAsync(clientId)).ReturnsAsync(clientWallet);
        _walletRepoMock.Setup(x => x.GetDriverWalletByDriverIdAsync(driverId)).ReturnsAsync(driverWallet);
        _walletRepoMock.Setup(x => x.AddTransferAsync(It.IsAny<WalletTransfer>())).Returns(Task.CompletedTask);
        _walletRepoMock.Setup(x => x.AddTransactionAsync(It.IsAny<WalletTransaction>())).Returns(Task.CompletedTask);

        // Act
        await _sut.ProcessDeliveryPaymentAsync(clientId, driverId, 50.0m, "MAD", deliveryId);

        // Assert
        clientWallet.Balance.Should().Be(50.0m);
        driverWallet.Balance.Should().Be(100.0m);

        _walletRepoMock.Verify(x => x.AddTransferAsync(It.Is<WalletTransfer>(t => 
            t.Type == WalletTransferType.DELIVERY_PAYMENT_SPLIT &&
            t.Status == WalletTransferStatus.COMPLETED &&
            t.Amount == 50.0m
        )), Times.Once);

        _walletRepoMock.Verify(x => x.AddTransactionAsync(It.Is<WalletTransaction>(t => 
            t.Direction == WalletTransactionDirection.DEBIT &&
            t.Reason == WalletTransactionReason.DELIVERY_PAYMENT &&
            t.DeliveryId == deliveryId &&
            t.WalletId == clientWallet.Id
        )), Times.Once);

        _walletRepoMock.Verify(x => x.AddTransactionAsync(It.Is<WalletTransaction>(t => 
            t.Direction == WalletTransactionDirection.CREDIT &&
            t.Reason == WalletTransactionReason.DELIVERY_EARNING &&
            t.DeliveryId == deliveryId &&
            t.WalletId == driverWallet.Id
        )), Times.Once);
    }

    [Fact]
    public async Task WithdrawDriverFundsAsync_Should_Throw_BadRequest_When_Balance_Is_Insufficient()
    {
        // Arrange
        var driverId = Guid.NewGuid();
        var driverWallet = new DriverWallet { Id = Guid.NewGuid(), DriverId = driverId, Balance = 40.0m };
        
        _walletRepoMock.Setup(x => x.GetDriverWalletByDriverIdAsync(driverId)).ReturnsAsync(driverWallet);

        var request = new WithdrawRequestDto { Amount = 100.0m, Currency = "MAD" };

        // Act
        var act = async () => await _sut.WithdrawDriverFundsAsync(driverId, request);

        // Assert
        var ex = await act.Should().ThrowAsync<ApiException>();
        ex.Which.StatusCode.Should().Be(400);

        _walletRepoMock.Verify(x => x.AddTransferAsync(It.IsAny<WalletTransfer>()), Times.Never);
        _walletRepoMock.Verify(x => x.AddTransactionAsync(It.IsAny<WalletTransaction>()), Times.Never);
    }

    [Fact]
    public async Task WithdrawDriverFundsAsync_Should_Throw_BadRequest_When_Amount_Is_Below_Minimum()
    {
        // Arrange
        var driverId = Guid.NewGuid();
        var driverWallet = new DriverWallet { Id = Guid.NewGuid(), DriverId = driverId, Balance = 200.0m };
        
        _walletRepoMock.Setup(x => x.GetDriverWalletByDriverIdAsync(driverId)).ReturnsAsync(driverWallet);

        var request = new WithdrawRequestDto { Amount = 10.0m, Currency = "MAD" };

        // Act
        var act = async () => await _sut.WithdrawDriverFundsAsync(driverId, request);

        // Assert
        var ex = await act.Should().ThrowAsync<ApiException>();
        ex.Which.StatusCode.Should().Be(400);
        
        _walletRepoMock.Verify(x => x.AddTransferAsync(It.IsAny<WalletTransfer>()), Times.Never);
        _walletRepoMock.Verify(x => x.AddTransactionAsync(It.IsAny<WalletTransaction>()), Times.Never);
    }

    [Fact]
    public async Task WithdrawDriverFundsAsync_Should_Create_Debit_Transaction_And_Pending_Transfer_When_Valid()
    {
        // Arrange
        var driverId = Guid.NewGuid();
        var driverWallet = new DriverWallet { Id = Guid.NewGuid(), DriverId = driverId, Balance = 200.0m };
        
        _walletRepoMock.Setup(x => x.GetDriverWalletByDriverIdAsync(driverId)).ReturnsAsync(driverWallet);
        _walletRepoMock.Setup(x => x.AddTransferAsync(It.IsAny<WalletTransfer>())).Returns(Task.CompletedTask);
        
        var request = new WithdrawRequestDto { Amount = 100.0m, Currency = "MAD" };

        // Act
        await _sut.WithdrawDriverFundsAsync(driverId, request);

        // Assert
        driverWallet.Balance.Should().Be(100.0m);

        _walletRepoMock.Verify(x => x.AddTransferAsync(It.Is<WalletTransfer>(t => 
            t.Type == WalletTransferType.WITHDRAWAL &&
            t.Status == WalletTransferStatus.PENDING &&
            t.Amount == 100.0m
        )), Times.Once);

        _walletRepoMock.Verify(x => x.AddTransactionAsync(It.Is<WalletTransaction>(t => 
            t.Direction == WalletTransactionDirection.DEBIT &&
            t.Reason == WalletTransactionReason.DRIVER_WITHDRAWAL &&
            t.DeliveryId == null
        )), Times.Once);
    }

    [Fact]
    public async Task EnsureDriverWalletExistsAsync_Should_Use_DriverId_Not_UserId()
    {
        // Arrange
        var driverId = Guid.NewGuid();
        _walletRepoMock.Setup(x => x.GetDriverWalletByDriverIdAsync(driverId)).ReturnsAsync((DriverWallet?)null);
        _walletRepoMock.Setup(x => x.CreateDriverWalletAsync(It.IsAny<DriverWallet>()))
            .ReturnsAsync(new DriverWallet { DriverId = driverId });

        // Act
        var result = await _sut.EnsureDriverWalletExistsAsync(driverId);

        // Assert
        result.DriverId.Should().Be(driverId);
        _walletRepoMock.Verify(x => x.GetDriverWalletByDriverIdAsync(driverId), Times.Once);
        _walletRepoMock.Verify(x => x.CreateDriverWalletAsync(It.IsAny<DriverWallet>()), Times.Once);
    }
}
