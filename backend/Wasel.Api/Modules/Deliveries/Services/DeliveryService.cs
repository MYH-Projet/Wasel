using Wasel.Api.Modules.Deliveries.DTOs;
using Wasel.Api.Modules.Deliveries.Entities;
using Wasel.Api.Modules.Deliveries.Enums;
using Wasel.Api.Modules.Deliveries.Repositories;
using Microsoft.EntityFrameworkCore;
using Wasel.Api.Shared.Database;
using Wasel.Api.Modules.Users.Entities;
using Wasel.Api.Modules.Users.Enums;
using Wasel.Api.Modules.Drivers.Entities;
using Wasel.Api.Modules.Drivers.Repositories;
using Wasel.Api.Modules.Notifications.Enums;
using Wasel.Api.Modules.Notifications.Services;

namespace Wasel.Api.Modules.Deliveries.Services;

public class DeliveryService : IDeliveryService
{
    private readonly IDeliveryRepository _deliveryRepository;
    private readonly WaselDbContext _context;
    private readonly IDriverRepository _driverRepository;
    private readonly INotificationService _notificationService;

    public DeliveryService(
    IDeliveryRepository deliveryRepository,
    IDriverRepository driverRepository,
    WaselDbContext context,
    INotificationService notificationService)
    {
        _deliveryRepository = deliveryRepository;
        _driverRepository = driverRepository;
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<CreateDeliveryResponseDto> CreateDeliveryAsync(
        CreateDeliveryRequestDto request,
        string keycloakId)
    {
        ValidateRequest(request);

        var client = await _deliveryRepository.GetUserByKeycloakIdAsync(keycloakId);

        if (client is null)
        {
            throw new InvalidOperationException("User not found. Please sync your account first.");
        }

        var hasClientMode = await _deliveryRepository.UserHasClientActiveModeAsync(client.Id);

        if (!hasClientMode)
        {
            throw new UnauthorizedAccessException("Only clients can create delivery requests.");
        }

        var pickupAddress = new Address
        {
            ClientId = client.Id,
            Label = request.PickupAddress.Label.Trim(),
            Street = request.PickupAddress.Street.Trim(),
            City = request.PickupAddress.City.Trim(),
            PostalCode = request.PickupAddress.PostalCode,
            Country = string.IsNullOrWhiteSpace(request.PickupAddress.Country)
                ? "Morocco"
                : request.PickupAddress.Country.Trim(),
            Latitude = request.PickupAddress.Latitude,
            Longitude = request.PickupAddress.Longitude,
            AdditionalInfo = request.PickupAddress.AdditionalInfo
        };

        var dropoffAddress = new Address
        {
            ClientId = client.Id,
            Label = request.DropoffAddress.Label.Trim(),
            Street = request.DropoffAddress.Street.Trim(),
            City = request.DropoffAddress.City.Trim(),
            PostalCode = request.DropoffAddress.PostalCode,
            Country = string.IsNullOrWhiteSpace(request.DropoffAddress.Country)
                ? "Morocco"
                : request.DropoffAddress.Country.Trim(),
            Latitude = request.DropoffAddress.Latitude,
            Longitude = request.DropoffAddress.Longitude,
            AdditionalInfo = request.DropoffAddress.AdditionalInfo
        };

        var parcel = new Parcel
        {
            Description = request.Parcel.Description.Trim(),
            Weight = request.Parcel.Weight,
            Volume = request.Parcel.Volume,
            IsFragile = request.Parcel.IsFragile,
            Instructions = request.Parcel.Instructions
        };

        var delivery = new Delivery
        {
            ClientId = client.Id,
            Status = DeliveryStatus.CREATED,
            PaymentMethod = request.PaymentMethod
        };

        var histories = new List<DeliveryStatusHistory>
        {
            new()
            {
                Status = DeliveryStatus.CREATED,
                Note = "Delivery request created."
            },
            new()
            {
                Status = DeliveryStatus.WAITING_DRIVER,
                Note = "Waiting for driver assignment."
            }
        };

        delivery.Status = DeliveryStatus.WAITING_DRIVER;

        var createdDelivery = await _deliveryRepository.CreateDeliveryRequestAsync(
            pickupAddress,
            dropoffAddress,
            parcel,
            delivery,
            histories);

        return new CreateDeliveryResponseDto
        {
            DeliveryId = createdDelivery.Id,
            Status = createdDelivery.Status.ToString()
        };
    }

    private static void ValidateRequest(CreateDeliveryRequestDto request)
    {
        if (request.PickupAddress is null)
        {
            throw new ArgumentException("Pickup address is required.");
        }

        if (request.DropoffAddress is null)
        {
            throw new ArgumentException("Dropoff address is required.");
        }

        ValidateAddress(request.PickupAddress, "Pickup address");
        ValidateAddress(request.DropoffAddress, "Dropoff address");

        if (request.Parcel is null)
        {
            throw new ArgumentException("Parcel information is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Parcel.Description))
        {
            throw new ArgumentException("Parcel description is required.");
        }

        if (request.Parcel.Weight <= 0)
        {
            throw new ArgumentException("Parcel weight must be greater than zero.");
        }

        if (request.Parcel.Volume <= 0)
        {
            throw new ArgumentException("Parcel volume must be greater than zero.");
        }

        if (!Enum.IsDefined(typeof(PaymentMethod), request.PaymentMethod))
        {
            throw new ArgumentException("Invalid payment method.");
        }
    }

    private static void ValidateAddress(CreateAddressRequestDto address, string addressName)
    {
        if (string.IsNullOrWhiteSpace(address.Street))
        {
            throw new ArgumentException($"{addressName} street is required.");
        }

        if (string.IsNullOrWhiteSpace(address.City))
        {
            throw new ArgumentException($"{addressName} city is required.");
        }
    }

    public async Task<PagedResultDto<AvailableDeliveryDto>> GetAvailableDeliveriesAsync(
    string keycloakId,
    double latitude,
    double longitude,
    double? radiusKm,
    int page,
    int pageSize)
    {
        if (string.IsNullOrWhiteSpace(keycloakId))
        {
            throw new UnauthorizedAccessException("Token invalide.");
        }

        if (page <= 0)
        {
            page = 1;
        }

        if (pageSize <= 0)
        {
            pageSize = 10;
        }

        if (pageSize > 50)
        {
            pageSize = 50;
        }

        if (radiusKm.HasValue && radiusKm.Value <= 0)
        {
            throw new ArgumentException("Le rayon doit être supérieur à zéro.");
        }

        var user = await _deliveryRepository.GetUserByKeycloakIdAsync(keycloakId);

        if (user is null)
        {
            throw new InvalidOperationException("User not found. Please sync your account first.");
        }

        var hasDriverMode = await _deliveryRepository.UserHasDriverActiveModeAsync(user.Id);

        if (!hasDriverMode)
        {
            throw new UnauthorizedAccessException("Accessible uniquement en mode DRIVER.");
        }

        var hasApprovedDriver = await _deliveryRepository.UserHasApprovedDriverAsync(user.Id);

        if (!hasApprovedDriver)
        {
            throw new UnauthorizedAccessException("Livreur non approuvé.");
        }

        var deliveries = await _deliveryRepository
            .GetAvailableDeliveriesQuery()
            .ToListAsync();

        var availableDeliveries = deliveries
            .Select(delivery =>
            {
                var distanceKm = CalculateDistanceKm(
                    latitude,
                    longitude,
                    delivery.PickupAddress.Latitude!.Value,
                    delivery.PickupAddress.Longitude!.Value
                );

                return new AvailableDeliveryDto
                {
                    Id = delivery.Id,

                    PickupAddress = MapAddress(delivery.PickupAddress),

                    DropoffAddress = MapAddress(delivery.DropoffAddress),

                    EstimatedDistanceKm = Math.Round(distanceKm, 2),

                    Price = delivery.Price,

                    Weight = delivery.Parcel.Weight,

                    IsFragile = delivery.Parcel.IsFragile
                };
            });

        if (radiusKm.HasValue)
        {
            availableDeliveries = availableDeliveries
                .Where(d => d.EstimatedDistanceKm <= radiusKm.Value);
        }

        availableDeliveries = availableDeliveries
            .OrderBy(d => d.EstimatedDistanceKm);

        var totalItems = availableDeliveries.Count();

        var items = availableDeliveries
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedResultDto<AvailableDeliveryDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }

    private static AddressDto MapAddress(Address address)
    {
        return new AddressDto
        {
            Id = address.Id,
            Label = address.Label,
            Street = address.Street,
            City = address.City,
            PostalCode = address.PostalCode,
            Country = address.Country,
            Latitude = address.Latitude,
            Longitude = address.Longitude,
            AdditionalInfo = address.AdditionalInfo
        };
    }

    private static double CalculateDistanceKm(
        double lat1,
        double lon1,
        double lat2,
        double lon2)
    {
        const double earthRadiusKm = 6371;

        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);

        var a =
            Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
            Math.Cos(ToRadians(lat1)) *
            Math.Cos(ToRadians(lat2)) *
            Math.Sin(dLon / 2) *
            Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return earthRadiusKm * c;
    }

    private static double ToRadians(double value)
    {
        return value * Math.PI / 180;
    }

    public async Task<(bool Success, string Message, DeliveryStatus Status)> RespondToDeliveryAsync(
    Guid deliveryId, string keycloakId, bool accept)
    {
                using var transaction = await _context.Database.BeginTransactionAsync();

        var delivery = await _deliveryRepository.GetByIdAsync(deliveryId);
        if (delivery == null)
            return (false, "Delivery not found", DeliveryStatus.WAITING_DRIVER);

        var user = await _deliveryRepository.GetUserByKeycloakIdAsync(keycloakId);
        if (user == null)
            return (false, "User not found", delivery.Status);

        var driver = await _driverRepository.GetByUserIdAsync(user.Id);
        if (driver == null)
            return (false, "Driver profile not found", delivery.Status);

        if (accept)
        {
            if (delivery.Status == DeliveryStatus.ASSIGNED)
                return (false, "Delivery already assigned", delivery.Status);

            delivery.DriverId = driver.Id;
            delivery.Status = DeliveryStatus.ASSIGNED;

            await _deliveryRepository.UpdateAsync(delivery);
            await _deliveryRepository.AddStatusHistoryAsync(new DeliveryStatusHistory
            {
                DeliveryId = delivery.Id,
                Status = DeliveryStatus.ASSIGNED,
                ChangedAt = DateTime.UtcNow,
                ChangedByDriverId = driver.Id
            });

            await transaction.CommitAsync();

            // Notify client — after commit, non-blocking
            try
            {
                await _notificationService.CreateAsync(
                    delivery.ClientId,
                    NotificationType.DELIVERY_ASSIGNED,
                    "Livreur assigné",
                    "Un livreur a accepté votre livraison.");
            }
            catch { /* notification failure must not affect delivery */ }

            return (true, "Delivery accepted", delivery.Status);
        }
        else
        {
            await transaction.CommitAsync();
            return (true, "Delivery declined", delivery.Status);
        }
    }


    public async Task<(bool Success, string Message, DeliveryStatus Status)> UpdateDeliveryStatusAsync(
    Guid deliveryId, string keycloakId, DeliveryStatus newStatus, string? note)
    {
        var delivery = await _deliveryRepository.GetByIdAsync(deliveryId);
        if (delivery == null)
            return (false, "Delivery not found", delivery?.Status ?? DeliveryStatus.CREATED);

        var user = await _deliveryRepository.GetUserByKeycloakIdAsync(keycloakId);
        if (user == null)
            return (false, "User not found", delivery.Status);

        var driver = await _driverRepository.GetByUserIdAsync(user.Id);
        if (driver == null)
            return (false, "Driver profile not found", delivery.Status);

        if (delivery.DriverId != driver.Id)
            return (false, "Driver not assigned to this delivery", delivery.Status);

        
        var lastStatus = delivery.StatusHistories
            .OrderByDescending(h => h.ChangedAt)
            .FirstOrDefault()?.Status ?? DeliveryStatus.CREATED;

        if (!IsValidTransition(lastStatus, newStatus))
            return (false, $"Invalid transition from {lastStatus} to {newStatus}", lastStatus);

        
        var history = new DeliveryStatusHistory
        {
            DeliveryId = delivery.Id,
            Status = newStatus,
            ChangedAt = DateTime.UtcNow,
            Note = note,
            ChangedByDriverId = driver.Id
        };

        await _deliveryRepository.AddStatusHistoryAsync(history);

        delivery.Status = newStatus;
        await _deliveryRepository.UpdateAsync(delivery);

        // Notify client on ARRIVED_AT_PICKUP — non-blocking
        if (newStatus == DeliveryStatus.ARRIVED_AT_PICKUP)
        {
            try
            {
                await _notificationService.CreateAsync(
                    delivery.ClientId,
                    NotificationType.DRIVER_ARRIVING,
                    "Livreur en approche",
                    "Votre livreur est arrivé au point de retrait.");
            }
            catch { /* notification failure must not affect delivery */ }
        }

        return (true, "Status updated successfully", newStatus);
    }

    private bool IsValidTransition(DeliveryStatus current, DeliveryStatus next)
    {
        return current switch
        {
            DeliveryStatus.ASSIGNED => next == DeliveryStatus.ACCEPTED,
            DeliveryStatus.ACCEPTED => next == DeliveryStatus.ARRIVED_AT_PICKUP,
            DeliveryStatus.ARRIVED_AT_PICKUP => next == DeliveryStatus.PICKED_UP,
            DeliveryStatus.PICKED_UP => next == DeliveryStatus.IN_TRANSIT,
            DeliveryStatus.IN_TRANSIT => next == DeliveryStatus.ARRIVED_AT_DROPOFF,
            DeliveryStatus.ARRIVED_AT_DROPOFF => next == DeliveryStatus.DELIVERED,
            _ => false
        };
    }


    public async Task<User?> GetUserByKeycloakIdAsync(string keycloakId)
    => await _deliveryRepository.GetUserByKeycloakIdAsync(keycloakId);

    public async Task CancelDeliveryAsync(Guid deliveryId, User currentUser, List<string> roles, string? reason)
{
    var delivery = await _deliveryRepository.GetByIdAsync(deliveryId);
    if (delivery == null)
        throw new KeyNotFoundException("Livraison introuvable.");

    if (roles.Contains("CLIENT"))
    {
        if (delivery.Status > DeliveryStatus.ASSIGNED)
            throw new UnauthorizedAccessException(
                "Impossible d'annuler la livraison côté client.");

        await _deliveryRepository.CancelDeliveryAsync(
            delivery, DeliveryStatus.CANCELLED_BY_CLIENT, reason);
    }
    else if (roles.Contains("ADMIN"))
    {
        await _deliveryRepository.CancelDeliveryAsync(
            delivery, DeliveryStatus.CANCELLED_BY_ADMIN, reason);
    }
    else
    {
        throw new UnauthorizedAccessException("Utilisateur non autorisé à annuler la livraison.");
    }
}

    public async Task<DeliveryDetailResponseDto> GetDeliveryDetailAsync(
    Guid deliveryId,
    string keycloakId,
    IEnumerable<string> roles)
{
    var delivery = await _deliveryRepository.GetDeliveryWithDetailsAsync(deliveryId)
        ?? throw new KeyNotFoundException($"Livraison {deliveryId} introuvable.");

    var user = await _deliveryRepository.GetUserByKeycloakIdAsync(keycloakId)
        ?? throw new UnauthorizedAccessException("Utilisateur introuvable.");

    var driver = await _driverRepository.GetByUserIdAsync(user.Id);

    bool isAdmin  = roles.Contains("ADMIN");
    bool isOwner  = delivery.ClientId == user.Id;
    bool isDriver = delivery.DriverId.HasValue && driver != null && delivery.DriverId == driver.Id;

    if (!isAdmin && !isOwner && !isDriver)
        throw new UnauthorizedAccessException("Accès refusé à cette livraison.");

    return MapToDetailDto(delivery);
}


    private static DeliveryDetailResponseDto MapToDetailDto(Delivery d)
    {
        return new DeliveryDetailResponseDto
        {
            Id             = d.Id,
            DeliveryStatus = d.Status.ToString(),
            CreatedAt      = d.CreatedAt,
            UpdatedAt      = d.UpdatedAt ?? DateTime.MinValue,

            PickupAddress   = MapAddress(d.PickupAddress),
            DeliveryAddress = MapAddress(d.DropoffAddress),  // DropoffAddress → DeliveryAddress

            Parcel  = MapParcel(d.Parcel),
            Payment = new PaymentDetailDto(),                // pas de Payment sur Delivery

            AssignedDriver = null,                           // pas de Driver nav sur Delivery

            StatusHistory = d.StatusHistories?
                .OrderBy(h => h.ChangedAt)
                .Select(MapHistory)
                .ToList() ?? new List<DeliveryStatusHistoryDto>()
        };
    }

    private static ParcelDetailDto MapParcel(Parcel? p) =>
    p is null ? new ParcelDetailDto() : new ParcelDetailDto
    {
        Id          = p.Id,
        Description = p.Description,
        Weight      = (double)p.Weight,    
        Volume      = (double)p.Volume,  
        IsFragile   = p.IsFragile
    };

    private static AssignedDriverDto MapDriver(Driver drv) =>
        new AssignedDriverDto
        {
            Id          = drv.Id,
            FullName    = $"{drv.User?.FirstName} {drv.User?.LastName}".Trim(),
            PhoneNumber = drv.User?.Phone ?? string.Empty,
            Vehicle     = drv.Vehicle is null ? null : new VehicleInfoDto
            {
                Matricule = drv.Vehicle.Matricule,
                Marque    = drv.Vehicle.Marque,
                Model     = drv.Vehicle.Model,
                Country   = drv.Vehicle.Country
            }
        };

    private static DeliveryStatusHistoryDto MapHistory(DeliveryStatusHistory h) =>
    new DeliveryStatusHistoryDto
    {
        Id             = h.Id,
        DeliveryStatus = h.Status.ToString(),
        Comment        = h.Note ?? string.Empty,  
        ChangedAt      = h.ChangedAt
    };

    public async Task<object> GetMyMissionsAsync(string keycloakId, int page, int pageSize)
    {
        var user = await _deliveryRepository.GetUserByKeycloakIdAsync(keycloakId);

        if (user == null)
            throw new UnauthorizedAccessException("Utilisateur introuvable.");

        var hasDriverMode = await _deliveryRepository.UserHasDriverActiveModeAsync(user.Id);

        if (!hasDriverMode)
            throw new UnauthorizedAccessException("Accès réservé aux livreurs.");

        var driver = await _driverRepository.GetByUserIdAsync(user.Id);

        if (driver == null)
            throw new UnauthorizedAccessException("Profil livreur introuvable.");

        var missions = await _deliveryRepository.GetDriverMissionsAsync(driver.Id, page, pageSize);
        var total = await _deliveryRepository.CountDriverMissionsAsync(driver.Id);

        var items = missions.Select(d => new DriverMissionHistoryDto
        {
            DeliveryId = d.Id,
            Date = d.CreatedAt,
            PickupAddress = $"{d.PickupAddress.Street}, {d.PickupAddress.City}",
            DropoffAddress = $"{d.DropoffAddress.Street}, {d.DropoffAddress.City}",
            FinalStatus = d.Status.ToString(),
            AmountEarned = d.Price
        });

        return new
        {
            page,
            pageSize,
            totalItems = total,
            totalPages = (int)Math.Ceiling(total / (double)pageSize),
            items
        };
    }

    public async Task<object> GetMyDeliveriesAsync(string keycloakId, int page, int pageSize)
    {
        var user = await _deliveryRepository.GetUserByKeycloakIdAsync(keycloakId);

        if (user == null)
            throw new UnauthorizedAccessException("Utilisateur introuvable.");

        var hasClientMode = await _deliveryRepository.UserHasClientActiveModeAsync(user.Id);

        if (!hasClientMode)
            throw new UnauthorizedAccessException("Accès réservé aux clients.");

        var deliveries = await _deliveryRepository.GetClientDeliveriesAsync(user.Id, page, pageSize);
        var total = await _deliveryRepository.CountClientDeliveriesAsync(user.Id);

        var items = deliveries.Select(d => new ClientDeliveryHistoryDto
        {
            DeliveryId = d.Id,
            Date = d.CreatedAt,
            PickupAddress = $"{d.PickupAddress.Street}, {d.PickupAddress.City}",
            DropoffAddress = $"{d.DropoffAddress.Street}, {d.DropoffAddress.City}",
            FinalStatus = d.Status.ToString(),
            PricePaid = d.Price
        });

        return new
        {
            page,
            pageSize,
            totalItems = total,
            totalPages = (int)Math.Ceiling(total / (double)pageSize),
            items
        };
    }

    public async Task<PagedResultDto<AdminDeliveryListItemDto>> GetAdminDeliveriesAsync(
    int page,
    int pageSize,
    string? search,
    string? status,
    DateTime? startDate,
    DateTime? endDate)
    {
        if (page <= 0)
            page = 1;

        if (pageSize <= 0)
            pageSize = 10;

        List<DeliveryStatus>? parsedStatuses = null;

        if (!string.IsNullOrWhiteSpace(status))
        {
            parsedStatuses = new List<DeliveryStatus>();

            var statusValues = status.Split(',', StringSplitOptions.RemoveEmptyEntries);

            foreach (var value in statusValues)
            {
                if (!Enum.TryParse<DeliveryStatus>(value.Trim(), true, out var parsedStatus))
                    throw new InvalidOperationException($"Invalid delivery status: {value}");

                parsedStatuses.Add(parsedStatus);
            }
        }

        var result = await _deliveryRepository.GetAdminDeliveriesAsync(
            page,
            pageSize,
            search,
            parsedStatuses,
            startDate,
            endDate);

        return new PagedResultDto<AdminDeliveryListItemDto>
{
    Page = page,
    PageSize = pageSize,
    TotalItems = result.TotalCount,
    Items = result.Items.Select(d => new AdminDeliveryListItemDto
    {
        Id = d.Id,
        ClientId = d.ClientId,
        ClientName = $"{d.Client.FirstName} {d.Client.LastName}",
        DriverId = d.DriverId,
        Status = d.Status.ToString(),
        PaymentMethod = d.PaymentMethod.ToString(),
        DistanceKm = d.DistanceKm,
        Price = d.Price,
        CreatedAt = d.CreatedAt
    }).ToList()
};
    }

    public DeliveryEstimateResponseDto EstimateDelivery(DeliveryEstimateRequestDto request)
    {
        if (request.Weight <= 0)
            throw new ArgumentException("Weight must be greater than zero.");
        
        if (request.PickupLat < -90 || request.PickupLat > 90 || request.DropoffLat < -90 || request.DropoffLat > 90 ||
            request.PickupLng < -180 || request.PickupLng > 180 || request.DropoffLng < -180 || request.DropoffLng > 180)
        {
            throw new ArgumentException("Invalid coordinates.");
        }

        var distanceKm = CalculateDistanceKm(request.PickupLat, request.PickupLng, request.DropoffLat, request.DropoffLng);
        
        decimal baseFee = 10m;
        decimal distanceFee = (decimal)distanceKm * 2.5m;
        decimal weightFee = (decimal)request.Weight * 2m;
        decimal fragileFee = request.IsFragile ? 5m : 0m;
        
        decimal estimatedPrice = Math.Round(baseFee + distanceFee + weightFee + fragileFee, 2);

        return new DeliveryEstimateResponseDto
        {
            EstimatedPrice = estimatedPrice,
            Currency = "MAD",
            DistanceKm = Math.Round(distanceKm, 2),
            Breakdown = new DeliveryEstimateBreakdownDto
            {
                BaseFee = baseFee,
                DistanceFee = distanceFee,
                WeightFee = weightFee,
                FragileFee = fragileFee
            }
        };
    }

    public async Task<List<ActiveDeliveryResponseDto>> GetMyActiveDeliveriesAsync(string keycloakId)
    {
        var user = await _deliveryRepository.GetUserByKeycloakIdAsync(keycloakId);
        if (user == null)
            throw new UnauthorizedAccessException("Utilisateur introuvable.");

        var driver = await _driverRepository.GetByUserIdAsync(user.Id);

        var activeStatuses = new List<DeliveryStatus>
        {
            DeliveryStatus.CREATED,
            DeliveryStatus.WAITING_DRIVER,
            DeliveryStatus.ASSIGNED,
            DeliveryStatus.ACCEPTED,
            DeliveryStatus.ARRIVED_AT_PICKUP,
            DeliveryStatus.PICKED_UP,
            DeliveryStatus.IN_TRANSIT,
            DeliveryStatus.ARRIVED_AT_DROPOFF
        };

        var deliveries = await _deliveryRepository.GetActiveDeliveriesForUserAsync(user.Id, driver?.Id, activeStatuses);

        return deliveries.Select(d => new ActiveDeliveryResponseDto
        {
            Id = d.Id,
            Status = d.Status.ToString(),
            ClientId = d.ClientId,
            DriverId = d.DriverId,
            PickupAddress = MapAddress(d.PickupAddress),
            DropoffAddress = MapAddress(d.DropoffAddress),
            ParcelSummary = d.Parcel?.Description ?? string.Empty,
            Price = d.Price,
            CreatedAt = d.CreatedAt,
            UpdatedAt = d.UpdatedAt
        }).ToList();
    }
}