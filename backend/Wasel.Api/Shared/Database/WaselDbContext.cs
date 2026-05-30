using Microsoft.EntityFrameworkCore;
using Wasel.Api.Modules.Deliveries.Entities;
using Wasel.Api.Modules.Drivers.Entities;
using Wasel.Api.Modules.Users.Entities;
using Wasel.Api.Modules.Tracking.Entities;
using Wasel.Api.Shared.Common;

namespace Wasel.Api.Shared.Database;

/// <summary>
/// Main database context for the Wasel application.
/// Manages all entity sets and applies automatic audit timestamps.
/// </summary>
public class WaselDbContext : DbContext
{
    public WaselDbContext(DbContextOptions<WaselDbContext> options) : base(options) { }

    // Module: Users
    public DbSet<User> Users => Set<User>();

    // Module: Drivers
    public DbSet<Driver> Drivers => Set<Driver>();

    // Module: Deliveries
    public DbSet<Delivery> Deliveries => Set<Delivery>();

    // Module: Tracking
    public DbSet<TrackingPoint> TrackingPoints => Set<TrackingPoint>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Users
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.KeycloakId).IsUnique();
        });

        // Drivers
        modelBuilder.Entity<Driver>(entity =>
        {
            entity.ToTable("drivers");
            entity.HasIndex(e => e.LicenseNumber).IsUnique();
        });

        // Deliveries
        modelBuilder.Entity<Delivery>(entity =>
        {
            entity.ToTable("deliveries");
        });

        // Tracking
        modelBuilder.Entity<TrackingPoint>(entity =>
        {
            entity.ToTable("tracking_points");
            entity.HasIndex(e => new { e.DriverId, e.RecordedAt });
            entity.HasIndex(e => new { e.DeliveryId, e.RecordedAt });
        });
    }

    /// <summary>
    /// Automatically sets CreatedAt on new entities and UpdatedAt on modified entities.
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<BaseEntity>();

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
