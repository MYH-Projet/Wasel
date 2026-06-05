using Wasel.Api.Modules.Users.DTOs;

namespace Wasel.Api.Modules.Users.Services;

public interface IUserService
{
    Task<List<UserResponseDto>> GetAllUsersAsync();
    Task<UserResponseDto?> GetUserByIdAsync(Guid id);
    Task<UserResponseDto?> GetByKeycloakIdAsync(string keycloakId);
    Task<UserResponseDto> FindOrCreateFromKeycloakAsync(string keycloakId, string email, string firstName, string lastName, List<string> roles);
    Task<UserResponseDto?> ChangeUserStatusAsync(Guid id, ChangeUserStatusRequestDto request);
    Task<UserResponseDto?> UpdateUserProfileAsync(string keycloakId, string? cin, string? phone, string? firstName, string? lastName, string? profileObjectKey);
    Task<UserResponseDto> UpdateMyProfileAsync(string keycloakId, UpdateMyProfileRequestDto request);
    Task<UserPreferencesResponseDto> UpdateMyPreferencesAsync(string keycloakId, UpdateUserPreferencesRequestDto request);
    Task<PaginatedResponseDto<UserResponseDto>> GetAdminUsersAsync(
    int page,
    int pageSize,
    string? search,
    string? role,
    string? status,
    DateTime? startDate,
    DateTime? endDate);
}