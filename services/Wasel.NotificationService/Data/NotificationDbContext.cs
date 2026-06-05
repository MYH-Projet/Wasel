using Microsoft.EntityFrameworkCore;
using Wasel.NotificationService.Entities;

namespace Wasel.NotificationService.Data;

public class NotificationDbContext : DbContext
{
    public NotificationDbContext(DbContextOptions<NotificationDbContext> options) : base(options) { }

    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<UserDeviceToken> UserDeviceTokens => Set<UserDeviceToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.ToTable("notifications");
            entity.HasKey(n => n.Id);

            entity.Property(n => n.Type)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(n => n.Status)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(n => n.Title)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(n => n.Body)
                .HasMaxLength(1000)
                .IsRequired();

            entity.Property(n => n.Channel)
                .HasMaxLength(50);

            entity.Property(n => n.RelatedEntityType)
                .HasMaxLength(50);

            entity.Property(n => n.FirebaseMessageId)
                .HasMaxLength(200);

            entity.Property(n => n.ErrorMessage)
                .HasMaxLength(1000);

            entity.HasIndex(n => new { n.UserId, n.CreatedAt });
            entity.HasIndex(n => new { n.UserId, n.Status });
        });

        modelBuilder.Entity<UserDeviceToken>(entity =>
        {
            entity.ToTable("user_device_tokens");
            entity.HasKey(t => t.Id);

            entity.Property(t => t.Token)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(t => t.Platform)
                .HasMaxLength(50)
                .IsRequired();

            entity.HasIndex(t => t.Token).IsUnique();
            entity.HasIndex(t => new { t.UserId, t.IsActive });
        });
    }
}
