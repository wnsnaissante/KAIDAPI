using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KaidAPI.Models;
using KaidAPI.Repositories;
using KaidAPI.Services;
using KaidAPI.ViewModel.Tasks;
using KaidAPI.ViewModel;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class ProjectTaskServiceTests
{
    private readonly Mock<IProjectTaskRepository> _taskRepoMock = new();
    private readonly Mock<IProjectRepository> _projectRepoMock = new();
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IMembershipRepository> _membershipRepoMock = new();
    private readonly Mock<ITeamRepository> _teamRepoMock = new();
    private readonly Mock<ILogger<ProjectTaskService>> _loggerMock = new();

    private ProjectTaskService CreateService() =>
        new(_taskRepoMock.Object, _projectRepoMock.Object, _userRepoMock.Object,
            _membershipRepoMock.Object, _teamRepoMock.Object, _loggerMock.Object);

    [Fact]
    public async Task CreateProjectTaskAsync_TeamDoesNotExist_ReturnsEmptyGuid()
    {
        _teamRepoMock.Setup(t => t.GetTeamByTeamIdAsync(It.IsAny<Guid>())).ReturnsAsync((Team?)null);
        var service = CreateService();

        var result = await service.CreateProjectTaskAsync(new ProjectTaskRequest { TeamId = Guid.NewGuid() });

        Assert.Equal(Guid.Empty, result);
    }

    [Fact]
    public async Task CreateProjectTaskAsync_TeamExists_ReturnsCreatedGuid()
    {
        var teamId = Guid.NewGuid();
        _teamRepoMock.Setup(t => t.GetTeamByTeamIdAsync(teamId)).ReturnsAsync(new Team { TeamId = teamId, Description = "desc" });
        _taskRepoMock.Setup(t => t.CreateProjectTaskAsync(It.IsAny<ProjectTaskRequest>())).ReturnsAsync(Guid.NewGuid());

        var service = CreateService();

        var result = await service.CreateProjectTaskAsync(new ProjectTaskRequest { TeamId = teamId });

        Assert.NotEqual(Guid.Empty, result);
    }

    [Fact]
    public async Task UpdateProjectTaskAsync_UserNotAuthorized_ReturnsFailure()
    {
        var task = new ProjectTask { ProjectId = Guid.NewGuid() };
        _userRepoMock.Setup(u => u.GetUserByOidcAsync(It.IsAny<string>())).ReturnsAsync(new User { UserId = Guid.NewGuid() });
        _projectRepoMock.Setup(p => p.GetProjectByIdAsync(task.ProjectId)).ReturnsAsync(new Project { OwnerId = Guid.NewGuid() }); // 다른 owner

        var service = CreateService();

        var result = await service.UpdateProjectTaskAsync(task, "oidc-sub");

        Assert.False(result.Success);
        Assert.Equal("No permission for this project", result.Message);
    }

    [Fact]
    public async Task UpdateProjectTaskAsync_UserAuthorized_UpdatesTaskSuccessfully()
    {
        var userId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var task = new ProjectTask { ProjectId = projectId };

        _userRepoMock.Setup(u => u.GetUserByOidcAsync("oidc-sub")).ReturnsAsync(new User { UserId = userId });
        _projectRepoMock.Setup(p => p.GetProjectByIdAsync(projectId)).ReturnsAsync(new Project { OwnerId = userId });
        _taskRepoMock.Setup(t => t.UpdateProjectTaskAsync(task)).Returns(Task.CompletedTask);

        var service = CreateService();

        var result = await service.UpdateProjectTaskAsync(task, "oidc-sub");

        Assert.True(result.Success);
        Assert.Equal("Task updated successfully", result.Message);
    }

    [Fact]
    public async Task DeleteProjectTaskAsync_TaskNotFound_ReturnsFailure()
    {
        _taskRepoMock.Setup(t => t.GetProjectTaskByIdAsync(It.IsAny<Guid>())).ReturnsAsync((ProjectTask?)null);

        var service = CreateService();

        var result = await service.DeleteProjectTaskAsync(Guid.NewGuid(), "oidc-sub");

        Assert.False(result.Success);
        Assert.Equal("Task not found", result.Message);
    }

    [Fact]
    public async Task DeleteProjectTaskAsync_UserNotAuthorized_ReturnsFailure()
    {
        var task = new ProjectTask { ProjectId = Guid.NewGuid() };
        _taskRepoMock.Setup(t => t.GetProjectTaskByIdAsync(task.TaskId)).ReturnsAsync(task);
        _userRepoMock.Setup(u => u.GetUserByOidcAsync(It.IsAny<string>())).ReturnsAsync(new User { UserId = Guid.NewGuid() });
        _projectRepoMock.Setup(p => p.GetProjectByIdAsync(task.ProjectId)).ReturnsAsync(new Project { OwnerId = Guid.NewGuid() }); // 다른 owner

        var service = CreateService();

        var result = await service.DeleteProjectTaskAsync(task.TaskId, "oidc-sub");

        Assert.False(result.Success);
        Assert.Equal("No permission for this project", result.Message);
    }

    [Fact]
    public async Task DeleteProjectTaskAsync_UserAuthorized_DeletesSuccessfully()
    {
        var userId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var task = new ProjectTask { TaskId = taskId, ProjectId = projectId };

        _taskRepoMock.Setup(t => t.GetProjectTaskByIdAsync(taskId)).ReturnsAsync(task);
        _userRepoMock.Setup(u => u.GetUserByOidcAsync("oidc-sub")).ReturnsAsync(new User { UserId = userId });
        _projectRepoMock.Setup(p => p.GetProjectByIdAsync(projectId)).ReturnsAsync(new Project { OwnerId = userId });
        _taskRepoMock.Setup(t => t.DeleteProjectTaskAsync(taskId)).Returns(Task.CompletedTask);

        var service = CreateService();

        var result = await service.DeleteProjectTaskAsync(taskId, "oidc-sub");

        Assert.True(result.Success);
        Assert.Equal("Task deleted successfully", result.Message);
    }
}
