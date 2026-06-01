using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wasel.Api.Modules.Auth.Services;
using Wasel.Api.Modules.Wallets.DTOs;
using Wasel.Api.Modules.Wallets.Services;
using Wasel.Api.Shared.Exceptions;
using Wasel.Api.Modules.Users.Enums;
using Wasel.Api.Modules.Users.Repositories;
using Wasel.Api.Modules.Drivers.Repositories;

namespace Wasel.Api.Modules.Wallets.Controllers;

[ApiController]
[Route("api/wallet")]
[Authorize(Policy = "ActiveUserOnly")]
public class WalletsController : ControllerBase
{
    private readonly IWalletService _walletService;
    private readonly IAuthService _authService;
    private readonly IUserRepository _userRepository;
    private readonly IDriverRepository _driverRepository;

    public WalletsController(IWalletService walletService, IAuthService authService, IUserRepository userRepository, IDriverRepository driverRepository)
    {
        _walletService = walletService;
        _authService = authService;
        _userRepository = userRepository;
        _driverRepository = driverRepository;
    }

    private async Task EnsureAppModeAsync(Guid userId, ActiveAppMode expectedMode)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        var preference = user?.Preference;
        if (preference == null || preference.ActiveAppMode != expectedMode)
        {
            throw ApiException.Forbidden($"Accès refusé. Mode {expectedMode} requis.");
        }
    }

    [HttpGet("me")]
    public async Task<ActionResult<WalletBalanceResponseDto>> GetMyClientWallet()
    {
        var currentUser = await _authService.EnsureCurrentUserExistsAsync();
        await EnsureAppModeAsync(currentUser.LocalUserId!.Value, ActiveAppMode.CLIENT);

        var balance = await _walletService.GetClientWalletBalanceAsync(currentUser.LocalUserId!.Value);
        return Ok(balance);
    }

    [HttpGet("me/transactions")]
    public async Task<ActionResult> GetMyClientTransactions([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 10;

        var currentUser = await _authService.EnsureCurrentUserExistsAsync();
        await EnsureAppModeAsync(currentUser.LocalUserId!.Value, ActiveAppMode.CLIENT);

        var (items, totalCount) = await _walletService.GetClientTransactionsAsync(currentUser.LocalUserId!.Value, page, pageSize);

        return Ok(new { items, totalCount, page, pageSize });
    }

    [HttpGet("driver/me")]
    [Authorize(Policy = "DriverOnly")]
    public async Task<ActionResult<WalletBalanceResponseDto>> GetMyDriverWallet()
    {
        var currentUser = await _authService.EnsureCurrentUserExistsAsync();
        await EnsureAppModeAsync(currentUser.LocalUserId!.Value, ActiveAppMode.DRIVER);

        var driver = await _driverRepository.GetByUserIdAsync(currentUser.LocalUserId!.Value);
        if (driver == null)
            throw ApiException.Forbidden("Utilisateur non reconnu comme livreur.");

        var balance = await _walletService.GetDriverWalletBalanceAsync(driver.Id);
        return Ok(balance);
    }

    [HttpGet("driver/me/transactions")]
    [Authorize(Policy = "DriverOnly")]
    public async Task<ActionResult> GetMyDriverTransactions([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 10;

        var currentUser = await _authService.EnsureCurrentUserExistsAsync();
        await EnsureAppModeAsync(currentUser.LocalUserId!.Value, ActiveAppMode.DRIVER);

        var driver = await _driverRepository.GetByUserIdAsync(currentUser.LocalUserId!.Value);
        if (driver == null)
            throw ApiException.Forbidden("Utilisateur non reconnu comme livreur.");

        var (items, totalCount) = await _walletService.GetDriverTransactionsAsync(driver.Id, page, pageSize);

        return Ok(new { items, totalCount, page, pageSize });
    }

    [HttpPost("driver/withdraw")]
    [Authorize(Policy = "DriverOnly")]
    public async Task<ActionResult> WithdrawDriverFunds([FromBody] WithdrawRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var currentUser = await _authService.EnsureCurrentUserExistsAsync();
        await EnsureAppModeAsync(currentUser.LocalUserId!.Value, ActiveAppMode.DRIVER);

        var driver = await _driverRepository.GetByUserIdAsync(currentUser.LocalUserId!.Value);
        if (driver == null)
            throw ApiException.Forbidden("Utilisateur non reconnu comme livreur.");

        await _walletService.WithdrawDriverFundsAsync(driver.Id, request);

        return Ok(new { message = "Demande de retrait enregistrée avec succès." });
    }
}
