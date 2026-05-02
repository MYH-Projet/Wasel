using Wasel.Api.Modules.Users.DTOs;
using Wasel.Api.Modules.Users.Entities;
using Wasel.Api.Modules.Users.Enums;
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

    public async Task<UserResponseDto?> GetByKeycloakIdAsync(string keycloakId)
    {
        var user = await _userRepository.GetByKeycloakIdAsync(keycloakId);

        if (user is null)
        {
            return null;
        }

        return MapToResponseDto(user);
    }

    public async Task<UserResponseDto> FindOrCreateFromKeycloakAsync(string keycloakId, string email, string firstName, string lastName)
    {
        var user = await _userRepository.GetByKeycloakIdAsync(keycloakId);

        if (user is null)
        {
            // Check if a user with this email already exists (fallback)
            user = await _userRepository.GetByEmailAsync(email);

            if (user is not null)
            {
                // Update existing user with KeycloakId
                user.KeycloakId = keycloakId;
                user.FirstName = firstName;
                user.LastName = lastName;
                await _userRepository.UpdateAsync(user);
            }
            else
            {
                // Create new user
                user = new User
                {
                    KeycloakId = keycloakId,
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    Status = UserStatus.Pending // Require manual approval for drivers, clients might be active by default depending on logic
                };
                await _userRepository.AddAsync(user);
            }
        }
        else
        {
            // Update details if they changed in Keycloak
            bool updated = false;
            if (user.Email != email) { user.Email = email; updated = true; }
            if (user.FirstName != firstName) { user.FirstName = firstName; updated = true; }
            if (user.LastName != lastName) { user.LastName = lastName; updated = true; }

            if (updated)
            {
                await _userRepository.UpdateAsync(user);
            }
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


    public async Task<UserResponseDto?> UpdateUserProfileAsync(string keycloakId, string? cin, string? phone, string? firstName, string? lastName, string? profileObjectKey)
    {
        var user = await _userRepository.GetByKeycloakIdAsync(keycloakId);

        if (user is null)
        {
            return null; // Return null to let the caller handle it (e.g., 404 Not Found)
        }

        bool updated = false;

        if (cin is not null && user.Cin != cin) { user.Cin = cin; updated = true; }
        if (phone is not null && user.Phone != phone) { user.Phone = phone; updated = true; }
        if (firstName is not null && user.FirstName != firstName) { user.FirstName = firstName; updated = true; }
        if (lastName is not null && user.LastName != lastName) { user.LastName = lastName; updated = true; }
        if (profileObjectKey is not null && user.ProfileObjectKey != profileObjectKey) { user.ProfileObjectKey = profileObjectKey; updated = true; }

        if (updated)
        {
            await _userRepository.UpdateAsync(user);
        }

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
