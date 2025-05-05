using KaidAPI.Context;
using KaidAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace KaidAPI.Services;

public class KaidUserService : IKaidUserService
{
    private readonly ServerDbContext _context;
    private readonly ILogger<KaidUserService> _logger;
    public KaidUserService(ServerDbContext context, ILogger<KaidUserService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    public async Task<Guid> FindOrCreateUserByOidcAsync(string issuer, string subject, string? email, string? name)
        {
            if (string.IsNullOrWhiteSpace(issuer)) throw new ArgumentNullException(nameof(issuer));
            if (string.IsNullOrWhiteSpace(subject)) throw new ArgumentNullException(nameof(subject));

            _logger.LogInformation("Attempting to find or create user directly in User table for Issuer: {Issuer}, Subject: {Subject}", issuer, subject);

            var existingUser = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.AuthentikIssuer == issuer && u.AuthentikSubject == subject);


            if (existingUser != null)
            {
                _logger.LogInformation("Found existing user with Kaid UserID: {UserId} for Issuer: {Issuer}, Subject: {Subject}", existingUser.UserID, issuer, subject);
                return existingUser.UserID;
            }

            _logger.LogInformation("No existing user found. Creating new user for Issuer: {Issuer}, Subject: {Subject}", issuer, subject);

            try
            {
                var newUserIdGuid = Guid.NewGuid();
                var newUser = new User
                {
                    UserID = newUserIdGuid,
                    Username = name ?? $"user_{newUserIdGuid.ToString("N").Substring(0, 8)}",
                    Email = email,
                    CreatedAt = DateTime.UtcNow,
                    AuthentikIssuer = issuer,
                    AuthentikSubject = subject 
                };
                
                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                _logger.LogInformation("New user created with Kaid UserID: {UserId}, linked to Issuer: {Issuer}, Subject: {Subject}", newUser.UserID, issuer, subject);
                
                return newUserIdGuid;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error occurred while creating user for Issuer: {Issuer}, Subject: {Subject}.", issuer, subject);

                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user for Issuer: {Issuer}, Subject: {Subject}.", issuer, subject);
                throw;
            }
        }

}