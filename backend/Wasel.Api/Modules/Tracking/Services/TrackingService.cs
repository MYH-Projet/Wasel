using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Wasel.Api.Modules.Auth.Services;
using Wasel.Api.Modules.Drivers.Enums;
using Wasel.Api.Modules.Tracking.DTOs;
using Wasel.Api.Modules.Tracking.Entities;
using Wasel.Api.Modules.Tracking.Repositories;
using Wasel.Api.Shared.Exceptions;

namespace Wasel.Api.Modules.Tracking.Services;

public class TrackingService : ITrackingService
{
    private readonly ITrackingRepository _trackingRepository;
    private readonly IAuthService _authService;
    private readonly IMemoryCache _memoryCache;

    public TrackingService(ITrackingRepository trackingRepository, IAuthService authService, IMemoryCache memoryCache)
    {
        _trackingRepository = trackingRepository;
        _authService = authService;
        _memoryCache = memoryCache;
    }

    public async Task<TrackingPointResponseDto> UpdateCurrentDriverPositionAsync(TrackingPointUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var currentUser = await _authService.EnsureCurrentUserExistsAsync();
        
        if (!currentUser.LocalUserId.HasValue)
        {
            throw ApiException.Unauthorized("Utilisateur local introuvable.");
        }

        var driver = await _trackingRepository.GetDriverByUserIdAsync(currentUser.LocalUserId.Value, cancellationToken);
        if (driver == null)
        {
            throw ApiException.Forbidden("Vous n'avez pas de profil de livreur associé.");
        }

        if (driver.Status != DriverStatus.Approved)
        {
            throw ApiException.Forbidden("Votre compte livreur n'est pas actif/approuvé.");
        }

        var recordedAt = dto.RecordedAt ?? DateTime.UtcNow;
        bool shouldPersist = false;

        string cacheKeyTime = $"Tracking_LastSaveTime_{driver.Id}";
        string cacheKeyDelivery = $"Tracking_LastDelivery_{driver.Id}";

        _memoryCache.TryGetValue(cacheKeyTime, out DateTime lastSaveTime);
        _memoryCache.TryGetValue(cacheKeyDelivery, out Guid? lastDeliveryId);

        if (lastSaveTime == default)
        {
            shouldPersist = true; // Première position enregistrée (depuis redémarrage)
        }
        else if (dto.DeliveryId != lastDeliveryId)
        {
            shouldPersist = true; // Changement de livraison affectée
        }
        else if ((DateTime.UtcNow - lastSaveTime).TotalSeconds >= 5)
        {
            shouldPersist = true; // Plus de 5 secondes depuis la dernière persistance
        }

        TrackingPoint? savedPoint = null;

        if (shouldPersist)
        {
            var point = new TrackingPoint
            {
                DriverId = driver.Id,
                DeliveryId = dto.DeliveryId,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                Heading = dto.Heading,
                SpeedKmh = dto.SpeedKmh,
                AccuracyMeters = dto.AccuracyMeters,
                RecordedAt = recordedAt
            };

            savedPoint = await _trackingRepository.AddTrackingPointAsync(point, cancellationToken);
            
            _memoryCache.Set(cacheKeyTime, DateTime.UtcNow, TimeSpan.FromMinutes(10));
            _memoryCache.Set(cacheKeyDelivery, dto.DeliveryId, TimeSpan.FromMinutes(10));
        }

        return new TrackingPointResponseDto
        {
            Id = savedPoint?.Id,
            Persisted = shouldPersist,
            DriverId = driver.Id,
            DeliveryId = dto.DeliveryId,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            Heading = dto.Heading,
            SpeedKmh = dto.SpeedKmh,
            AccuracyMeters = dto.AccuracyMeters,
            RecordedAt = recordedAt
        };
    }

    public async Task<TrackingPointResponseDto?> GetLastPositionByDriverIdAsync(Guid driverId, CancellationToken cancellationToken = default)
    {
        var position = await _trackingRepository.GetLastPositionByDriverIdAsync(driverId, cancellationToken);
        return position != null ? MapToDto(position) : null;
    }

    public async Task<TrackingPointResponseDto?> GetLastPositionByDeliveryIdAsync(Guid deliveryId, CancellationToken cancellationToken = default)
    {
        var position = await _trackingRepository.GetLastPositionByDeliveryIdAsync(deliveryId, cancellationToken);
        return position != null ? MapToDto(position) : null;
    }

    private static TrackingPointResponseDto MapToDto(TrackingPoint entity)
    {
        return new TrackingPointResponseDto
        {
            Id = entity.Id,
            Persisted = true, // S'il vient de la base, il est persisté
            DriverId = entity.DriverId,
            DeliveryId = entity.DeliveryId,
            Latitude = entity.Latitude,
            Longitude = entity.Longitude,
            Heading = entity.Heading,
            SpeedKmh = entity.SpeedKmh,
            AccuracyMeters = entity.AccuracyMeters,
            RecordedAt = entity.RecordedAt
        };
    }
}
