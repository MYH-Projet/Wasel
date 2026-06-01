using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wasel.Api.Modules.Auth.Services;
using Wasel.Api.Modules.Payments.DTOs;
using Wasel.Api.Modules.Payments.Services;

namespace Wasel.Api.Modules.Payments.Controllers;

[ApiController]
[Route("api/payment-methods")]
[Authorize(Policy = "ClientOnly")]
public class PaymentMethodsController : ControllerBase
{
    private readonly IPaymentMethodService _paymentMethodService;
    private readonly IAuthService _authService;

    public PaymentMethodsController(IPaymentMethodService paymentMethodService, IAuthService authService)
    {
        _paymentMethodService = paymentMethodService;
        _authService = authService;
    }

    [HttpPost]
    public async Task<ActionResult<PaymentMethodResponseDto>> Create([FromBody] CreatePaymentMethodRequestDto request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var currentUser = await _authService.EnsureCurrentUserExistsAsync();
        var response = await _paymentMethodService.CreateAsync(request, currentUser.LocalUserId!.Value);
        return Ok(response);
    }

    [HttpGet("my")]
    public async Task<ActionResult<IEnumerable<PaymentMethodResponseDto>>> GetMy()
    {
        var currentUser = await _authService.EnsureCurrentUserExistsAsync();
        var response = await _paymentMethodService.GetMyPaymentMethodsAsync(currentUser.LocalUserId!.Value);
        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var currentUser = await _authService.EnsureCurrentUserExistsAsync();
        await _paymentMethodService.DeleteAsync(id, currentUser.LocalUserId!.Value);
        return NoContent();
    }

    [HttpPatch("{id:guid}/default")]
    public async Task<ActionResult> SetDefault(Guid id)
    {
        var currentUser = await _authService.EnsureCurrentUserExistsAsync();
        await _paymentMethodService.SetDefaultAsync(id, currentUser.LocalUserId!.Value);
        return NoContent();
    }
}
