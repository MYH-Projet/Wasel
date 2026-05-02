using Microsoft.EntityFrameworkCore;
using Wasel.Api.Modules.Users.Entities;
using Wasel.Api.Shared.Database;

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
        return await _context.Users.ToListAsync();
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }
    }