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

    public async Task<CurrentUserResponseDto> GetCurrentUserAsync()
    {
        var keycloakId = _currentUserService.KeycloakId;
        if (string.IsNullOrEmpty(keycloakId))
        {
            throw ApiException.Unauthorized("Invalid token or missing subject claim");
        }

        var dto = new CurrentUserResponseDto
        {
            KeycloakId = keycloakId,
            Email = _currentUserService.Email ?? string.Empty,
            FirstName = _currentUserService.FirstName ?? string.Empty,
            LastName = _currentUserService.LastName ?? string.Empty,
            Roles = _currentUserService.Roles.ToList()
        };

        var localUser = await _userService.GetByKeycloakIdAsync(keycloakId);
        if (localUser != null)
        {
            dto.LocalUserId = localUser.Id;
            dto.Phone = localUser.Phone;
            dto.Cin = localUser.Cin;
            dto.Status = localUser.Status.ToString();
        }

        return dto;
    }

    public async Task<UserResponseDto> SyncCurrentUserAsync()
    {
        var keycloakId = _currentUserService.KeycloakId;
        var email = _currentUserService.Email;
        var firstName = _currentUserService.FirstName;
        var lastName = _currentUserService.LastName;

        if (string.IsNullOrEmpty(keycloakId) || string.IsNullOrEmpty(email))
        {
            throw ApiException.Unauthorized("Invalid token: missing subject or email claim");
        }

        return await _userService.FindOrCreateFromKeycloakAsync(
            keycloakId, 
            email, 
            firstName ?? string.Empty, 
            lastName ?? string.Empty);
    }

    public async Task<CurrentUserResponseDto?> UpdateProfileAsync(UpdateCurrentUserProfileRequestDto request)
    {
        var keycloakId = _currentUserService.KeycloakId;
        if (string.IsNullOrEmpty(keycloakId))
        {
            throw ApiException.Unauthorized("Invalid token or missing subject claim");
        }

        var user = await _userService.UpdateUserProfileAsync(
            keycloakId,
            request.Cin,
            request.Phone,
            request.FirstName,
            request.LastName,
            request.ProfileObjectKey);

        if (user is null)
        {
            return null; // Local user not found
        }

        return new CurrentUserResponseDto
        {
            KeycloakId = user.KeycloakId,
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
