using Wasel.Api.Modules.Auth.DTOs;
using Wasel.Api.Modules.Users.DTOs;
using Wasel.Api.Modules.Users.Services;
using Wasel.Api.Shared.Exceptions;
using Wasel.Api.Shared.Security;

namespace Wasel.Api.Modules.Auth.Services;

public class AuthService : IAuthService
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserService _userService;

    public AuthService(ICurrentUserService currentUserService, IUserService userService)
    {
        _currentUserService = currentUserService;
        _userService = userService;
    }

    /// <summary>
    /// Ensures that a local user profile exists in PostgreSQL for the current JWT identity.
    /// If the user does not exist locally, it is created via FindOrCreateFromKeycloakAsync.
    /// This is the central auto-sync method — all endpoints needing the current user should call this.
    /// </summary>
    public async Task<CurrentUserResponseDto> EnsureCurrentUserExistsAsync()
    {
        var keycloakId = _currentUserService.KeycloakId;
        var email = _currentUserService.Email;
        var firstName = _currentUserService.FirstName;
        var lastName = _currentUserService.LastName;
        var roles = _currentUserService.Roles.ToList();

        if (string.IsNullOrEmpty(keycloakId) || string.IsNullOrEmpty(email))
        {
            throw ApiException.Unauthorized("Invalid token: missing subject or email claim");
        }

        // FindOrCreateFromKeycloakAsync handles: lookup by KeycloakId → fallback by Email → create
        // DB exceptions (DbException, DbUpdateException, timeout) propagate naturally — no catch here.
        var localUser = await _userService.FindOrCreateFromKeycloakAsync(
            keycloakId,
            email,
            firstName ?? string.Empty,
            lastName ?? string.Empty, roles);

        return new CurrentUserResponseDto
        {
            KeycloakId = keycloakId,
            Email = email,
            FirstName = firstName ?? string.Empty,
            LastName = lastName ?? string.Empty,
            Roles = _currentUserService.Roles.ToList(),
            LocalUserId = localUser.Id,
            Phone = localUser.Phone,
            Cin = localUser.Cin,
            Status = localUser.Status.ToString()
        };
    }

    public async Task<CurrentUserResponseDto> GetCurrentUserAsync()
    {
        // Auto-sync: the local user is created if it doesn't exist yet
        return await EnsureCurrentUserExistsAsync();
    }

    public async Task<UserResponseDto> SyncCurrentUserAsync()
    {
        var keycloakId = _currentUserService.KeycloakId;
        var email = _currentUserService.Email;
        var firstName = _currentUserService.FirstName;
        var lastName = _currentUserService.LastName;
        var roles = _currentUserService.Roles.ToList();

        if (string.IsNullOrEmpty(keycloakId) || string.IsNullOrEmpty(email))
        {
            throw ApiException.Unauthorized("Invalid token: missing subject or email claim");
        }

        return await _userService.FindOrCreateFromKeycloakAsync(
            keycloakId, 
            email, 
            firstName ?? string.Empty, 
            lastName ?? string.Empty, roles);
    }

    public async Task<CurrentUserResponseDto> UpdateProfileAsync(UpdateCurrentUserProfileRequestDto request)
    {
        // Auto-sync: guarantee the local user exists before attempting profile update
        var currentUser = await EnsureCurrentUserExistsAsync();

        var user = await _userService.UpdateUserProfileAsync(
            currentUser.KeycloakId,
            request.Cin,
            request.Phone,
            request.FirstName,
            request.LastName,
            request.ProfileObjectKey);

        // user is guaranteed non-null because EnsureCurrentUserExistsAsync already created it
        return new CurrentUserResponseDto
        {
            KeycloakId = user!.KeycloakId,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Cin = user.Cin,
            Phone = user.Phone,
            Status = user.Status.ToString(),
            Roles = _currentUserService.Roles.ToList(),
            LocalUserId = user.Id
        };
    }
}
