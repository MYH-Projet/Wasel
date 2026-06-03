using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Wasel.Api.Modules.Drivers.Entities;
using Wasel.Api.Modules.Drivers.Enums;
using Wasel.Api.Modules.Users.Entities;
using Wasel.Api.Modules.Users.Enums;
using Wasel.Api.Modules.Documents.Entities;
using Wasel.Api.Modules.Documents.Enums;
using Wasel.Api.Shared.Database;

namespace Wasel.Api.Modules.Drivers.Seeders;

public static class DriverSeeder
{
    public static async Task SeedAsync(DbContext context, CancellationToken cancellationToken = default)
    {
        if (context is not WaselDbContext dbContext) return;

        await SeedDriverAsync(dbContext, "driver_pending@wasel.ma", "Pending", "Driver", "DRV-100", DriverStatus.PendingVerification, DriverDossierStatus.Submitted, cancellationToken);
        await SeedDriverAsync(dbContext, "driver_approved@wasel.ma", "Approved", "Driver", "DRV-200", DriverStatus.Approved, DriverDossierStatus.Approved, cancellationToken);
        await SeedDriverAsync(dbContext, "driver_rejected@wasel.ma", "Rejected", "Driver", "DRV-300", DriverStatus.Rejected, DriverDossierStatus.Rejected, cancellationToken);
        await SeedDriverAsync(dbContext, "driver_suspended@wasel.ma", "Suspended", "Driver", "DRV-400", DriverStatus.Suspended, DriverDossierStatus.Approved, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public static void Seed(DbContext context)
    {
        if (context is not WaselDbContext dbContext) return;

        SeedDriver(dbContext, "driver_pending@wasel.ma", "Pending", "Driver", "DRV-100", DriverStatus.PendingVerification, DriverDossierStatus.Submitted);
        SeedDriver(dbContext, "driver_approved@wasel.ma", "Approved", "Driver", "DRV-200", DriverStatus.Approved, DriverDossierStatus.Approved);
        SeedDriver(dbContext, "driver_rejected@wasel.ma", "Rejected", "Driver", "DRV-300", DriverStatus.Rejected, DriverDossierStatus.Rejected);
        SeedDriver(dbContext, "driver_suspended@wasel.ma", "Suspended", "Driver", "DRV-400", DriverStatus.Suspended, DriverDossierStatus.Approved);

        dbContext.SaveChanges();
    }

    private static async Task SeedDriverAsync(WaselDbContext context, string email, string firstName, string lastName, string permitNumber, DriverStatus driverStatus, DriverDossierStatus dossierStatus, CancellationToken cancellationToken)
    {
        var exists = await context.Users.AnyAsync(u => u.Email == email, cancellationToken);
        if (!exists)
        {
            var userId = Guid.NewGuid();
            var driverId = Guid.NewGuid();
            var dossierId = Guid.NewGuid();
            var keycloakId = Guid.NewGuid();

            var user = new User
            {
                Id = userId,
                KeycloakId = keycloakId.ToString(),
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                Cin = "CIN" + permitNumber,
                Phone = "+21260000" + new Random().Next(1000, 9999),
                Status = UserStatus.Active,
                Preference = new UserPreference
                {
                    ActiveAppMode = ActiveAppMode.DRIVER,
                    PreferredMode = ActiveAppMode.DRIVER
                }
            };

            var documents = new List<Document>
            {
                new Document { Id = Guid.NewGuid(), DriverDossierId = dossierId, DocumentType = DocumentType.Cin, ObjectKey = $"documents/{driverId}/cin.pdf", Status = GetDocStatus(dossierStatus) },
                new Document { Id = Guid.NewGuid(), DriverDossierId = dossierId, DocumentType = DocumentType.Permit, ObjectKey = $"documents/{driverId}/permit.pdf", Status = GetDocStatus(dossierStatus) },
                new Document { Id = Guid.NewGuid(), DriverDossierId = dossierId, DocumentType = DocumentType.VehicleCard, ObjectKey = $"documents/{driverId}/carte_grise.pdf", Status = GetDocStatus(dossierStatus) },
                new Document { Id = Guid.NewGuid(), DriverDossierId = dossierId, DocumentType = DocumentType.ProfilePhoto, ObjectKey = $"profile-photos/{driverId}/photo.jpg", Status = GetDocStatus(dossierStatus) }
            };

            var dossier = new DriverDossier
            {
                Id = dossierId,
                DriverId = driverId,
                Status = dossierStatus,
                SubmissionDate = DateTime.UtcNow.AddDays(-2),
                VerificationDate = dossierStatus == DriverDossierStatus.Submitted ? null : DateTime.UtcNow.AddDays(-1),
                RejectionReason = dossierStatus == DriverDossierStatus.Rejected ? "Documents are illegible" : null,
                Documents = documents
            };

            var driver = new Driver
            {
                Id = driverId,
                UserId = userId,
                PermitNumber = permitNumber,
                Status = driverStatus,
                Vehicle = new Vehicle
                {
                    Id = Guid.NewGuid(),
                    Type = "Car",
                    Matricule = permitNumber + "-A-1",
                    Marque = "Dacia",
                    Model = "Logan",
                    Country = "Morocco"
                },
                Dossier = dossier
            };

            await context.Users.AddAsync(user, cancellationToken);
            await context.Drivers.AddAsync(driver, cancellationToken);
        }
    }

    private static void SeedDriver(WaselDbContext context, string email, string firstName, string lastName, string permitNumber, DriverStatus driverStatus, DriverDossierStatus dossierStatus)
    {
        var exists = context.Users.Any(u => u.Email == email);
        if (!exists)
        {
            var userId = Guid.NewGuid();
            var driverId = Guid.NewGuid();
            var dossierId = Guid.NewGuid();
            var keycloakId = Guid.NewGuid();

            var user = new User
            {
                Id = userId,
                Email = email,
                KeycloakId = keycloakId.ToString(),
                FirstName = firstName,
                LastName = lastName,
                Cin = "CIN" + permitNumber,
                Phone = "+21260000" + new Random().Next(1000, 9999),
                Status = UserStatus.Active,
                Preference = new UserPreference
                {
                    ActiveAppMode = ActiveAppMode.DRIVER,
                    PreferredMode = ActiveAppMode.DRIVER
                }
            };

            var documents = new List<Document>
            {
                new Document { Id = Guid.NewGuid(), DriverDossierId = dossierId, DocumentType = DocumentType.Cin, ObjectKey = $"documents/{driverId}/cin.pdf", Status = GetDocStatus(dossierStatus) },
                new Document { Id = Guid.NewGuid(), DriverDossierId = dossierId, DocumentType = DocumentType.Permit, ObjectKey = $"documents/{driverId}/permit.pdf", Status = GetDocStatus(dossierStatus) },
                new Document { Id = Guid.NewGuid(), DriverDossierId = dossierId, DocumentType = DocumentType.VehicleCard, ObjectKey = $"documents/{driverId}/carte_grise.pdf", Status = GetDocStatus(dossierStatus) },
                new Document { Id = Guid.NewGuid(), DriverDossierId = dossierId, DocumentType = DocumentType.ProfilePhoto, ObjectKey = $"profile-photos/{driverId}/photo.jpg", Status = GetDocStatus(dossierStatus) }
            };

            var dossier = new DriverDossier
            {
                Id = dossierId,
                DriverId = driverId,
                Status = dossierStatus,
                SubmissionDate = DateTime.UtcNow.AddDays(-2),
                VerificationDate = dossierStatus == DriverDossierStatus.Submitted ? null : DateTime.UtcNow.AddDays(-1),
                RejectionReason = dossierStatus == DriverDossierStatus.Rejected ? "Documents are illegible" : null,
                Documents = documents
            };

            var driver = new Driver
            {
                Id = driverId,
                UserId = userId,
                PermitNumber = permitNumber,
                Status = driverStatus,
                Vehicle = new Vehicle
                {
                    Id = Guid.NewGuid(),
                    Type = "Car",
                    Matricule = permitNumber + "-A-1",
                    Marque = "Dacia",
                    Model = "Logan",
                    Country = "Morocco"
                },
                Dossier = dossier
            };

            context.Users.Add(user);
            context.Drivers.Add(driver);
        }
    }

    private static DocumentStatus GetDocStatus(DriverDossierStatus dossierStatus)
    {
        return dossierStatus switch
        {
            DriverDossierStatus.Approved => DocumentStatus.Approved,
            DriverDossierStatus.Rejected => DocumentStatus.Rejected,
            _ => DocumentStatus.Pending
        };
    }
}