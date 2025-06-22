using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using KaidAPI.Controllers;
using KaidAPI.Models;
using KaidAPI.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace KaidAPI.Tests
{
    public class TeamControllerTests
    {
        private readonly Mock<ITeamService> mockTeamService;
        private readonly TeamController controller;

        public TeamControllerTests()
        {
            mockTeamService = new Mock<ITeamService>();
            controller = new TeamController(mockTeamService.Object);

            // HttpContext, User.Claims ¼³Á¤ (OIDC Sub)
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", "test-oidc-sub")
            }, "mock"));

            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
        }

        [Fact]
        public async Task CreateTeam_ReturnsOk_WithGuid()
        {
            var request = new TeamRequest
            {
                ProjectId = Guid.NewGuid(),
                TeamName = "Test Team",
                Description = "Description"
            };

            var expectedGuid = Guid.NewGuid();
            mockTeamService.Setup(x => x.CreateTeamAsync(request))
                .ReturnsAsync(expectedGuid);

            var result = await controller.CreateTeam(request);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedGuid, okResult.Value);
        }

        [Fact]
        public async Task DeleteTeam_ReturnsOk_WithOperationResult()
        {
            var teamId = Guid.NewGuid();
            var expectedResult = new OperationResult { Success = true };

            mockTeamService.Setup(x => x.DeleteTeamAsync("test-oidc-sub", teamId))
                .ReturnsAsync(expectedResult);

            var result = await controller.DeleteTeam(teamId);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedResult, okResult.Value);
        }

        [Fact]
        public async Task UpdateTeam_ReturnsOk_WithOperationResult()
        {
            var teamId = Guid.NewGuid();
            var request = new TeamRequest
            {
                ProjectId = Guid.NewGuid(),
                TeamName = "Updated Team",
                Description = "Updated Description"
            };
            var expectedResult = new OperationResult { Success = true };

            mockTeamService.Setup(x => x.UpdateTeamAsync("test-oidc-sub", teamId, request))
                .ReturnsAsync(expectedResult);

            var result = await controller.UpdateTeam(teamId, request);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedResult, okResult.Value);
        }

        [Fact]
        public async Task GetTeams_ReturnsOk_WithOperationResult()
        {
            var projectId = Guid.NewGuid();
            var expectedResult = new OperationResult { Success = true, Data = new List<Team>() };

            mockTeamService.Setup(x => x.GetTeamsByProjectId("test-oidc-sub", projectId))
                .ReturnsAsync(expectedResult);

            var result = await controller.GetTeams(projectId);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedResult, okResult.Value);
        }

        [Fact]
        public async Task Methods_ReturnUnauthorized_IfOidcSubMissing()
        {
            // Setup controller with empty user claims (no OIDC sub)
            var emptyUser = new ClaimsPrincipal(new ClaimsIdentity());
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = emptyUser }
            };

            var createResult = await controller.CreateTeam(new TeamRequest());
            Assert.IsType<UnauthorizedObjectResult>(createResult);

            var deleteResult = await controller.DeleteTeam(Guid.NewGuid());
            Assert.IsType<UnauthorizedObjectResult>(deleteResult);

            var updateResult = await controller.UpdateTeam(Guid.NewGuid(), new TeamRequest());
            Assert.IsType<UnauthorizedObjectResult>(updateResult);

            var getResult = await controller.GetTeams(Guid.NewGuid());
            Assert.IsType<UnauthorizedObjectResult>(getResult);
        }
    }
}
