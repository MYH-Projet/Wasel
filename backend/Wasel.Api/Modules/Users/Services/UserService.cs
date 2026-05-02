using Wasel.Api.Modules.Users.DTOs;
using Wasel.Api.Modules.Users.Entities;
using Wasel.Api.Modules.Users.Repositories;

namespace Wasel.Api.Modules.Users.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<List<UserResponseDto>> GetAllUsersAsync()
    {
        var users = await _userRepository.GetAllAsync();

        return users.Select(MapToResponseDto).ToList();
        //comme return users.Select(user => MapToResponseDto(user)).ToList();
    }

    public async Task<UserResponseDto?> GetUserByIdAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);

        if (user is null)
        {
            return null;
        }

        return MapToResponseDto(user);
    }

    public async Task<UserResponseDto?> ChangeUserStatusAsync(Guid id, ChangeUserStatusRequestDto request)
    {
        var user = await _userRepository.GetByIdAsync(id);

        if (user is null)
        {
            return null;
        }

        user.Status = request.Status;

        await _userRepository.UpdateAsync(user);

        return MapToResponseDto(user);
    }


    private static UserResponseDto MapToResponseDto(User user)
    {
        return new UserResponseDto
        {
            Id = user.Id,
            KeycloakId = user.KeycloakId,
            Cin = user.Cin,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Phone = user.Phone,
            Status = user.Status,
            ProfileObjectKey = user.ProfileObjectKey,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}
