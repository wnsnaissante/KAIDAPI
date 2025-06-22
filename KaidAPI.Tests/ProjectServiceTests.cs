using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KaidAPI.Models;
using KaidAPI.Repositories;
using KaidAPI.Services;
using KaidAPI.ViewModel;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace KaidAPI.Tests.Services
{
    public class ProjectServiceTests
    {
        private readonly Mock<IUserRepository> _mockUserRepo = new();
        private readonly Mock<IProjectRepository> _mockProjectRepo = new();
        private readonly Mock<IMembershipRepository> _mockMembershipRepo = new();
        private readonly Mock<ILogger<ProjectService>> _mockLogger = new();

        private ProjectService CreateService() => new(
            _mockUserRepo.Object,
            _mockProjectRepo.Object,
            _mockMembershipRepo.Object,
            _mockLogger.Object);

        [Fact]
        public async Task CreateNewProjectAsync_UserNotFound_ReturnsFailure()
        {
            _mockUserRepo.Setup(x => x.GetUserByOidcAsync(It.IsAny<string>()))
                .ReturnsAsync((User?)null);

            var service = CreateService();

            var result = await service.CreateNewProjectAsync(new ProjectRequest { ProjectName = "Test" }, "oidc-sub");

            Assert.False(result.Success);
            Assert.Equal("User not found", result.Message);
        }

        [Fact]
        public async Task CreateNewProjectAsync_Success_ReturnsProjectId()
        {
            var userId = Guid.NewGuid();

            _mockUserRepo.Setup(x => x.GetUserByOidcAsync(It.IsAny<string>()))
                .ReturnsAsync(new User { UserId = userId, Username = "tester" });

            _mockProjectRepo.Setup(x => x.CreateProjectAsync(It.IsAny<Project>()))
                .ReturnsAsync(Guid.NewGuid());

            _mockMembershipRepo.Setup(x => x.CreateMembershipAsync(It.IsAny<Membership>()))
                .ReturnsAsync(new OperationResult { Success = true, Message = "Success" });

            var service = CreateService();

            var request = new ProjectRequest { ProjectName = "New Project", ProjectDescription = "Desc" };
            var result = await service.CreateNewProjectAsync(request, "oidc-sub");

            Assert.True(result.Success);
            Assert.Equal("Project created successfully", result.Message);
            Assert.IsType<Guid>(result.Data);
        }

        [Fact]
        public async Task UpdateProjectAsync_NoUser_ReturnsFailure()
        {
            _mockUserRepo.Setup(x => x.GetUserByOidcAsync(It.IsAny<string>())).ReturnsAsync((User?)null);

            var service = CreateService();
            var request = new ProjectRequest { ProjectName = "Updated" };

            var result = await service.UpdateProjectAsync(request, "oidc-sub", Guid.NewGuid());

            Assert.False(result.Success);
            Assert.Equal("User not found", result.Message);
        }

        [Fact]
        public async Task UpdateProjectAsync_ProjectNotFound_ReturnsFailure()
        {
            _mockUserRepo.Setup(x => x.GetUserByOidcAsync(It.IsAny<string>()))
                .ReturnsAsync(new User { UserId = Guid.NewGuid() });

            _mockProjectRepo.Setup(x => x.GetProjectByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Project?)null);

            var service = CreateService();
            var request = new ProjectRequest { ProjectName = "Updated" };

            var result = await service.UpdateProjectAsync(request, "oidc-sub", Guid.NewGuid());

            Assert.False(result.Success);
            Assert.Equal("Project not found", result.Message);
        }

        [Fact]
        public async Task UpdateProjectAsync_NoPermission_ReturnsFailure()
        {
            var userId = Guid.NewGuid();
            var project = new Project { ProjectId = Guid.NewGuid(), OwnerId = Guid.NewGuid() };

            _mockUserRepo.Setup(x => x.GetUserByOidcAsync(It.IsAny<string>()))
                .ReturnsAsync(new User { UserId = userId });

            _mockProjectRepo.Setup(x => x.GetProjectByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(project);

            var service = CreateService();
            var request = new ProjectRequest { ProjectName = "Updated" };

            var result = await service.UpdateProjectAsync(request, "oidc-sub", project.ProjectId);

            Assert.False(result.Success);
            Assert.Equal("No permission to edit project", result.Message);
        }

        [Fact]
        public async Task UpdateProjectAsync_Success_ReturnsSuccess()
        {
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var existingProject = new Project
            {
                ProjectId = projectId,
                OwnerId = userId,
                ProjectName = "Old Name",
                CreatedAt = DateTime.UtcNow
            };

            _mockUserRepo.Setup(x => x.GetUserByOidcAsync(It.IsAny<string>()))
                .ReturnsAsync(new User { UserId = userId });

            _mockProjectRepo.Setup(x => x.GetProjectByIdAsync(projectId))
                .ReturnsAsync(existingProject);

            _mockProjectRepo.Setup(x => x.UpdateProjectAsync(It.IsAny<Project>()))
                .Returns(Task.CompletedTask);

            var service = CreateService();
            var request = new ProjectRequest { ProjectName = "New Name", ProjectDescription = "New Desc" };

            var result = await service.UpdateProjectAsync(request, "oidc-sub", projectId);

            Assert.True(result.Success);
            Assert.Equal("Project updated successfully", result.Message);
        }

        [Fact]
        public async Task DeleteProjectAsync_NoUser_ReturnsFailure()
        {
            _mockUserRepo.Setup(x => x.GetUserByOidcAsync(It.IsAny<string>()))
                .ReturnsAsync((User?)null);

            var service = CreateService();

            var result = await service.DeleteProjectAsync(Guid.NewGuid(), "oidc-sub");

            Assert.False(result.Success);
            Assert.Equal("User not found", result.Message);
        }

        [Fact]
        public async Task DeleteProjectAsync_ProjectNotFound_ReturnsFailure()
        {
            var userId = Guid.NewGuid();

            _mockUserRepo.Setup(x => x.GetUserByOidcAsync(It.IsAny<string>()))
                .ReturnsAsync(new User { UserId = userId });

            _mockProjectRepo.Setup(x => x.GetProjectByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Project?)null);

            var service = CreateService();

            var result = await service.DeleteProjectAsync(Guid.NewGuid(), "oidc-sub");

            Assert.False(result.Success);
            Assert.Equal("Project not found", result.Message);
        }

        [Fact]
        public async Task DeleteProjectAsync_NoPermission_ReturnsFailure()
        {
            var userId = Guid.NewGuid();

            var project = new Project { ProjectId = Guid.NewGuid(), OwnerId = Guid.NewGuid() };

            _mockUserRepo.Setup(x => x.GetUserByOidcAsync(It.IsAny<string>()))
                .ReturnsAsync(new User { UserId = userId });

            _mockProjectRepo.Setup(x => x.GetProjectByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(project);

            var service = CreateService();

            var result = await service.DeleteProjectAsync(project.ProjectId, "oidc-sub");

            Assert.False(result.Success);
            Assert.Equal("No permission to delete project", result.Message);
        }

        [Fact]
        public async Task DeleteProjectAsync_Success_ReturnsSuccess()
        {
            var userId = Guid.NewGuid();

            var project = new Project { ProjectId = Guid.NewGuid(), OwnerId = userId };

            _mockUserRepo.Setup(x => x.GetUserByOidcAsync(It.IsAny<string>()))
                .ReturnsAsync(new User { UserId = userId });

            _mockProjectRepo.Setup(x => x.GetProjectByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(project);

            _mockProjectRepo.Setup(x => x.DeleteProjectAsync(It.IsAny<Guid>()))
                .Returns(Task.CompletedTask);

            var service = CreateService();

            var result = await service.DeleteProjectAsync(project.ProjectId, "oidc-sub");

            Assert.True(result.Success);
            Assert.Equal("Project deleted successfully", result.Message);
        }

        [Fact]
        public async Task GetProjectByProjectIdAsync_NoUser_ReturnsFailure()
        {
            _mockUserRepo.Setup(x => x.GetUserByOidcAsync(It.IsAny<string>()))
                .ReturnsAsync((User?)null);

            var service = CreateService();

            var result = await service.GetProjectByProjectIdAsync("oidc-sub", Guid.NewGuid());

            Assert.False(result.Success);
            Assert.Equal("User not found", result.Message);
        }

        [Fact]
        public async Task GetProjectByProjectIdAsync_ProjectNotFound_ReturnsFailure()
        {
            var userId = Guid.NewGuid();

            _mockUserRepo.Setup(x => x.GetUserByOidcAsync(It.IsAny<string>()))
                .ReturnsAsync(new User { UserId = userId });

            _mockProjectRepo.Setup(x => x.GetProjectByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Project?)null);

            var service = CreateService();

            var result = await service.GetProjectByProjectIdAsync("oidc-sub", Guid.NewGuid());

            Assert.False(result.Success);
            Assert.Equal("Project not found", result.Message);
        }

        [Fact]
        public async Task GetProjectByProjectIdAsync_NoPermission_ReturnsFailure()
        {
            var userId = Guid.NewGuid();

            var project = new Project { ProjectId = Guid.NewGuid(), OwnerId = Guid.NewGuid() };

            _mockUserRepo.Setup(x => x.GetUserByOidcAsync(It.IsAny<string>()))
                .ReturnsAsync(new User { UserId = userId });

            _mockProjectRepo.Setup(x => x.GetProjectByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(project);

            var service = CreateService();

            var result = await service.GetProjectByProjectIdAsync("oidc-sub", project.ProjectId);

            Assert.False(result.Success);
            Assert.Equal("No permission to get project", result.Message);
        }

        [Fact]
        public async Task GetProjectByProjectIdAsync_Success_ReturnsProject()
        {
            var userId = Guid.NewGuid();

            var project = new Project { ProjectId = Guid.NewGuid(), OwnerId = userId };

            _mockUserRepo.Setup(x => x.GetUserByOidcAsync(It.IsAny<string>()))
                .ReturnsAsync(new User { UserId = userId });

            _mockProjectRepo.Setup(x => x.GetProjectByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(project);

            var service = CreateService();

            var result = await service.GetProjectByProjectIdAsync("oidc-sub", project.ProjectId);

            Assert.True(result.Success);
            Assert.Equal("Project retrieved successfully", result.Message);
            Assert.Equal(project.ProjectId, ((Project)result.Data).ProjectId);
        }

        [Fact]
        public async Task GetProjectsByUserIdAsync_NoUser_ReturnsFailure()
        {
            _mockUserRepo.Setup(x => x.GetUserByOidcAsync(It.IsAny<string>()))
                .ReturnsAsync((User?)null);

            var service = CreateService();

            var result = await service.GetProjectsByUserIdAsync("oidc-sub");

            Assert.False(result.Success);
            Assert.Equal("User not found", result.Message);
        }

        [Fact]
        public async Task GetProjectsByUserIdAsync_Success_ReturnsProjects()
        {
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var user = new User { UserId = userId };
            var memberships = new List<Membership>
    {
        new Membership { ProjectId = projectId, IsActivated = true }
    };
            var project = new Project
            {
                ProjectId = projectId,
                ProjectName = "Test Project",
                ProjectDescription = "Desc",
                CreatedAt = DateTime.UtcNow,
                DueDate = null,
                OwnerId = userId
            };

            _mockUserRepo.Setup(x => x.GetUserByOidcAsync(It.IsAny<string>())).ReturnsAsync(user);
            _mockMembershipRepo.Setup(x => x.GetActivatedMembershipsByUserIdAsync(userId)).ReturnsAsync(memberships);
            _mockProjectRepo.Setup(x => x.GetProjectByIdAsync(projectId)).ReturnsAsync(project);

            var service = CreateService();

            var result = await service.GetProjectsByUserIdAsync("oidc-sub");

            Assert.True(result.Success);
            Assert.Equal("Projects retrieved successfully", result.Message);
            var projects = Assert.IsType<List<ProjectResponse>>(result.Data);
            Assert.Single(projects);
            Assert.Equal(projectId, projects[0].ProjectId);
        }

        [Fact]
        public async Task GetInvitationsByUserIdAsync_NoUser_ReturnsFailure()
        {
            _mockUserRepo.Setup(x => x.GetUserByOidcAsync(It.IsAny<string>()))
                .ReturnsAsync((User?)null);

            var service = CreateService();

            var result = await service.GetInvitationsByUserIdAsync("oidc-sub");

            Assert.False(result.Success);
            Assert.Equal("User not found", result.Message);
        }

        [Fact]
        public async Task GetInvitationsByUserIdAsync_Success_ReturnsInvitations()
        {
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var user = new User { UserId = userId };
            var memberships = new List<Membership>
    {
        new Membership { ProjectId = projectId, IsActivated = false }
    };
            var project = new Project
            {
                ProjectId = projectId,
                ProjectName = "Invited Project",
                ProjectDescription = "Invitation Desc",
                CreatedAt = DateTime.UtcNow,
                DueDate = null,
                OwnerId = userId
            };

            _mockUserRepo.Setup(x => x.GetUserByOidcAsync(It.IsAny<string>())).ReturnsAsync(user);
            _mockMembershipRepo.Setup(x => x.GetDeactivatedMembershipsByUserIdAsync(userId)).ReturnsAsync(memberships);
            _mockProjectRepo.Setup(x => x.GetProjectByIdAsync(projectId)).ReturnsAsync(project);

            var service = CreateService();

            var result = await service.GetInvitationsByUserIdAsync("oidc-sub");

            Assert.True(result.Success);
            Assert.Equal("Projects retrieved successfully", result.Message);
            var projects = Assert.IsType<List<ProjectResponse>>(result.Data);
            Assert.Single(projects);
            Assert.Equal(projectId, projects[0].ProjectId);
        }

        [Fact]
        public async Task GetProjectDeadlineAsync_NoUser_ReturnsFailure()
        {
            _mockUserRepo.Setup(x => x.GetUserByOidcAsync(It.IsAny<string>()))
                .ReturnsAsync((User?)null);

            var service = CreateService();

            var result = await service.GetProjectDeadlineAsync("oidc-sub", Guid.NewGuid());

            Assert.False(result.Success);
            Assert.Equal("User not found", result.Message);
        }

        [Fact]
        public async Task GetProjectDeadlineAsync_ProjectNotFound_ReturnsFailure()
        {
            var userId = Guid.NewGuid();

            _mockUserRepo.Setup(x => x.GetUserByOidcAsync(It.IsAny<string>()))
                .ReturnsAsync(new User { UserId = userId });

            _mockProjectRepo.Setup(x => x.GetProjectByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Project?)null);

            var service = CreateService();

            var result = await service.GetProjectDeadlineAsync("oidc-sub", Guid.NewGuid());

            Assert.False(result.Success);
            Assert.Equal("Project not found", result.Message);
        }

        [Fact]
        public async Task GetProjectDeadlineAsync_NoPermission_ReturnsFailure()
        {
            var userId = Guid.NewGuid();

            var project = new Project { ProjectId = Guid.NewGuid(), OwnerId = Guid.NewGuid() };

            _mockUserRepo.Setup(x => x.GetUserByOidcAsync(It.IsAny<string>()))
                .ReturnsAsync(new User { UserId = userId });

            _mockProjectRepo.Setup(x => x.GetProjectByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(project);

            var service = CreateService();

            var result = await service.GetProjectDeadlineAsync("oidc-sub", project.ProjectId);

            Assert.False(result.Success);
            Assert.Equal("No permission to get project deadline", result.Message);
        }

        [Fact]
        public async Task GetProjectDeadlineAsync_Success_ReturnsDueDate()
        {
            var userId = Guid.NewGuid();

            var dueDate = DateTime.UtcNow.AddDays(7);
            var project = new Project { ProjectId = Guid.NewGuid(), OwnerId = userId, DueDate = dueDate };

            _mockUserRepo.Setup(x => x.GetUserByOidcAsync(It.IsAny<string>()))
                .ReturnsAsync(new User { UserId = userId });

            _mockProjectRepo.Setup(x => x.GetProjectByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(project);

            var service = CreateService();

            var result = await service.GetProjectDeadlineAsync("oidc-sub", project.ProjectId);

            Assert.True(result.Success);
            Assert.Equal("Project deadline retrieved successfully", result.Message);
            Assert.Equal(dueDate, result.Data);
        }
    }
}
