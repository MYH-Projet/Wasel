using System.Net.Http.Headers;
using Wasel.Api.Modules.Deliveries.Entities;
using Wasel.Api.Modules.Deliveries.Enums;
using Wasel.Api.Modules.Drivers.Entities;
using Wasel.Api.Modules.Drivers.Enums;
using Wasel.Api.Modules.Payments.Entities;
using Wasel.Api.Modules.Payments.Enums;
using Wasel.Api.Modules.Users.Entities;
using Wasel.Api.Modules.Users.Enums;
using Wasel.Api.Modules.Wallets.Entities;
using Wasel.Api.Modules.Wallets.Enums;
using Wasel.Api.Shared.Database;
using Wasel.Api.Tests.Security;

namespace Wasel.Api.Tests.Integration.Fixtures;

public static class TestHelpers
{
    public static User CreateClientUser(Guid? id = null, string? keycloakId = null, string? email = null)
    {
        var userId = id ?? Guid.NewGuid();
        return new User
        {
            Id = userId,
            KeycloakId = keycloakId ?? $"kc-client-{userId}",
            Email = email ?? $"client-{userId}@test.com",
            FirstName = "Test",
            LastName = "Client",
            Status = UserStatus.Active,
            Preference = new UserPreference
            {
                ActiveAppMode = ActiveAppMode.CLIENT
            }
        };
    }

    public static (User User, Driver Driver) CreateDriverUser(Guid? userId = null, Guid? driverId = null, string? keycloakId = null, string? email = null)
    {
        var uId = userId ?? Guid.NewGuid();
        var dId = driverId ?? Guid.NewGuid();

        var user = new User
        {
            Id = uId,
            KeycloakId = keycloakId ?? $"kc-driver-{uId}",
            Email = email ?? $"driver-{uId}@test.com",
            FirstName = "Test",
            LastName = "Driver",
            Status = UserStatus.Active,
            Preference = new UserPreference
            {
                ActiveAppMode = ActiveAppMode.DRIVER
            }
        };

        var driver = new Driver
        {
            Id = dId,
            UserId = uId,
            PermitNumber = $"PERMIT-{dId.ToString().Substring(0, 8)}",
            Status = DriverStatus.Approved
        };

        return (user, driver);
    }

    public static Delivery CreateDelivery(Guid clientId, Guid? driverId, decimal price, PaymentMethod method, DeliveryStatus status = DeliveryStatus.CREATED)
    {
        var delivery = new Delivery
        {
            Id = Guid.NewGuid(),
            ClientId = clientId,
            DriverId = driverId,
            Price = price,
            Currency = "MAD",
            PaymentMethod = method,
            Status = status,
            PickupAddress = new Address { ClientId = clientId, Street = "123 Pickup St", City = "Casablanca", Country = "Morocco" },
            DropoffAddress = new Address { ClientId = clientId, Street = "456 Dropoff St", City = "Casablanca", Country = "Morocco" },
            Parcel = new Parcel { Description = "Test Parcel", Weight = 1.0m, Volume = 1.0m }
        };
        return delivery;
    }

    public static ClientWallet CreateClientWallet(Guid userId, decimal balance)
    {
        return new ClientWallet
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Balance = balance,
            Currency = "MAD",
            Status = WalletStatus.ACTIVE
        };
    }

    public static DriverWallet CreateDriverWallet(Guid driverId, decimal balance)
    {
        return new DriverWallet
        {
            Id = Guid.NewGuid(),
            DriverId = driverId,
            Balance = balance,
            Currency = "MAD",
            Status = WalletStatus.ACTIVE
        };
    }

    public static Payment CreatePayment(Guid deliveryId, decimal amount, PaymentMethod method, PaymentStatus status)
    {
        return new Payment
        {
            Id = Guid.NewGuid(),
            DeliveryId = deliveryId,
            Amount = amount,
            Currency = "MAD",
            Method = method,
            Status = status,
            TransactionReference = $"TX-{Guid.NewGuid().ToString().Substring(0, 8)}"
        };
    }

    public static SavedPaymentMethod CreateSavedPaymentMethod(Guid userId)
    {
        return new SavedPaymentMethod
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ProviderName = "Stripe",
            ProviderCustomerId = "cus_" + Guid.NewGuid().ToString().Substring(0, 8),
            ProviderPaymentMethodId = "pm_" + Guid.NewGuid().ToString().Substring(0, 8),
            CardBrand = "Visa",
            CardLast4 = "4242",
            IsDefault = true
        };
    }

    public static void SetAuthHeaders(HttpRequestMessage request, string keycloakId, string role, string email)
    {
        request.Headers.Add("X-Test-KeycloakId", keycloakId);
        request.Headers.Add("X-Test-Role", role);
        request.Headers.Add("X-Test-Email", email);
    }
}
