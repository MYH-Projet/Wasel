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

    public async Task<UserResponseDto> FindOrCreateFromKeycloakAsync(
    string keycloakId,
    string email,
    string firstName,
    string lastName)
    {
        var user = await _userRepository.GetByKeycloakIdAsync(keycloakId);

        if (user is null)
        {
            user = await _userRepository.GetByEmailAsync(email);

            if (user is not null)
            {
                user.KeycloakId = keycloakId;
                user.FirstName = firstName;
                user.LastName = lastName;

                if (user.Preference is null)
                {
                    user.Preference = new UserPreference
                    {
                        Id = Guid.Empty,
                        ActiveAppMode = ActiveAppMode.CLIENT
                    };
                }

                await _userRepository.UpdateAsync(user);
            }
            else
            {
                user = new User
                {
                    KeycloakId = keycloakId,
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    Status = email == "admin@wasel.ma" ? UserStatus.Active : UserStatus.Pending,
                    Preference = new UserPreference
                    {
                        ActiveAppMode = ActiveAppMode.CLIENT
                    }
                };

                await _userRepository.AddAsync(user);
            }
        }
        else
        {
            bool updated = false;

            if (user.Email != email)
            {
                user.Email = email;
                updated = true;
            }

            if (user.FirstName != firstName)
            {
                user.FirstName = firstName;
                updated = true;
            }

            if (user.LastName != lastName)
            {
                user.LastName = lastName;
                updated = true;
            }

            if (user.Preference is null)
            {
                user.Preference = new UserPreference
                {
                    Id = Guid.Empty,
                    ActiveAppMode = ActiveAppMode.CLIENT
                };

                updated = true;
            }

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
            return null; 
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

    public async Task<UserResponseDto> UpdateMyProfileAsync(string keycloakId, UpdateMyProfileRequestDto request)
    {
        var user = await _userRepository.GetByKeycloakIdAsync(keycloakId);

        if (user is null)
        {
            throw new Shared.Exceptions.ApiException("User not found locally", 404);
        }

        bool updated = false;

        if (request.FirstName is not null && user.FirstName != request.FirstName) { user.FirstName = request.FirstName; updated = true; }
        if (request.LastName is not null && user.LastName != request.LastName) { user.LastName = request.LastName; updated = true; }
        if (request.Phone is not null && user.Phone != request.Phone) { user.Phone = request.Phone; updated = true; }
        if (request.ProfileObjectKey is not null && user.ProfileObjectKey != request.ProfileObjectKey) { user.ProfileObjectKey = request.ProfileObjectKey; updated = true; }

        if (updated)
        {
            await _userRepository.UpdateAsync(user);
        }

        return MapToResponseDto(user);
    }

    public async Task<UserPreferencesResponseDto> UpdateMyPreferencesAsync(string keycloakId, UpdateUserPreferencesRequestDto request)
    {
        var user = await _userRepository.GetUserWithPreferenceAndDriverAsync(keycloakId);

        if (user is null)
        {
            throw new Shared.Exceptions.ApiException("User not found locally", 404);
        }

        if (request.ActiveAppMode == ActiveAppMode.DRIVER && user.Driver is null)
        {
            throw new Shared.Exceptions.ApiException("Cannot set active mode to DRIVER: no driver profile exists.", 400);
        }

        if (user.Preference is null)
        {
            user.Preference = new UserPreference
            {
                Id = Guid.Empty,
                ActiveAppMode = request.ActiveAppMode,
                PreferredMode = request.PreferredMode
            };
            await _userRepository.UpdateAsync(user);
        }
        else
        {
            bool updated = false;
            if (user.Preference.ActiveAppMode != request.ActiveAppMode)
            {
                user.Preference.ActiveAppMode = request.ActiveAppMode;
                updated = true;
            }
            if (user.Preference.PreferredMode != request.PreferredMode)
            {
                user.Preference.PreferredMode = request.PreferredMode;
                updated = true;
            }

            if (updated)
            {
                await _userRepository.UpdateAsync(user);
            }
        }

        return new UserPreferencesResponseDto
        {
            ActiveAppMode = user.Preference.ActiveAppMode,
            PreferredMode = user.Preference.PreferredMode
        };
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
            ActiveAppMode = user.Preference?.ActiveAppMode,
            ProfileObjectKey = user.ProfileObjectKey,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }

    public async Task<PaginatedResponseDto<UserResponseDto>> GetAdminUsersAsync(
    int page,
    int pageSize,
    string? search,
    string? role,
    string? status,
    DateTime? startDate,
    DateTime? endDate)
    {
        if (page <= 0)
            page = 1;

        if (pageSize <= 0)
            pageSize = 10;

        ActiveAppMode? parsedRole = null;

        if (!string.IsNullOrWhiteSpace(role))
        {
            if (!Enum.TryParse<ActiveAppMode>(role, true, out var roleValue))
                throw new InvalidOperationException("Invalid role filter.");

            parsedRole = roleValue;
        }

        UserStatus? parsedStatus = null;

        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!Enum.TryParse<UserStatus>(status, true, out var statusValue))
                throw new InvalidOperationException("Invalid status filter.");

            parsedStatus = statusValue;
        }

        var result = await _userRepository.GetAdminUsersAsync(
            page,
            pageSize,
            search,
            parsedRole,
            parsedStatus,
            startDate,
            endDate);

        return new PaginatedResponseDto<UserResponseDto>
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = result.TotalCount,
            TotalPages = (int)Math.Ceiling(result.TotalCount / (double)pageSize),
            Items = result.Items.Select(MapToResponseDto).ToList()
        };
    }
}
