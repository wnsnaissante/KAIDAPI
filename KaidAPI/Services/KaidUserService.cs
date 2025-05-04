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

            // 1. User 테이블에서 Issuer와 Subject가 일치하는 사용자 직접 조회
            var existingUser = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.AuthentikIssuer == issuer && u.AuthentikSubject == subject);

            // 2. 이미 등록된 사용자가 있으면 해당 UserID 반환
            if (existingUser != null)
            {
                _logger.LogInformation("Found existing user with Kaid UserID: {UserId} for Issuer: {Issuer}, Subject: {Subject}", existingUser.UserID, issuer, subject);
                return existingUser.UserID;
            }

            // 3. 등록된 사용자가 없으면 새로 생성
            _logger.LogInformation("No existing user found. Creating new user for Issuer: {Issuer}, Subject: {Subject}", issuer, subject);

            // User 생성은 단일 작업이므로 트랜잭션은 필수는 아님 (SaveChanges가 원자성 보장 시도)
            // 하지만 명시적으로 사용하는 것이 더 안전할 수는 있음
            try
            {
                // 3a. 새 User 객체 생성 및 Authentik 정보 포함
                var newUserIdGuid = Guid.NewGuid(); // 새 Guid 생성
                var newUser = new User
                {
                    UserID = newUserIdGuid, // Guid를 string으로 변환하여 PK 설정
                    Username = name ?? $"user_{newUserIdGuid.ToString("N").Substring(0, 8)}", // 임의 생성 또는 정책 필요
                    Email = email, // Nullable 이메일
                    CreatedAt = DateTime.UtcNow,
                    AuthentikIssuer = issuer,   // Authentik 정보 직접 저장
                    AuthentikSubject = subject // Authentik 정보 직접 저장
                };

                // 3b. User 테이블에 추가 및 저장
                _context.Users.Add(newUser);
                await _context.SaveChangesAsync(); // DB에 저장

                _logger.LogInformation("New user created with Kaid UserID: {UserId}, linked to Issuer: {Issuer}, Subject: {Subject}", newUser.UserID, issuer, subject);

                // 3c. 새로 생성된 UserID (Guid 타입) 반환
                return newUserIdGuid;
            }
            catch (DbUpdateException ex) // 특히 Unique 제약 조건 위반 등 DB 오류 처리
            {
                _logger.LogError(ex, "Database error occurred while creating user for Issuer: {Issuer}, Subject: {Subject}.", issuer, subject);
                // Unique 제약 조건 위반 (예: 이메일 중복) 등의 경우 더 구체적인 처리 가능
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user for Issuer: {Issuer}, Subject: {Subject}.", issuer, subject);
                throw;
            }
        }

}