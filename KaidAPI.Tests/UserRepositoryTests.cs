using System;
using System.Threading.Tasks;
using KaidAPI.Context;
using KaidAPI.Models;
using KaidAPI.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace KaidAPI.Tests
{
    public class UserRepositoryTests
    {
        private ServerDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<ServerDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ServerDbContext(options);
        }

        [Fact]
        public async Task GetUserByOidcAsync_ShouldReturnCorrectUser()
        {
            // Arrange
            using var context = CreateInMemoryContext();

            var testUser = new User
            {
                UserId = Guid.NewGuid(),
                Email = "test@example.com",
                AuthentikSubject = "testsub",
                Username = "tester"
            };

            context.Users.Add(testUser);
            await context.SaveChangesAsync();

            var repository = new UserRepository(context);

            // Act
            var result = await repository.GetUserByOidcAsync("testsub");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("tester", result.Username);
            Assert.Equal("test@example.com", result.Email);
        }
    }
}