namespace KaidAPI.Services;

public interface IKaidUserService
{
    Task<Guid> FindOrCreateUserByOidcAsync(string issuer, string subject, string? email, string? name);
}