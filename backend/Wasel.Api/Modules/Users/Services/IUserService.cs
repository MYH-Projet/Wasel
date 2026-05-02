using Wasel.Api.Modules.Users.DTOs;

namespace Wasel.Api.Modules.Users.Services;

public interface IUserService
{
    Task<List<UserResponseDto>> GetAllUsersAsync();
    Task<UserResponseDto?> GetUserByIdAsync(Guid id);
    Task<UserResponseDto?> ChangeUserStatusAsync(Guid id, ChangeUserStatusRequestDto request);
}