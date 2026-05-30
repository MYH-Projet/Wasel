using Wasel.Api.Modules.Deliveries.Enums;
using Wasel.Api.Modules.Deliveries.Repositories;
using Wasel.Api.Modules.Payments.DTOs;
using Wasel.Api.Modules.Payments.Entities;
using Wasel.Api.Modules.Payments.Enums;
using Wasel.Api.Modules.Payments.Repositories;
using Wasel.Api.Modules.Wallets.Services;
using Wasel.Api.Modules.Tracking.Repositories;
using Wasel.Api.Shared.Database;
using Wasel.Api.Shared.Exceptions;
using Wasel.Api.Infrastructure.Keycloak;
using Wasel.Api.Modules.Drivers.Repositories;
using Wasel.Api.Modules.Wallets.Entities;
using Wasel.Api.Modules.Wallets.Repositories;

namespace Wasel.Api.Modules.Payments.Services;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IDeliveryRepository _deliveryRepository;
    private readonly IDriverRepository _driverRepository;
    private readonly IWalletService _walletService;
    private readonly IPaymentGateway _paymentGateway;
    private readonly WaselDbContext _context;
    private readonly IWalletRepository _walletRepository;

    public PaymentService(
        IPaymentRepository paymentRepository,
        IDeliveryRepository deliveryRepository,
        IDriverRepository driverRepository,
        IWalletService walletService,
        IPaymentGateway paymentGateway,
        WaselDbContext context,
        IWalletRepository walletRepository)
    {
        _paymentRepository = paymentRepository;
        _deliveryRepository = deliveryRepository;
        _driverRepository = driverRepository;
        _walletService = walletService;
        _paymentGateway = paymentGateway;
        _context = context;
        _walletRepository = walletRepository;
    }

    public async Task<InitiatePaymentResponseDto> InitiatePaymentAsync(InitiatePaymentRequestDto request, Guid currentUserId)
    {
        var delivery = await _deliveryRepository.GetByIdAsync(request.DeliveryId);
        if (delivery == null)
            throw ApiException.NotFound("Livraison introuvable.");

        if (delivery.ClientId != currentUserId)
            throw ApiException.Forbidden("Seul le propriétaire de la livraison peut initier le paiement.");

        if (!Enum.TryParse<PaymentMethod>(request.PaymentMethod, true, out var method))
            throw ApiException.BadRequest("Méthode de paiement invalide.");

        var existingPayment = await _paymentRepository.GetPaymentByDeliveryIdAsync(request.DeliveryId);
        if (existingPayment != null && existingPayment.Status != PaymentStatus.FAILED && existingPayment.Status != PaymentStatus.CANCELLED)
        {
            throw ApiException.BadRequest("Un paiement actif existe déjà pour cette livraison.");
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var payment = new Payment
            {
                DeliveryId = delivery.Id,
                Amount = delivery.Price,
                Currency = delivery.Currency ?? "MAD",
                Method = method,
                Status = PaymentStatus.PENDING,
                TransactionReference = Guid.NewGuid().ToString("N")
            };

            await _paymentRepository.CreatePaymentAsync(payment);

            string? paymentUrl = null;

            if (method == PaymentMethod.CASH)
            {
                // Nothing more to do
            }
            else if (method == PaymentMethod.CARD)
            {
                paymentUrl = await _paymentGateway.GeneratePaymentUrlAsync(payment.Id, payment.Amount, payment.Currency);
            }
            else if (method == PaymentMethod.WALLET)
            {
                if (delivery.DriverId == null)
                    throw ApiException.BadRequest("Impossible de payer par wallet si aucun livreur n'est assigné.");

                var driver = await _driverRepository.GetByIdAsync(delivery.DriverId.Value);
                if (driver == null)
                    throw ApiException.BadRequest("Livreur introuvable.");

                Console.WriteLine($"[DEBUG] PaymentService avant ProcessDeliveryPaymentAsync - DeliveryId: {delivery.Id}, ClientId: {delivery.ClientId}, DriverId: {driver.Id}");
                await _walletService.ProcessDeliveryPaymentAsync(delivery.ClientId, driver.Id, payment.Amount, payment.Currency, delivery.Id);
                
                payment.Status = PaymentStatus.PAID;
                await _paymentRepository.UpdatePaymentAsync(payment);
            }

            await transaction.CommitAsync();

            return new InitiatePaymentResponseDto
            {
                PaymentId = payment.Id,
                Status = payment.Status,
                PaymentUrl = paymentUrl
            };
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<ConfirmCashPaymentResponseDto> ConfirmCashPaymentAsync(Guid paymentId, Guid currentUserId)
    {
        var driver = await _driverRepository.GetByUserIdAsync(currentUserId);
        if (driver == null)
            throw ApiException.Forbidden("Utilisateur non reconnu comme livreur.");

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var payment = await _paymentRepository.GetPaymentByIdAsync(paymentId);
            if (payment == null)
                throw ApiException.NotFound("Paiement introuvable.");

            var delivery = await _deliveryRepository.GetByIdAsync(payment.DeliveryId);
            if (delivery == null)
                throw ApiException.NotFound("Livraison introuvable.");

            if (delivery.DriverId != driver.Id)
                throw ApiException.Forbidden("Seul le livreur assigné peut confirmer l'encaissement cash.");

            if (payment.Method != PaymentMethod.CASH)
                throw ApiException.BadRequest("Ce paiement n'est pas de type CASH.");

            if (payment.Status != PaymentStatus.PENDING)
                throw ApiException.BadRequest($"Impossible de confirmer un paiement au statut {payment.Status}.");

            payment.Status = PaymentStatus.PAID;
            await _paymentRepository.UpdatePaymentAsync(payment);

            var driverWallet = await _walletService.EnsureDriverWalletExistsAsync(driver.Id);
            
            // Note: For CASH, the driver physically collected the money.
            // We record this in their wallet for tracking, even though their virtual balance might not increase.
            // Or maybe it does increase? Wait, if they collected cash, they owe the platform a commission, or they just keep it.
            // The requirement says: "Créer un WalletTransaction pour le driver: direction=CREDIT, reason=CASH_COLLECTION".
            // Since it's a credit, it implies their balance should increase, or it's just a log. Let's strictly increase balance and log it,
            // or just add the transaction without modifying balance? The requirement didn't explicitly say "update balance" for confirm cash, only "créer un WalletTransaction".
            // But a CREDIT transaction usually increases balance. Let's increase it to be consistent with WALLET rules, OR just record it.
            // I will increase the balance as it's standard double-entry bookkeeping.
            
            driverWallet.Balance += payment.Amount;
            await _walletRepository.UpdateWalletAsync(driverWallet);

            var walletTx = new WalletTransaction
            {
                WalletId = driverWallet.Id,
                Amount = payment.Amount,
                Currency = payment.Currency,
                Direction = Wasel.Api.Modules.Wallets.Enums.WalletTransactionDirection.CREDIT,
                Reason = Wasel.Api.Modules.Wallets.Enums.WalletTransactionReason.CASH_COLLECTION,
                Description = "Encaissement en espèces",
                DeliveryId = delivery.Id
            };
            await _walletRepository.AddTransactionAsync(walletTx);

            await transaction.CommitAsync();

            return new ConfirmCashPaymentResponseDto { Success = true };
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<PaymentDetailsResponseDto> GetPaymentDetailsAsync(Guid deliveryId, Guid currentUserId, IEnumerable<string> currentUserRoles)
    {
        var delivery = await _deliveryRepository.GetByIdAsync(deliveryId);
        if (delivery == null)
            throw ApiException.NotFound("Livraison introuvable.");

        bool isAuthorized = false;

        if (currentUserRoles.Contains(KeycloakConstants.RoleAdmin))
        {
            isAuthorized = true;
        }
        else if (currentUserRoles.Contains(KeycloakConstants.RoleClient) && delivery.ClientId == currentUserId)
        {
            isAuthorized = true;
        }
        else if (currentUserRoles.Contains(KeycloakConstants.RoleDriver))
        {
            var driver = await _driverRepository.GetByUserIdAsync(currentUserId);
            if (driver != null && delivery.DriverId == driver.Id)
            {
                isAuthorized = true;
            }
        }

        if (!isAuthorized)
            throw ApiException.Forbidden("Non autorisé à consulter ce paiement.");

        var payment = await _paymentRepository.GetPaymentByDeliveryIdAsync(deliveryId);
        if (payment == null)
            throw ApiException.NotFound("Aucun paiement trouvé pour cette livraison.");

        return new PaymentDetailsResponseDto
        {
            PaymentId = payment.Id,
            DeliveryId = payment.DeliveryId,
            Amount = payment.Amount,
            Currency = payment.Currency,
            Method = payment.Method.ToString(),
            Status = payment.Status,
            TransactionReference = payment.TransactionReference,
            CreatedAt = payment.CreatedAt
        };
    }
}

public class ConfirmCashPaymentResponseDto
{
    public bool Success { get; set; }
}
