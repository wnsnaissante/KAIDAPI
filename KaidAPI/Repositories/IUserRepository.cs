using KaidAPI.Models;

namespace KaidAPI.Repositories;

public interface IUserRepository
{
    Task<Guid> CreateUserAsync(User user);
    Task<User?> GetUserByEmailAsync(string email);
    Task<User?> GetUserByIdAsync(Guid userId);
    Task<User?> GetUserByOidcAsync(string oidcSub);
    Task<String> GetUserNameByIdAsync(Guid userId);
}