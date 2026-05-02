using Wasel.Api.Modules.Users.Entities;

namespace Wasel.Api.Modules.Users.Repositories;

public interface IUserRepository
{
    Task<List<User>> GetAllAsync();
    // return await _context.Users.ToListAsync();

    Task<User?> GetByIdAsync(Guid id);
    // ? Parce qu’il est possible que l’utilisateur n’existe pas.

    Task UpdateAsync(User user);
}