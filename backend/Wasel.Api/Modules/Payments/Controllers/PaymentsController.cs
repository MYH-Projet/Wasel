using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wasel.Api.Modules.Auth.Services;
using Wasel.Api.Modules.Payments.DTOs;
using Wasel.Api.Modules.Payments.Services;
using Wasel.Api.Shared.Exceptions;

namespace Wasel.Api.Modules.Payments.Controllers;

[ApiController]
[Route("api/payments")]
[Authorize(Policy = "ActiveUserOnly")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly IAuthService _authService;

    public PaymentsController(IPaymentService paymentService, IAuthService authService)
    {
        _paymentService = paymentService;
        _authService = authService;
    }

    [HttpPost("initiate")]
    public async Task<ActionResult<InitiatePaymentResponseDto>> InitiatePayment([FromBody] InitiatePaymentRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var currentUser = await _authService.EnsureCurrentUserExistsAsync();
        
        var response = await _paymentService.InitiatePaymentAsync(request, currentUser.LocalUserId!.Value);
        return Ok(response);
    }

    [HttpPost("{id:guid}/confirm-cash")]
    [Authorize(Policy = "DriverOnly")]
    public async Task<ActionResult<ConfirmCashPaymentResponseDto>> ConfirmCashPayment(Guid id)
    {
        var currentUser = await _authService.EnsureCurrentUserExistsAsync();
        var response = await _paymentService.ConfirmCashPaymentAsync(id, currentUser.LocalUserId!.Value);
        return Ok(response);
    }

    [HttpGet("{deliveryId:guid}")]
    public async Task<ActionResult<PaymentDetailsResponseDto>> GetPaymentDetails(Guid deliveryId)
    {
        var currentUser = await _authService.EnsureCurrentUserExistsAsync();
        var response = await _paymentService.GetPaymentDetailsAsync(deliveryId, currentUser.LocalUserId!.Value, currentUser.Roles);
        return Ok(response);
    }
}
