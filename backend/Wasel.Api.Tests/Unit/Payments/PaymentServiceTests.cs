using FluentAssertions;
using Moq;
using Wasel.Api.Modules.Deliveries.Entities;
using Wasel.Api.Modules.Deliveries.Enums;
using Wasel.Api.Modules.Deliveries.Repositories;
using Wasel.Api.Modules.Payments.DTOs;
using Wasel.Api.Modules.Payments.Entities;
using Wasel.Api.Modules.Payments.Enums;
using Wasel.Api.Modules.Payments.Repositories;
using Wasel.Api.Modules.Payments.Services;
using Wasel.Api.Modules.Wallets.Services;
using Wasel.Api.Modules.Drivers.Repositories;
using Wasel.Api.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Wasel.Api.Tests.Unit.Payments;

public class PaymentServiceTests
{
    private readonly Mock<IPaymentRepository> _paymentRepoMock;
    private readonly Mock<IDeliveryRepository> _deliveryRepoMock;
    private readonly Mock<IWalletService> _walletServiceMock;
    private readonly Mock<IDriverRepository> _driverRepoMock;
    private readonly PaymentService _sut;

    public PaymentServiceTests()
    {
        _paymentRepoMock = new Mock<IPaymentRepository>();
        _deliveryRepoMock = new Mock<IDeliveryRepository>();
        _walletServiceMock = new Mock<IWalletService>();
        
        _driverRepoMock = new Mock<IDriverRepository>();

        var options = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<Wasel.Api.Shared.Database.WaselDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        var context = new Wasel.Api.Shared.Database.WaselDbContext(options);

        _sut = new PaymentService(
            _paymentRepoMock.Object, 
            _deliveryRepoMock.Object, 
            _driverRepoMock.Object,
            _walletServiceMock.Object,
            null!, // IPaymentGateway
            context, // WaselDbContext
            null! // IWalletRepository
        );
    }

    [Fact]
    public async Task InitiatePaymentAsync_Should_Create_Pending_Payment_When_Method_Is_Cash()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var deliveryId = Guid.NewGuid();
        var delivery = new Delivery { Id = deliveryId, ClientId = currentUserId, Price = 50.0m, Currency = "MAD", PaymentMethod = PaymentMethod.CASH };
        
        _deliveryRepoMock.Setup(x => x.GetByIdAsync(deliveryId)).ReturnsAsync(delivery);
        _paymentRepoMock.Setup(x => x.GetPaymentByDeliveryIdAsync(deliveryId)).ReturnsAsync((Payment?)null);
        _paymentRepoMock.Setup(x => x.CreatePaymentAsync(It.IsAny<Payment>())).ReturnsAsync((Payment p) => p);

        var request = new InitiatePaymentRequestDto { DeliveryId = deliveryId, PaymentMethod = "CASH" };

        // Act
        var result = await _sut.InitiatePaymentAsync(request, currentUserId);

        // Assert
        result.Status.Should().Be(PaymentStatus.PENDING);
        result.PaymentUrl.Should().BeNull();

        _paymentRepoMock.Verify(x => x.CreatePaymentAsync(It.Is<Payment>(p => 
            p.Method == PaymentMethod.CASH && 
            p.Status == PaymentStatus.PENDING &&
            p.Amount == 50.0m
        )), Times.Once);

        _walletServiceMock.Verify(x => x.ProcessDeliveryPaymentAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task InitiatePaymentAsync_Should_Mark_Payment_As_Paid_When_Method_Is_Wallet_And_Balance_Is_Sufficient()
    {
        // Arrange
        var currentUserId = Guid.NewGuid(); // ClientId
        var driverId = Guid.NewGuid();
        var deliveryId = Guid.NewGuid();
        var driver = new Wasel.Api.Modules.Drivers.Entities.Driver { Id = driverId, UserId = Guid.NewGuid() };
        var delivery = new Delivery { Id = deliveryId, ClientId = currentUserId, DriverId = driverId, Price = 50.0m, Currency = "MAD", PaymentMethod = PaymentMethod.WALLET };
        
        _driverRepoMock.Setup(x => x.GetByIdAsync(driverId)).ReturnsAsync(driver);
        _deliveryRepoMock.Setup(x => x.GetByIdAsync(deliveryId)).ReturnsAsync(delivery);
        _paymentRepoMock.Setup(x => x.GetPaymentByDeliveryIdAsync(deliveryId)).ReturnsAsync((Payment?)null);
        _paymentRepoMock.Setup(x => x.CreatePaymentAsync(It.IsAny<Payment>())).ReturnsAsync((Payment p) => p);
        
        // WalletService simule un succès (pas d'exception lancée)
        _walletServiceMock.Setup(x => x.ProcessDeliveryPaymentAsync(currentUserId, driverId, 50.0m, "MAD", deliveryId))
            .Returns(Task.CompletedTask);

        var request = new InitiatePaymentRequestDto { DeliveryId = deliveryId, PaymentMethod = "WALLET" };

        // Act
        var result = await _sut.InitiatePaymentAsync(request, currentUserId);

        // Assert
        result.Status.Should().Be(PaymentStatus.PAID);
        
        _paymentRepoMock.Verify(x => x.CreatePaymentAsync(It.Is<Payment>(p => 
            p.Method == PaymentMethod.WALLET && 
            p.Status == PaymentStatus.PAID
        )), Times.Once);

        _walletServiceMock.Verify(x => x.ProcessDeliveryPaymentAsync(currentUserId, driverId, 50.0m, "MAD", deliveryId), Times.Once);
    }

    [Fact]
    public async Task InitiatePaymentAsync_Should_Return_BadRequest_When_ActivePayment_Already_Exists()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var deliveryId = Guid.NewGuid();
        var delivery = new Delivery { Id = deliveryId, ClientId = currentUserId, Price = 50.0m, Currency = "MAD", PaymentMethod = PaymentMethod.WALLET };
        
        var existingPayment = new Payment { Id = Guid.NewGuid(), Status = PaymentStatus.PAID }; // Déjà payé

        _deliveryRepoMock.Setup(x => x.GetByIdAsync(deliveryId)).ReturnsAsync(delivery);
        _paymentRepoMock.Setup(x => x.GetPaymentByDeliveryIdAsync(deliveryId)).ReturnsAsync(existingPayment);

        var request = new InitiatePaymentRequestDto { DeliveryId = deliveryId, PaymentMethod = "WALLET" };

        // Act
        var act = async () => await _sut.InitiatePaymentAsync(request, currentUserId);

        // Assert
        var ex = await act.Should().ThrowAsync<ApiException>();
        ex.Which.StatusCode.Should().Be(400);

        _paymentRepoMock.Verify(x => x.CreatePaymentAsync(It.IsAny<Payment>()), Times.Never);
        _walletServiceMock.Verify(x => x.ProcessDeliveryPaymentAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task ConfirmCashAsync_Should_Throw_BadRequest_When_Payment_Is_Not_Pending()
    {
        // Arrange
        var driverId = Guid.NewGuid(); // L'utilisateur courant (Livreur)
        var driver = new Wasel.Api.Modules.Drivers.Entities.Driver { Id = Guid.NewGuid(), UserId = driverId };
        var paymentId = Guid.NewGuid();
        var deliveryId = Guid.NewGuid();
        
        var delivery = new Delivery { Id = deliveryId, DriverId = driver.Id };
        var payment = new Payment { Id = paymentId, DeliveryId = deliveryId, Status = PaymentStatus.PAID }; // Déjà PAID

        _driverRepoMock.Setup(x => x.GetByUserIdAsync(driverId)).ReturnsAsync(driver);
        _paymentRepoMock.Setup(x => x.GetPaymentByIdAsync(paymentId)).ReturnsAsync(payment);
        _deliveryRepoMock.Setup(x => x.GetByIdAsync(deliveryId)).ReturnsAsync(delivery);

        // Act
        var act = async () => await _sut.ConfirmCashPaymentAsync(paymentId, driverId); // Note: currentUserId ici doit être mapé vers DriverId via le repository.

        // Assert
        var ex = await act.Should().ThrowAsync<ApiException>();
        ex.Which.StatusCode.Should().Be(400);

        _paymentRepoMock.Verify(x => x.UpdatePaymentAsync(It.IsAny<Payment>()), Times.Never);
    }
}
