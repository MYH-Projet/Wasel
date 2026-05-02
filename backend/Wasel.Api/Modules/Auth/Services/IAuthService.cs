using Wasel.Api.Modules.Auth.DTOs;
using Wasel.Api.Modules.Users.DTOs;

namespace Wasel.Api.Modules.Auth.Services;

public interface IAuthService
{
    Task<CurrentUserResponseDto> GetCurrentUserAsync();
    Task<UserResponseDto> SyncCurrentUserAsync();
    Task<CurrentUserResponseDto?> UpdateProfileAsync(UpdateCurrentUserProfileRequestDto request);
}
