using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using KaidAPI.Controllers;
using KaidAPI.Models;
using KaidAPI.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace KaidAPI.Tests
{
    public class UserControllerTests
    {
        [Fact]
        public async Task GetUserByOidc_ReturnsUser_WhenUserExists()
        {
            // Arrange
            var oidcSub = "test-oidc-sub";

            var expectedUser = new User
            {
                UserId = Guid.NewGuid(),
                Email = "test@example.com",
                CreatedAt = DateTime.UtcNow
            };

            var mockRepo = new Mock<IUserRepository>();
            mockRepo.Setup(repo => repo.GetUserByOidcAsync(oidcSub))
                    .ReturnsAsync(expectedUser);

            var controller = new UserController(mockRepo.Object);

            // 사용자 ClaimsPrincipal 생성
            var claims = new List<Claim>
            {
                new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", oidcSub)
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var user = new ClaimsPrincipal(identity);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var result = await controller.GetUserByOidc(oidcSub);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedUser = Assert.IsType<User>(okResult.Value);
            Assert.Equal(expectedUser.Email, returnedUser.Email);
        }

        [Fact]
        public async Task GetUserByOidc_ReturnsNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var oidcSub = "nonexistent-oidc-sub";

            var mockRepo = new Mock<IUserRepository>();
            mockRepo.Setup(repo => repo.GetUserByOidcAsync(oidcSub))
                    .ReturnsAsync((User?)null);

            var controller = new UserController(mockRepo.Object);

            // 사용자 ClaimsPrincipal 생성
            var claims = new List<Claim>
            {
                new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", oidcSub)
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var user = new ClaimsPrincipal(identity);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var result = await controller.GetUserByOidc(oidcSub);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
