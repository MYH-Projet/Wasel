using Wasel.Api.Modules.Complaints.DTOs;
using Wasel.Api.Modules.Complaints.Entities;
using Wasel.Api.Modules.Complaints.Enums;
using Wasel.Api.Modules.Complaints.Repositories;
using Wasel.Api.Modules.Deliveries.Enums;
using Wasel.Api.Modules.Deliveries.Repositories;
using Wasel.Api.Modules.Users.Repositories;
using Wasel.Api.Modules.Notifications.Enums;
using Wasel.Api.Modules.Notifications.Services;

namespace Wasel.Api.Modules.Complaints.Services;

public class ComplaintService : IComplaintService
{
    private readonly IComplaintRepository _complaintRepository;
    private readonly IDeliveryRepository _deliveryRepository;
    private readonly IUserRepository _userRepository;
    private readonly INotificationService _notificationService;

    public ComplaintService(
        IComplaintRepository complaintRepository,
        IDeliveryRepository deliveryRepository,
        IUserRepository userRepository,
        INotificationService notificationService)
    {
        _complaintRepository = complaintRepository;
        _deliveryRepository = deliveryRepository;
        _userRepository = userRepository;
        _notificationService = notificationService;
    }

    public async Task<object> CreateComplaintAsync(CreateComplaintRequestDto request, string keycloakId)
    {
        var user = await _userRepository.GetByKeycloakIdAsync(keycloakId);

        if (user is null)
            throw new UnauthorizedAccessException("Utilisateur introuvable.");

        var delivery = await _deliveryRepository.GetByIdAsync(request.DeliveryId);

        if (delivery is null)
            throw new InvalidOperationException("Livraison introuvable.");

        if (delivery.ClientId != user.Id)
            throw new UnauthorizedAccessException("Vous n'êtes pas le propriétaire de cette livraison.");

        if (delivery.Status != DeliveryStatus.DELIVERED &&
            delivery.Status != DeliveryStatus.PROBLEM_REPORTED)
        {
            throw new InvalidOperationException("La livraison doit être DELIVERED ou PROBLEM_REPORTED.");
        }

        var exists = await _complaintRepository.ExistsByDeliveryIdAsync(request.DeliveryId);

        if (exists)
            throw new InvalidOperationException("Une réclamation existe déjà pour cette livraison.");

        if (!Enum.TryParse<ComplaintType>(request.ComplaintType, true, out var complaintType))
            throw new InvalidOperationException("Type de réclamation invalide.");

        var complaint = new Complaint
        {
            DeliveryId = request.DeliveryId,
            ComplaintType = complaintType,
            Status = ComplaintStatus.Open,
            Title = request.Title,
            Description = request.Description,
            RequestedAmount = request.RequestedAmount
        };

        await _complaintRepository.AddAsync(complaint);

        // TODO: Notify admin — requires a method to find admin users by role.
        // Once available: await _notificationService.CreateAsync(adminUserId, NotificationType.COMPLAINT_CREATED, "Nouvelle réclamation", ...);

        return new
        {
            complaint.Id,
            complaint.DeliveryId,
            complaint.ComplaintType,
            complaint.Status,
            complaint.Title,
            complaint.Description,
            complaint.RequestedAmount,
            complaint.CreatedAt
        };
    }

    public async Task<object> AddEvidenceAsync(Guid complaintId, AddComplaintEvidenceRequestDto request, string keycloakId)
    {
        var user = await _userRepository.GetByKeycloakIdAsync(keycloakId);

        if (user is null)
            throw new UnauthorizedAccessException("Utilisateur introuvable.");

        var complaint = await _complaintRepository.GetByIdWithDeliveryAsync(complaintId);

        if (complaint is null)
            throw new InvalidOperationException("Réclamation introuvable.");

        if (complaint.Delivery.ClientId != user.Id)
            throw new UnauthorizedAccessException("Vous n'êtes pas le propriétaire de cette réclamation.");

        if (complaint.Status != ComplaintStatus.Open &&
            complaint.Status != ComplaintStatus.UnderReview)
        {
            throw new InvalidOperationException("La réclamation doit être OPEN ou UNDER_REVIEW.");
        }

        var evidenceCount = await _complaintRepository.CountEvidencesAsync(complaintId);

        if (evidenceCount >= 5)
            throw new InvalidOperationException("Maximum 5 preuves par réclamation.");

        var evidence = new ComplaintEvidence
        {
            ComplaintId = complaintId,
            ObjectKey = request.ObjectKey,
            FileType = request.FileType
        };

        await _complaintRepository.AddEvidenceAsync(evidence);

        return new
        {
            evidence.Id,
            evidence.ComplaintId,
            evidence.ObjectKey,
            evidence.FileType,
            evidence.CreatedAt
        };
    }

    public async Task<object> GetMyComplaintsAsync(string keycloakId, string? status, int page, int pageSize)
    {
        var user = await _userRepository.GetByKeycloakIdAsync(keycloakId);

        if (user is null)
            throw new UnauthorizedAccessException("Utilisateur introuvable.");

        ComplaintStatus? parsedStatus = null;

        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!Enum.TryParse<ComplaintStatus>(status, true, out var statusValue))
                throw new InvalidOperationException("Statut de réclamation invalide.");

            parsedStatus = statusValue;
        }

        if (page <= 0)
            page = 1;

        if (pageSize <= 0)
            pageSize = 10;

        var totalItems = await _complaintRepository.CountMyComplaintsAsync(
            user.Id,
            parsedStatus);

        var complaints = await _complaintRepository.GetMyComplaintsAsync(
            user.Id,
            parsedStatus,
            page,
            pageSize);

        return new
        {
            page,
            pageSize,
            totalItems,
            totalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
            items = complaints.Select(c => new ComplaintListItemDto
            {
                Id = c.Id,
                DeliveryId = c.DeliveryId,
                ComplaintType = c.ComplaintType.ToString(),
                Title = c.Title,
                Status = c.Status.ToString(),
                CreatedAt = c.CreatedAt,
                RequestedAmount = c.RequestedAmount
            })
        };
    }
    public async Task<object> GetComplaintDetailsAsync(Guid complaintId, string keycloakId)
    {
        var user = await _userRepository.GetByKeycloakIdAsync(keycloakId);

        if (user is null)
            throw new UnauthorizedAccessException("Utilisateur introuvable.");

        var complaint = await _complaintRepository.GetDetailsByIdAsync(complaintId);

        if (complaint is null)
            throw new InvalidOperationException("Réclamation introuvable.");

        if (complaint.Delivery.ClientId != user.Id)
            throw new UnauthorizedAccessException("Vous n'êtes pas le propriétaire de cette réclamation.");

        return new ComplaintDetailsDto
        {
            Id = complaint.Id,
            DeliveryId = complaint.DeliveryId,
            ComplaintType = complaint.ComplaintType.ToString(),
            Status = complaint.Status.ToString(),
            Title = complaint.Title,
            Description = complaint.Description,
            RequestedAmount = complaint.RequestedAmount,
            ApprovedAmount = complaint.ApprovedAmount,
            AdminComment = complaint.AdminComment,
            ResolvedAt = complaint.ResolvedAt,
            CreatedAt = complaint.CreatedAt,
            Evidences = complaint.Evidences.Select(e => new ComplaintEvidenceDto
            {
                Id = e.Id,
                ObjectKey = e.ObjectKey,
                FileType = e.FileType,
                CreatedAt = e.CreatedAt
            }).ToList()
        };
    }

    public async Task<object> GetAdminComplaintsAsync(
    string? status,
    string? type,
    DateTime? fromDate,
    DateTime? toDate,
    int page,
    int pageSize)
    {
        ComplaintStatus? parsedStatus = null;
        ComplaintType? parsedType = null;

        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!Enum.TryParse<ComplaintStatus>(status, true, out var statusValue))
                throw new InvalidOperationException("Statut de réclamation invalide.");

            parsedStatus = statusValue;
        }

        if (!string.IsNullOrWhiteSpace(type))
        {
            if (!Enum.TryParse<ComplaintType>(type, true, out var typeValue))
                throw new InvalidOperationException("Type de réclamation invalide.");

            parsedType = typeValue;
        }

        if (page <= 0)
            page = 1;

        if (pageSize <= 0)
            pageSize = 10;

        var totalItems = await _complaintRepository.CountAdminComplaintsAsync(
            parsedStatus,
            parsedType,
            fromDate,
            toDate);

        var complaints = await _complaintRepository.GetAdminComplaintsAsync(
            parsedStatus,
            parsedType,
            fromDate,
            toDate,
            page,
            pageSize);

        return new
        {
            page,
            pageSize,
            totalItems,
            totalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
            items = complaints.Select(c => new
            {
                c.Id,
                c.DeliveryId,
                ComplaintType = c.ComplaintType.ToString(),
                Status = c.Status.ToString(),
                c.Title,
                c.RequestedAmount,
                c.CreatedAt
            })
        };
    }

    public async Task<object> GetAdminComplaintDetailsAsync(Guid complaintId)
    {
        var complaint = await _complaintRepository.GetAdminDetailsByIdAsync(complaintId);

        if (complaint is null)
            throw new InvalidOperationException("Réclamation introuvable.");

        return new
        {
            complaint.Id,
            complaint.DeliveryId,
            ComplaintType = complaint.ComplaintType.ToString(),
            Status = complaint.Status.ToString(),
            complaint.Title,
            complaint.Description,
            complaint.RequestedAmount,
            complaint.ApprovedAmount,
            complaint.AdminComment,
            complaint.ResolvedAt,
            complaint.CreatedAt,
            Evidences = complaint.Evidences.Select(e => new
            {
                e.Id,
                e.ObjectKey,
                e.FileType,
                e.CreatedAt
            })
        };
    }

    public async Task<object> UpdateComplaintStatusAsync(Guid complaintId, UpdateComplaintStatusRequestDto request)
    {
        var complaint = await _complaintRepository.GetAdminDetailsByIdAsync(complaintId);

        if (complaint is null)
            throw new InvalidOperationException("Réclamation introuvable.");

        if (!Enum.TryParse<ComplaintStatus>(request.Status, true, out var newStatus))
            throw new InvalidOperationException("Statut de réclamation invalide.");

        if (newStatus != ComplaintStatus.UnderReview &&
            newStatus != ComplaintStatus.ClientWalletReview &&
            newStatus != ComplaintStatus.DriverReview)
        {
            throw new InvalidOperationException("Le statut autorisé est UNDER_REVIEW, CLIENT_WALLET_REVIEW ou DRIVER_REVIEW.");
        }

        complaint.Status = newStatus;
        complaint.UpdatedAt = DateTime.UtcNow;

        await _complaintRepository.SaveChangesAsync();

        return new
        {
            complaint.Id,
            Status = complaint.Status.ToString(),
            complaint.UpdatedAt
        };
    }

    public async Task<object> ResolveComplaintAsync(Guid complaintId, ResolveComplaintRequestDto request)
    {
        var complaint = await _complaintRepository.GetAdminDetailsByIdAsync(complaintId);

        if (complaint is null)
            throw new InvalidOperationException("Réclamation introuvable.");

        if (!Enum.TryParse<ResolutionType>(request.ResolutionType, true, out var resolutionType))
            throw new InvalidOperationException("Type de résolution invalide.");

        if ((resolutionType == ResolutionType.ClientWalletCredit ||
            resolutionType == ResolutionType.DriverPenalty ||
            resolutionType == ResolutionType.ManualAdjustment)
            && (!request.ApprovedAmount.HasValue || request.ApprovedAmount <= 0))
        {
            throw new InvalidOperationException("approvedAmount est obligatoire et doit être supérieur à 0.");
        }

        complaint.Status = ComplaintStatus.Resolved;
        complaint.AdminComment = request.AdminComment;
        complaint.ApprovedAmount = request.ApprovedAmount;
        complaint.ResolvedAt = DateTime.UtcNow;
        complaint.UpdatedAt = DateTime.UtcNow;

        // TODO Wallet:
        // - ClientWalletCredit : créer WalletTransaction CREDIT vers ClientWallet + WalletTransfer CLIENT_REFUND
        // - DriverPenalty : créer WalletTransaction DEBIT sur DriverWallet
        // - ManualAdjustment : ajustement libre selon votre modèle Wallet

        await _complaintRepository.SaveChangesAsync();

        // Notify the client who owns the delivery that complaint is resolved
        try
        {
            var delivery = await _deliveryRepository.GetByIdAsync(complaint.DeliveryId);
            if (delivery != null)
            {
                await _notificationService.CreateAsync(
                    delivery.ClientId,
                    NotificationType.COMPLAINT_RESOLVED,
                    "Réclamation résolue",
                    $"Votre réclamation \"{complaint.Title}\" a été résolue.");
            }
        }
        catch { /* notification failure must not affect complaint resolution */ }

        return new
        {
            complaint.Id,
            Status = complaint.Status.ToString(),
            ResolutionType = resolutionType.ToString(),
            complaint.AdminComment,
            complaint.ApprovedAmount,
            complaint.ResolvedAt
        };
    }
}