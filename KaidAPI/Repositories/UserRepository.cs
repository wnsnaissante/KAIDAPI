using KaidAPI.Context;
using KaidAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace KaidAPI.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ServerDbContext _context;

    public UserRepository(ServerDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Guid> CreateUserAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user.UserId;
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetUserByIdAsync(Guid userId)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.UserId == userId);
    }

    public async Task<User?> GetUserByOidcAsync(string oidcSub)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.AuthentikSubject == oidcSub);
    }
}