using Wasel.Api.Modules.Users.DTOs;

namespace Wasel.Api.Modules.Users.Services;

public interface IUserService
{
    Task<List<UserResponseDto>> GetAllUsersAsync();
    Task<UserResponseDto?> GetUserByIdAsync(Guid id);
    Task<UserResponseDto?> GetByKeycloakIdAsync(string keycloakId);
    Task<UserResponseDto> FindOrCreateFromKeycloakAsync(string keycloakId, string email, string firstName, string lastName);
    Task<UserResponseDto?> ChangeUserStatusAsync(Guid id, ChangeUserStatusRequestDto request);
    Task<UserResponseDto?> UpdateUserProfileAsync(string keycloakId, string? cin, string? phone, string? firstName, string? lastName, string? profileObjectKey);
}