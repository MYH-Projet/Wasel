using Microsoft.EntityFrameworkCore;
using Wasel.Api.Modules.Deliveries.Entities;
using Wasel.Api.Modules.Drivers.Entities;
using Wasel.Api.Modules.Users.Entities;
using Wasel.Api.Modules.Documents.Entities;
using Wasel.Api.Shared.Common;
using Wasel.Api.Modules.Complaints.Entities;
using Wasel.Api.Modules.Complaints.Entities;
using Wasel.Api.Modules.Messaging.Entities;

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
    public DbSet<UserPreference> UserPreferences => Set<UserPreference>();

    // Module: Drivers
    public DbSet<Driver> Drivers => Set<Driver>();
    public DbSet<DriverDossier> DriverDossiers => Set<DriverDossier>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    // Module: Deliveries
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<Parcel> Parcels => Set<Parcel>();
    public DbSet<Delivery> Deliveries => Set<Delivery>();
    public DbSet<DeliveryStatusHistory> DeliveryStatusHistories => Set<DeliveryStatusHistory>();
    // Module: Documents
    public DbSet<Document> Documents => Set<Document>();
    //Module complaints
    public DbSet<Complaint> Complaints => Set<Complaint>();
    public DbSet<ComplaintEvidence> ComplaintEvidences => Set<ComplaintEvidence>();
    //message
    public DbSet<Message> Messages => Set<Message>();
    
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

        modelBuilder.Entity<User>()
        .HasOne(u => u.Preference)
        .WithOne(p => p.User)
        .HasForeignKey<UserPreference>(p => p.UserId)
        .OnDelete(DeleteBehavior.Cascade);
        
        // Drivers
        // Drivers
    modelBuilder.Entity<Driver>(entity =>
    {
        entity.ToTable("drivers");
        entity.HasIndex(e => e.PermitNumber).IsUnique();

        // Relation 1 : Driver → User (1-1)
        entity.HasOne(d => d.User)
            .WithOne(u => u.Driver)
            .HasForeignKey<Driver>(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relation 2 : Driver → Vehicle (1-1)
        entity.HasOne(d => d.Vehicle)
            .WithOne(v => v.Driver)
            .HasForeignKey<Vehicle>(v => v.DriverId)
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
        modelBuilder.Entity<Delivery>()
        .Property(d => d.Status)
        .HasConversion<string>();

        modelBuilder.Entity<Delivery>()
            .Property(d => d.PaymentMethod)
            .HasConversion<string>();

        modelBuilder.Entity<DeliveryStatusHistory>()
            .Property(h => h.Status)
            .HasConversion<string>();
        
        modelBuilder.Entity<Address>(entity =>
        {
            entity.ToTable("addresses");

            entity.HasKey(a => a.Id);

            entity.Property(a => a.Label)
                .HasMaxLength(100);

            entity.Property(a => a.Street)
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(a => a.City)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(a => a.PostalCode)
                .HasMaxLength(30);

            entity.Property(a => a.Country)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(a => a.AdditionalInfo)
                .HasMaxLength(500);

            entity.HasOne(a => a.Client)
                .WithMany()
                .HasForeignKey(a => a.ClientId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<Parcel>(entity =>
        {
            entity.ToTable("parcels");

            entity.HasKey(p => p.Id);

            entity.Property(p => p.Description)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(p => p.Weight)
                .HasColumnType("decimal(10,2)")
                .IsRequired();

            entity.Property(p => p.Volume)
                .HasColumnType("decimal(10,2)")
                .IsRequired();

            entity.Property(p => p.Instructions)
                .HasMaxLength(500);
        });

        modelBuilder.Entity<Delivery>(entity =>
        {
            entity.ToTable("deliveries");

            entity.HasKey(d => d.Id);

            entity.Property(d => d.Status)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(d => d.PaymentMethod)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            entity.HasOne(d => d.Client)
                .WithMany()
                .HasForeignKey(d => d.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.PickupAddress)
                .WithMany()
                .HasForeignKey(d => d.PickupAddressId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.DropoffAddress)
                .WithMany()
                .HasForeignKey(d => d.DropoffAddressId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Parcel)
                .WithOne()
                .HasForeignKey<Delivery>(d => d.ParcelId)
                .OnDelete(DeleteBehavior.Cascade);
        });


        modelBuilder.Entity<DeliveryStatusHistory>(entity =>
        {
            entity.ToTable("delivery_status_histories");

            entity.HasKey(h => h.Id);

            entity.Property(h => h.Status)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(h => h.Note)
                .HasMaxLength(500);

            entity.HasOne(h => h.Delivery)
                .WithMany(d => d.StatusHistories)
                .HasForeignKey(h => h.DeliveryId)
                .OnDelete(DeleteBehavior.Cascade);
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
