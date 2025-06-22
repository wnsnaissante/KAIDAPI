using System;
using System.Threading.Tasks;
using KaidAPI.Context;
using KaidAPI.Models;
using KaidAPI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class KaidUserServiceTests
{
    private ServerDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ServerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()) // 독립된 DB 생성
            .Options;
        return new ServerDbContext(options);
    }

    [Fact]
    public async Task FindOrCreateUserByOidcAsync_ReturnsExistingUserId_WhenUserExists()
    {
        var context = GetInMemoryDbContext();
        var loggerMock = new Mock<ILogger<KaidUserService>>();

        var existingUser = new User
        {
            UserId = Guid.NewGuid(),
            AuthentikIssuer = "issuer1",
            AuthentikSubject = "subject1",
            Email = "test@example.com",
            Username = "existingUser",
            CreatedAt = DateTime.UtcNow
        };
        context.Users.Add(existingUser);
        await context.SaveChangesAsync();

        var service = new KaidUserService(context, loggerMock.Object);

        var result = await service.FindOrCreateUserByOidcAsync("issuer1", "subject1", "test@example.com", "existingUser");

        Assert.Equal(existingUser.UserId, result);
    }

    [Fact]
    public async Task FindOrCreateUserByOidcAsync_CreatesAndReturnsNewUserId_WhenUserDoesNotExist()
    {
        var context = GetInMemoryDbContext();
        var loggerMock = new Mock<ILogger<KaidUserService>>();

        var service = new KaidUserService(context, loggerMock.Object);

        var newUserId = await service.FindOrCreateUserByOidcAsync("newIssuer", "newSubject", "new@example.com", "newUser");

        var createdUser = await context.Users.FirstOrDefaultAsync(u => u.UserId == newUserId);

        Assert.NotNull(createdUser);
        Assert.Equal("newIssuer", createdUser.AuthentikIssuer);
        Assert.Equal("newSubject", createdUser.AuthentikSubject);
        Assert.Equal("new@example.com", createdUser.Email);
        Assert.Equal("newUser", createdUser.Username);
    }

    [Fact]
    public async Task FindOrCreateUserByOidcAsync_ThrowsException_WhenSaveFails()
    {
        var options = new DbContextOptionsBuilder<ServerDbContext>()
            .UseInMemoryDatabase("TestDB")
            .Options;
        var context = new ServerDbContext(options);

        // Dispose context to simulate failure on SaveChangesAsync
        context.Dispose();

        var loggerMock = new Mock<ILogger<KaidUserService>>();
        var service = new KaidUserService(context, loggerMock.Object);

        await Assert.ThrowsAsync<ObjectDisposedException>(async () =>
        {
            await service.FindOrCreateUserByOidcAsync("issuer", "subject", "email", "name");
        });
    }
}
