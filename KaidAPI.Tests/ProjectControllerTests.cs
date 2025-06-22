using System;
using System.Security.Claims;
using System.Threading.Tasks;
using KaidAPI.Controllers;
using KaidAPI.Services;
using KaidAPI.ViewModel;
using KaidAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace KaidAPI.Tests.Controllers
{
    public class ProjectControllerTests
    {
        private readonly Mock<IProjectService> _mockProjectService = new();
        private ProjectController CreateControllerWithUser(string? oidcSub)
        {
            var controller = new ProjectController(_mockProjectService.Object);

            var user = new ClaimsPrincipal();
            if (!string.IsNullOrEmpty(oidcSub))
            {
                var identity = new ClaimsIdentity(new[]
                {
                    new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", oidcSub)
                }, "mock");
                user = new ClaimsPrincipal(identity);
            }

            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };

            return controller;
        }

        [Fact]
        public async Task CreateProject_Unauthorized_WhenOidcSubMissing()
        {
            var controller = CreateControllerWithUser(null);
            var result = await controller.CreateProject(new ProjectRequest());
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task CreateProject_ReturnsOk_WhenSuccess()
        {
            var oidcSub = "user-123";
            var serviceResult = new OperationResult { Success = true };
            _mockProjectService.Setup(x => x.CreateNewProjectAsync(It.IsAny<ProjectRequest>(), oidcSub))
                .ReturnsAsync(serviceResult);

            var controller = CreateControllerWithUser(oidcSub);
            var result = await controller.CreateProject(new ProjectRequest());

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(serviceResult, okResult.Value);
        }

        [Fact]
        public async Task CreateProject_ReturnsBadRequest_WhenFailure()
        {
            var oidcSub = "user-123";
            var serviceResult = new OperationResult { Success = false };
            _mockProjectService.Setup(x => x.CreateNewProjectAsync(It.IsAny<ProjectRequest>(), oidcSub))
                .ReturnsAsync(serviceResult);

            var controller = CreateControllerWithUser(oidcSub);
            var result = await controller.CreateProject(new ProjectRequest());

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(serviceResult, badRequest.Value);
        }

        [Fact]
        public async Task UpdateProject_Unauthorized_WhenOidcSubMissing()
        {
            var controller = CreateControllerWithUser(null);
            var result = await controller.UpdateProject(Guid.NewGuid(), new ProjectRequest());
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task UpdateProject_ReturnsOk_WhenSuccess()
        {
            var oidcSub = "user-123";
            var serviceResult = new OperationResult { Success = true };
            _mockProjectService.Setup(x => x.UpdateProjectAsync(It.IsAny<ProjectRequest>(), oidcSub, It.IsAny<Guid>()))
                .ReturnsAsync(serviceResult);

            var controller = CreateControllerWithUser(oidcSub);
            var result = await controller.UpdateProject(Guid.NewGuid(), new ProjectRequest());

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(serviceResult, okResult.Value);
        }

        [Fact]
        public async Task UpdateProject_ReturnsBadRequest_WhenFailure()
        {
            var oidcSub = "user-123";
            var serviceResult = new OperationResult { Success = false };
            _mockProjectService.Setup(x => x.UpdateProjectAsync(It.IsAny<ProjectRequest>(), oidcSub, It.IsAny<Guid>()))
                .ReturnsAsync(serviceResult);

            var controller = CreateControllerWithUser(oidcSub);
            var result = await controller.UpdateProject(Guid.NewGuid(), new ProjectRequest());

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(serviceResult, badRequest.Value);
        }

        [Fact]
        public async Task GetProjects_Unauthorized_WhenOidcSubMissing()
        {
            var controller = CreateControllerWithUser(null);
            var result = await controller.GetProjects();
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task GetProjects_ReturnsOk_WhenSuccess()
        {
            var oidcSub = "user-123";
            var serviceResult = new OperationResult { Success = true };
            _mockProjectService.Setup(x => x.GetProjectsByUserIdAsync(oidcSub))
                .ReturnsAsync(serviceResult);

            var controller = CreateControllerWithUser(oidcSub);
            var result = await controller.GetProjects();

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(serviceResult, okResult.Value);
        }

        [Fact]
        public async Task GetProjects_ReturnsBadRequest_WhenFailure()
        {
            var oidcSub = "user-123";
            var serviceResult = new OperationResult { Success = false };
            _mockProjectService.Setup(x => x.GetProjectsByUserIdAsync(oidcSub))
                .ReturnsAsync(serviceResult);

            var controller = CreateControllerWithUser(oidcSub);
            var result = await controller.GetProjects();

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(serviceResult, badRequest.Value);
        }

        [Fact]
        public async Task GetInvitations_Unauthorized_WhenOidcSubMissing()
        {
            var controller = CreateControllerWithUser(null);
            var result = await controller.GetInvitations();
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task GetInvitations_ReturnsOk_WhenSuccess()
        {
            var oidcSub = "user-123";
            var serviceResult = new OperationResult { Success = true };
            _mockProjectService.Setup(x => x.GetInvitationsByUserIdAsync(oidcSub))
                .ReturnsAsync(serviceResult);

            var controller = CreateControllerWithUser(oidcSub);
            var result = await controller.GetInvitations();

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(serviceResult, okResult.Value);
        }

        [Fact]
        public async Task GetInvitations_ReturnsBadRequest_WhenFailure()
        {
            var oidcSub = "user-123";
            var serviceResult = new OperationResult { Success = false };
            _mockProjectService.Setup(x => x.GetInvitationsByUserIdAsync(oidcSub))
                .ReturnsAsync(serviceResult);

            var controller = CreateControllerWithUser(oidcSub);
            var result = await controller.GetInvitations();

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(serviceResult, badRequest.Value);
        }

        [Fact]
        public async Task DeleteProject_Unauthorized_WhenOidcSubMissing()
        {
            var controller = CreateControllerWithUser(null);
            var result = await controller.DeleteProject(Guid.NewGuid());
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task DeleteProject_ReturnsOk_WhenSuccess()
        {
            var oidcSub = "user-123";
            var serviceResult = new OperationResult { Success = true };
            _mockProjectService.Setup(x => x.DeleteProjectAsync(It.IsAny<Guid>(), oidcSub))
                .ReturnsAsync(serviceResult);

            var controller = CreateControllerWithUser(oidcSub);
            var result = await controller.DeleteProject(Guid.NewGuid());

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(serviceResult, okResult.Value);
        }

        [Fact]
        public async Task DeleteProject_ReturnsBadRequest_WhenFailure()
        {
            var oidcSub = "user-123";
            var serviceResult = new OperationResult { Success = false };
            _mockProjectService.Setup(x => x.DeleteProjectAsync(It.IsAny<Guid>(), oidcSub))
                .ReturnsAsync(serviceResult);

            var controller = CreateControllerWithUser(oidcSub);
            var result = await controller.DeleteProject(Guid.NewGuid());

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(serviceResult, badRequest.Value);
        }

        [Fact]
        public async Task GetProjectDeadline_Unauthorized_WhenOidcSubMissing()
        {
            var controller = CreateControllerWithUser(null);
            var result = await controller.GetProjectDeadline(Guid.NewGuid());
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task GetProjectDeadline_ReturnsOk_WhenSuccess()
        {
            var oidcSub = "user-123";
            var serviceResult = new OperationResult { Success = true };
            _mockProjectService.Setup(x => x.GetProjectDeadlineAsync(oidcSub, It.IsAny<Guid>()))
                .ReturnsAsync(serviceResult);

            var controller = CreateControllerWithUser(oidcSub);
            var result = await controller.GetProjectDeadline(Guid.NewGuid());

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(serviceResult, okResult.Value);
        }

        [Fact]
        public async Task GetProjectDeadline_ReturnsBadRequest_WhenFailure()
        {
            var oidcSub = "user-123";
            var serviceResult = new OperationResult { Success = false };
            _mockProjectService.Setup(x => x.GetProjectDeadlineAsync(oidcSub, It.IsAny<Guid>()))
                .ReturnsAsync(serviceResult);

            var controller = CreateControllerWithUser(oidcSub);
            var result = await controller.GetProjectDeadline(Guid.NewGuid());

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(serviceResult, badRequest.Value);
        }
    }
}
