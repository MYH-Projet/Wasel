using Microsoft.EntityFrameworkCore;
using Wasel.Api.Modules.Deliveries.Entities;
using Wasel.Api.Modules.Drivers.Entities;
using Wasel.Api.Modules.Users.Entities;
using Wasel.Api.Modules.Documents.Entities;
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
    public DbSet<DriverDossier> DriverDossiers => Set<DriverDossier>();

    // Module: Deliveries
    public DbSet<Delivery> Deliveries => Set<Delivery>();

    // Module: Documents
    public DbSet<Document> Documents => Set<Document>();

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
            entity.HasIndex(e => e.PermitNumber).IsUnique();
            entity.HasOne(d => d.User)
            .WithOne(u => u.Driver)
            .HasForeignKey<Driver>(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        });

        // Driver dossiers
        modelBuilder.Entity<DriverDossier>(entity =>
        {
            entity.ToTable("driver_dossiers");

            entity.HasOne(dossier => dossier.Driver)
                .WithOne(driver => driver.Dossier)
                .HasForeignKey<DriverDossier>(dossier => dossier.DriverId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Documents
        modelBuilder.Entity<Document>(entity =>
        {
            entity.ToTable("documents");

            entity.HasOne(document => document.DriverDossier)
                .WithMany(dossier => dossier.Documents)
                .HasForeignKey(document => document.DriverDossierId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        
        // Deliveries
        modelBuilder.Entity<Delivery>(entity =>
        {
            entity.ToTable("deliveries");
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
