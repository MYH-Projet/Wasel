using Wasel.Api.Modules.Users.Entities;
using Wasel.Api.Modules.Users.Enums;

namespace Wasel.Api.Modules.Users.Repositories;

public interface IUserRepository
{
    Task<List<User>> GetAllAsync();

    Task<User?> GetByIdAsync(Guid id);

    Task<User?> GetByKeycloakIdAsync(string keycloakId);

    Task<User?> GetByEmailAsync(string email);

    Task<User> AddAsync(User user);

    Task UpdateAsync(User user);

    Task<(List<User> Items, int TotalCount)> GetAdminUsersAsync(
    int page,
    int pageSize,
    string? search,
    ActiveAppMode? role,
    UserStatus? status,
    DateTime? startDate,
    DateTime? endDate);
}