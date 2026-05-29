using Microsoft.EntityFrameworkCore;
using Wasel.Api.Modules.Users.Entities;
using Wasel.Api.Shared.Database;
using Wasel.Api.Modules.Users.Enums;
namespace Wasel.Api.Modules.Users.Repositories;

public class UserRepository : IUserRepository
{
    private readonly WaselDbContext _context;

    public UserRepository(WaselDbContext context)
    {
        _context = context;
    }

    public async Task<List<User>> GetAllAsync()
    {
        return await _context.Users
            .Include(u => u.Preference)
            .ToListAsync();
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.Users
            .Include(u => u.Preference)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> GetByKeycloakIdAsync(string keycloakId)
    {
        return await _context.Users
            .Include(u => u.Preference)
            .FirstOrDefaultAsync(u => u.KeycloakId == keycloakId);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .Include(u => u.Preference)
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User> AddAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    public async Task<(List<User> Items, int TotalCount)> GetAdminUsersAsync(
    int page,
    int pageSize,
    string? search,
    ActiveAppMode? role,
    UserStatus? status,
    DateTime? startDate,
    DateTime? endDate)
    {
        var query = _context.Users
            .Include(u => u.Preference)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search}%";

            query = query.Where(u =>
                EF.Functions.ILike(u.FirstName, pattern) ||
                EF.Functions.ILike(u.LastName, pattern) ||
                EF.Functions.ILike(u.Email, pattern) ||
                EF.Functions.ILike(u.Phone, pattern));
        }

        if (role.HasValue)
        {
            query = query.Where(u =>
                u.Preference != null &&
                u.Preference.ActiveAppMode == role.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(u => u.Status == status.Value);
        }

        if (startDate.HasValue)
{
    var utcStartDate = DateTime.SpecifyKind(startDate.Value, DateTimeKind.Utc);
    query = query.Where(u => u.CreatedAt >= utcStartDate);
}

if (endDate.HasValue)
{
    var utcEndDate = DateTime.SpecifyKind(endDate.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);
    query = query.Where(u => u.CreatedAt <= utcEndDate);
}

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

}