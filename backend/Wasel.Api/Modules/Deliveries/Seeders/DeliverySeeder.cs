using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Wasel.Api.Modules.Deliveries.Entities;
using Wasel.Api.Modules.Deliveries.Enums;
using Wasel.Api.Modules.Users.Entities;
using Wasel.Api.Modules.Users.Enums;
using Wasel.Api.Modules.Payments.Entities;
using Wasel.Api.Modules.Payments.Enums;
using Wasel.Api.Shared.Database;

namespace Wasel.Api.Modules.Deliveries.Seeders;

public static class DeliverySeeder
{
    public static async Task SeedAsync(DbContext context, CancellationToken cancellationToken = default)
    {
        if (context is not WaselDbContext dbContext) return;

        // Ensure we have a client user
        var clientEmail = "client_test@wasel.ma";
        var clientUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == clientEmail, cancellationToken);
        
        if (clientUser == null)
        {
            clientUser = new User
            {
                Id = Guid.NewGuid(),
                KeycloakId = Guid.NewGuid().ToString(),
                Email = clientEmail,
                FirstName = "Test",
                LastName = "Client",
                Phone = "+212600000000",
                Status = UserStatus.Active,
                Preference = new UserPreference
                {
                    ActiveAppMode = ActiveAppMode.CLIENT,
                    PreferredMode = ActiveAppMode.CLIENT
                }
            };
            await dbContext.Users.AddAsync(clientUser, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        // Get an approved driver if any exists
        var driver = await dbContext.Drivers.FirstOrDefaultAsync(d => d.Status == Wasel.Api.Modules.Drivers.Enums.DriverStatus.Approved, cancellationToken);

        // Clear existing deliveries and payments to ensure fresh seed
        var existingPayments = await dbContext.Payments.ToListAsync(cancellationToken);
        if (existingPayments.Any()) dbContext.Payments.RemoveRange(existingPayments);

        var existingHistories = await dbContext.DeliveryStatusHistories.ToListAsync(cancellationToken);
        if (existingHistories.Any()) dbContext.DeliveryStatusHistories.RemoveRange(existingHistories);

        var existingDeliveries = await dbContext.Deliveries.ToListAsync(cancellationToken);
        if (existingDeliveries.Any()) dbContext.Deliveries.RemoveRange(existingDeliveries);

        var existingParcels = await dbContext.Parcels.ToListAsync(cancellationToken);
        if (existingParcels.Any()) dbContext.Parcels.RemoveRange(existingParcels);

        var existingAddresses = await dbContext.Addresses.Where(a => a.ClientId == clientUser.Id).ToListAsync(cancellationToken);
        if (existingAddresses.Any()) dbContext.Addresses.RemoveRange(existingAddresses);

        await dbContext.SaveChangesAsync(cancellationToken);

        // Delivery 1: Created (Unassigned)
        var pickupAddress1 = new Address { Id = Guid.NewGuid(), ClientId = clientUser.Id, Label = "Supermarket Marjane", Street = "Route d'El Jadida", City = "Casablanca", Country = "Morocco", Latitude = 33.5600, Longitude = -7.6300 };
        var dropoffAddress1 = new Address { Id = Guid.NewGuid(), ClientId = clientUser.Id, Label = "Appartement A12", Street = "Rue Nour", City = "Casablanca", Country = "Morocco", Latitude = 33.5700, Longitude = -7.6400 };
        var parcel1 = new Parcel { Id = Guid.NewGuid(), Description = "Groceries & Vegetables", Weight = 8.5m, Volume = 0.02m, IsFragile = false };
        var delivery1 = new Delivery
        {
            Id = Guid.NewGuid(),
            ClientId = clientUser.Id,
            DriverId = null,
            PickupAddressId = pickupAddress1.Id,
            DropoffAddressId = dropoffAddress1.Id,
            PickupAddress = pickupAddress1,
            DropoffAddress = dropoffAddress1,
            ParcelId = parcel1.Id,
            Parcel = parcel1,
            Status = DeliveryStatus.CREATED,
            PaymentMethod = PaymentMethod.CASH,
            DistanceKm = 3.5m,
            Price = 45.00m,
            Currency = "MAD",
            CreatedAt = DateTime.UtcNow.AddMinutes(-45)
        };

        // Delivery 2: Accepted (Assigned to Approved Driver)
        var pickupAddress2 = new Address { Id = Guid.NewGuid(), ClientId = clientUser.Id, Label = "Pizza Hut Gauthier", Street = "Rue Hamza", City = "Casablanca", Country = "Morocco", Latitude = 33.5900, Longitude = -7.6200 };
        var dropoffAddress2 = new Address { Id = Guid.NewGuid(), ClientId = clientUser.Id, Label = "Tech Office", Street = "Boulevard Zerktouni", City = "Casablanca", Country = "Morocco", Latitude = 33.5850, Longitude = -7.6150 };
        var parcel2 = new Parcel { Id = Guid.NewGuid(), Description = "Double Feast Pizza Box", Weight = 1.8m, Volume = 0.01m, IsFragile = true };
        var delivery2 = new Delivery
        {
            Id = Guid.NewGuid(),
            ClientId = clientUser.Id,
            DriverId = driver?.Id,
            PickupAddressId = pickupAddress2.Id,
            DropoffAddressId = dropoffAddress2.Id,
            PickupAddress = pickupAddress2,
            DropoffAddress = dropoffAddress2,
            ParcelId = parcel2.Id,
            Parcel = parcel2,
            Status = DeliveryStatus.ACCEPTED,
            PaymentMethod = PaymentMethod.WALLET,
            DistanceKm = 2.1m,
            Price = 25.00m,
            Currency = "MAD",
            CreatedAt = DateTime.UtcNow.AddHours(-1)
        };

        // Delivery 3: In Transit (Assigned to Approved Driver)
        var pickupAddress3 = new Address { Id = Guid.NewGuid(), ClientId = clientUser.Id, Label = "Decathlon Casablanca", Street = "Ain Diab", City = "Casablanca", Country = "Morocco", Latitude = 33.5960, Longitude = -7.6650 };
        var dropoffAddress3 = new Address { Id = Guid.NewGuid(), ClientId = clientUser.Id, Label = "Villa 45", Street = "Anfa", City = "Casablanca", Country = "Morocco", Latitude = 33.5820, Longitude = -7.6520 };
        var parcel3 = new Parcel { Id = Guid.NewGuid(), Description = "Sports Equipment (Bicycle Helmet)", Weight = 3.2m, Volume = 0.03m, IsFragile = false };
        var delivery3 = new Delivery
        {
            Id = Guid.NewGuid(),
            ClientId = clientUser.Id,
            DriverId = driver?.Id,
            PickupAddressId = pickupAddress3.Id,
            DropoffAddressId = dropoffAddress3.Id,
            PickupAddress = pickupAddress3,
            DropoffAddress = dropoffAddress3,
            ParcelId = parcel3.Id,
            Parcel = parcel3,
            Status = DeliveryStatus.IN_TRANSIT,
            PaymentMethod = PaymentMethod.CARD,
            DistanceKm = 4.8m,
            Price = 60.00m,
            Currency = "MAD",
            CreatedAt = DateTime.UtcNow.AddHours(-2)
        };

        // Delivery 4: Delivered (Assigned to Approved Driver)
        var pickupAddress4 = new Address { Id = Guid.NewGuid(), ClientId = clientUser.Id, Label = "Store", Street = "789 Market Rd", City = "Rabat", Country = "Morocco", Latitude = 34.0209, Longitude = -6.8416 };
        var dropoffAddress4 = new Address { Id = Guid.NewGuid(), ClientId = clientUser.Id, Label = "Friend's House", Street = "321 Park Lane", City = "Rabat", Country = "Morocco", Latitude = 34.0100, Longitude = -6.8300 };
        var parcel4 = new Parcel { Id = Guid.NewGuid(), Description = "Gift box", Weight = 1.0m, Volume = 0.005m, IsFragile = false };
        var delivery4 = new Delivery
        {
            Id = Guid.NewGuid(),
            ClientId = clientUser.Id,
            DriverId = driver?.Id,
            PickupAddressId = pickupAddress4.Id,
            DropoffAddressId = dropoffAddress4.Id,
            PickupAddress = pickupAddress4,
            DropoffAddress = dropoffAddress4,
            ParcelId = parcel4.Id,
            Parcel = parcel4,
            Status = DeliveryStatus.DELIVERED,
            PaymentMethod = PaymentMethod.CARD,
            DistanceKm = 12.4m,
            Price = 80.00m,
            Currency = "MAD",
            CreatedAt = DateTime.UtcNow.AddDays(-3)
        };

        // Delivery 5: Cancelled
        var pickupAddress5 = new Address { Id = Guid.NewGuid(), ClientId = clientUser.Id, Label = "Starbucks Racine", Street = "Casablanca", City = "Casablanca", Country = "Morocco", Latitude = 33.5810, Longitude = -7.6350 };
        var dropoffAddress5 = new Address { Id = Guid.NewGuid(), ClientId = clientUser.Id, Label = "Appartement B6", Street = "Casablanca", City = "Casablanca", Country = "Morocco", Latitude = 33.5780, Longitude = -7.6310 };
        var parcel5 = new Parcel { Id = Guid.NewGuid(), Description = "Coffee and Pastries", Weight = 0.8m, Volume = 0.005m, IsFragile = true };
        var delivery5 = new Delivery
        {
            Id = Guid.NewGuid(),
            ClientId = clientUser.Id,
            DriverId = driver?.Id,
            PickupAddressId = pickupAddress5.Id,
            DropoffAddressId = dropoffAddress5.Id,
            PickupAddress = pickupAddress5,
            DropoffAddress = dropoffAddress5,
            ParcelId = parcel5.Id,
            Parcel = parcel5,
            Status = DeliveryStatus.CANCELLED_BY_CLIENT,
            PaymentMethod = PaymentMethod.CARD,
            DistanceKm = 1.2m,
            Price = 30.00m,
            Currency = "MAD",
            CreatedAt = DateTime.UtcNow.AddDays(-2)
        };

        // Save Addresses, Parcels, Deliveries
        await dbContext.Addresses.AddRangeAsync(new[] { 
            pickupAddress1, dropoffAddress1, 
            pickupAddress2, dropoffAddress2, 
            pickupAddress3, dropoffAddress3, 
            pickupAddress4, dropoffAddress4, 
            pickupAddress5, dropoffAddress5 
        }, cancellationToken);
        
        await dbContext.Parcels.AddRangeAsync(new[] { parcel1, parcel2, parcel3, parcel4, parcel5 }, cancellationToken);
        await dbContext.Deliveries.AddRangeAsync(new[] { delivery1, delivery2, delivery3, delivery4, delivery5 }, cancellationToken);

        // Add Status Histories
        delivery1.StatusHistories.Add(new DeliveryStatusHistory { Id = Guid.NewGuid(), DeliveryId = delivery1.Id, Status = DeliveryStatus.CREATED, Note = "Delivery request submitted by client.", ChangedAt = DateTime.UtcNow.AddMinutes(-45) });

        delivery2.StatusHistories.Add(new DeliveryStatusHistory { Id = Guid.NewGuid(), DeliveryId = delivery2.Id, Status = DeliveryStatus.CREATED, Note = "Order placed.", ChangedAt = DateTime.UtcNow.AddHours(-1) });
        delivery2.StatusHistories.Add(new DeliveryStatusHistory { Id = Guid.NewGuid(), DeliveryId = delivery2.Id, Status = DeliveryStatus.ACCEPTED, Note = "Driver accepted the mission.", ChangedAt = DateTime.UtcNow.AddHours(-1).AddMinutes(12) });

        delivery3.StatusHistories.Add(new DeliveryStatusHistory { Id = Guid.NewGuid(), DeliveryId = delivery3.Id, Status = DeliveryStatus.CREATED, Note = "Order initiated.", ChangedAt = DateTime.UtcNow.AddHours(-2) });
        delivery3.StatusHistories.Add(new DeliveryStatusHistory { Id = Guid.NewGuid(), DeliveryId = delivery3.Id, Status = DeliveryStatus.ACCEPTED, Note = "Driver assigned to delivery.", ChangedAt = DateTime.UtcNow.AddHours(-2).AddMinutes(15) });
        delivery3.StatusHistories.Add(new DeliveryStatusHistory { Id = Guid.NewGuid(), DeliveryId = delivery3.Id, Status = DeliveryStatus.PICKED_UP, Note = "Parcel picked up from store.", ChangedAt = DateTime.UtcNow.AddHours(-2).AddMinutes(45) });
        delivery3.StatusHistories.Add(new DeliveryStatusHistory { Id = Guid.NewGuid(), DeliveryId = delivery3.Id, Status = DeliveryStatus.IN_TRANSIT, Note = "Driver is on the way.", ChangedAt = DateTime.UtcNow.AddHours(-2).AddMinutes(50) });

        delivery4.StatusHistories.Add(new DeliveryStatusHistory { Id = Guid.NewGuid(), DeliveryId = delivery4.Id, Status = DeliveryStatus.CREATED, Note = "Gift delivery request submitted.", ChangedAt = DateTime.UtcNow.AddDays(-3) });
        delivery4.StatusHistories.Add(new DeliveryStatusHistory { Id = Guid.NewGuid(), DeliveryId = delivery4.Id, Status = DeliveryStatus.ACCEPTED, Note = "Driver accepted the job.", ChangedAt = DateTime.UtcNow.AddDays(-3).AddMinutes(5) });
        delivery4.StatusHistories.Add(new DeliveryStatusHistory { Id = Guid.NewGuid(), DeliveryId = delivery4.Id, Status = DeliveryStatus.PICKED_UP, Note = "Package picked up from store.", ChangedAt = DateTime.UtcNow.AddDays(-3).AddMinutes(20) });
        delivery4.StatusHistories.Add(new DeliveryStatusHistory { Id = Guid.NewGuid(), DeliveryId = delivery4.Id, Status = DeliveryStatus.IN_TRANSIT, Note = "In transit to Rabat destination.", ChangedAt = DateTime.UtcNow.AddDays(-3).AddMinutes(25) });
        delivery4.StatusHistories.Add(new DeliveryStatusHistory { Id = Guid.NewGuid(), DeliveryId = delivery4.Id, Status = DeliveryStatus.DELIVERED, Note = "Package successfully delivered to recipient.", ChangedAt = DateTime.UtcNow.AddDays(-3).AddMinutes(55) });

        delivery5.StatusHistories.Add(new DeliveryStatusHistory { Id = Guid.NewGuid(), DeliveryId = delivery5.Id, Status = DeliveryStatus.CREATED, Note = "Order placed.", ChangedAt = DateTime.UtcNow.AddDays(-2) });
        delivery5.StatusHistories.Add(new DeliveryStatusHistory { Id = Guid.NewGuid(), DeliveryId = delivery5.Id, Status = DeliveryStatus.CANCELLED_BY_CLIENT, Note = "Client cancelled the order.", ChangedAt = DateTime.UtcNow.AddDays(-2).AddMinutes(10) });

        // Add Payments
        var payment1 = new Payment { Id = Guid.NewGuid(), DeliveryId = delivery1.Id, Amount = delivery1.Price, Currency = delivery1.Currency, Method = delivery1.PaymentMethod, Status = PaymentStatus.PENDING, TransactionReference = "TXN-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper() };
        var payment2 = new Payment { Id = Guid.NewGuid(), DeliveryId = delivery2.Id, Amount = delivery2.Price, Currency = delivery2.Currency, Method = delivery2.PaymentMethod, Status = PaymentStatus.PAID, TransactionReference = "TXN-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper() };
        var payment3 = new Payment { Id = Guid.NewGuid(), DeliveryId = delivery3.Id, Amount = delivery3.Price, Currency = delivery3.Currency, Method = delivery3.PaymentMethod, Status = PaymentStatus.PAID, TransactionReference = "TXN-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper() };
        var payment4 = new Payment { Id = Guid.NewGuid(), DeliveryId = delivery4.Id, Amount = delivery4.Price, Currency = delivery4.Currency, Method = delivery4.PaymentMethod, Status = PaymentStatus.PAID, TransactionReference = "TXN-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper() };
        var payment5 = new Payment { Id = Guid.NewGuid(), DeliveryId = delivery5.Id, Amount = delivery5.Price, Currency = delivery5.Currency, Method = delivery5.PaymentMethod, Status = PaymentStatus.REFUNDED, TransactionReference = "TXN-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper() };

        await dbContext.Payments.AddRangeAsync(new[] { payment1, payment2, payment3, payment4, payment5 }, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
