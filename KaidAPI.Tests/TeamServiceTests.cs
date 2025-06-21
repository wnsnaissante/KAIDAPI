using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KaidAPI.Models;
using KaidAPI.Repositories;
using KaidAPI.Services;
using KaidAPI.ViewModel;
using Moq;
using Xunit;

namespace KaidAPI.Tests
{
    public class TeamServiceTests
    {
        private readonly Mock<ITeamRepository> _mockTeamRepo;
        private readonly Mock<IUserRepository> _mockUserRepo;
        private readonly Mock<IProjectRepository> _mockProjectRepo;
        private readonly Mock<IMembershipRepository> _mockMembershipRepo;
        private readonly TeamService _teamService;

        public TeamServiceTests()
        {
            _mockTeamRepo = new Mock<ITeamRepository>();
            _mockUserRepo = new Mock<IUserRepository>();
            _mockProjectRepo = new Mock<IProjectRepository>();
            _mockMembershipRepo = new Mock<IMembershipRepository>();

            _teamService = new TeamService(
                _mockTeamRepo.Object,
                _mockUserRepo.Object,
                _mockProjectRepo.Object,
                _mockMembershipRepo.Object
            );
        }

        [Fact]
        public async Task CreateTeamAsync_ShouldReturnNewTeamId()
        {
            var request = new TeamRequest
            {
                TeamName = "New Team",
                Description = "Desc",
                ProjectId = Guid.NewGuid(),
                LeaderId = Guid.NewGuid()
            };

            _mockTeamRepo.Setup(r => r.CreateTeamAsync(It.IsAny<Team>()))
                .ReturnsAsync(new OperationResult { Success = true });

            var result = await _teamService.CreateTeamAsync(request);

            Assert.NotEqual(Guid.Empty, result);
            _mockTeamRepo.Verify(r => r.CreateTeamAsync(It.Is<Team>(
                t => t.TeamName == request.TeamName &&
                     t.Description == request.Description &&
                     t.ProjectId == request.ProjectId &&
                     t.LeaderId == request.LeaderId.Value)), Times.Once);
        }

        [Fact]
        public async Task DeleteTeamAsync_ShouldFail_WhenUserNotFound()
        {
            _mockUserRepo.Setup(r => r.GetUserByOidcAsync(It.IsAny<string>()))
                .ReturnsAsync((User)null);

            var result = await _teamService.DeleteTeamAsync("oidcSub", Guid.NewGuid());

            Assert.False(result.Success);
            Assert.Equal("User not found", result.Message);
        }

        [Fact]
        public async Task DeleteTeamAsync_ShouldFail_WhenTeamNotFound()
        {
            _mockUserRepo.Setup(r => r.GetUserByOidcAsync(It.IsAny<string>()))
                .ReturnsAsync(new User { UserId = Guid.NewGuid() });

            _mockTeamRepo.Setup(r => r.GetTeamByTeamIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Team)null);

            var result = await _teamService.DeleteTeamAsync("oidcSub", Guid.NewGuid());

            Assert.False(result.Success);
            Assert.Equal("Team not found", result.Message);
        }

        [Fact]
        public async Task DeleteTeamAsync_ShouldFail_WhenNoAccess()
        {
            var userId = Guid.NewGuid();
            var team = new Team { LeaderId = Guid.NewGuid(), ProjectId = Guid.NewGuid() };
            var project = new Project { OwnerId = Guid.NewGuid() };

            _mockUserRepo.Setup(r => r.GetUserByOidcAsync(It.IsAny<string>()))
                .ReturnsAsync(new User { UserId = userId });

            _mockTeamRepo.Setup(r => r.GetTeamByTeamIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(team);

            _mockProjectRepo.Setup(r => r.GetProjectByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(project);

            var result = await _teamService.DeleteTeamAsync("oidcSub", Guid.NewGuid());

            Assert.False(result.Success);
            Assert.Equal("You do not have access to this team", result.Message);
        }

        [Fact]
        public async Task DeleteTeamAsync_ShouldDelete_WhenAuthorized()
        {
            var userId = Guid.NewGuid();
            var team = new Team { LeaderId = userId, ProjectId = Guid.NewGuid() };
            var project = new Project { OwnerId = Guid.NewGuid() };

            _mockUserRepo.Setup(r => r.GetUserByOidcAsync(It.IsAny<string>()))
                .ReturnsAsync(new User { UserId = userId });

            _mockTeamRepo.Setup(r => r.GetTeamByTeamIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(team);

            _mockProjectRepo.Setup(r => r.GetProjectByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(project);

            _mockTeamRepo.Setup(r => r.DeleteTeamAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new OperationResult { Success = true, Message = "Deleted" });

            var result = await _teamService.DeleteTeamAsync("oidcSub", Guid.NewGuid());

            Assert.True(result.Success);
            Assert.Equal("Deleted", result.Message);
        }

        [Fact]
        public async Task UpdateTeamAsync_ShouldFail_WhenTeamNotFound()
        {
            _mockTeamRepo.Setup(r => r.GetTeamByTeamIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Team)null);

            var result = await _teamService.UpdateTeamAsync("oidcSub", Guid.NewGuid(), new TeamRequest());

            Assert.False(result.Success);
            Assert.Equal("Team not found", result.Message);
        }

        [Fact]
        public async Task UpdateTeamAsync_ShouldFail_WhenUserNotFound()
        {
            _mockTeamRepo.Setup(r => r.GetTeamByTeamIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Team());

            _mockUserRepo.Setup(r => r.GetUserByOidcAsync(It.IsAny<string>()))
                .ReturnsAsync((User)null);

            var result = await _teamService.UpdateTeamAsync("oidcSub", Guid.NewGuid(), new TeamRequest());

            Assert.False(result.Success);
            Assert.Equal("User not found", result.Message);
        }

        [Fact]
        public async Task UpdateTeamAsync_ShouldFail_WhenUserNotOwner()
        {
            var team = new Team { ProjectId = Guid.NewGuid() };
            var user = new User { UserId = Guid.NewGuid() };
            var project = new Project { OwnerId = Guid.NewGuid() };

            _mockTeamRepo.Setup(r => r.GetTeamByTeamIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(team);

            _mockUserRepo.Setup(r => r.GetUserByOidcAsync(It.IsAny<string>()))
                .ReturnsAsync(user);

            _mockProjectRepo.Setup(r => r.GetProjectByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(project);

            var result = await _teamService.UpdateTeamAsync("oidcSub", Guid.NewGuid(), new TeamRequest());

            Assert.False(result.Success);
            Assert.Equal("You are not authorized to update this team", result.Message);
        }

        [Fact]
        public async Task UpdateTeamAsync_ShouldUpdateTeam_WhenAuthorized()
        {
            var team = new Team { TeamId = Guid.NewGuid(), ProjectId = Guid.NewGuid(), TeamName = "Old", Description = "OldDesc", LeaderId = Guid.NewGuid() };
            var user = new User { UserId = Guid.NewGuid() };
            var project = new Project { OwnerId = user.UserId };
            var request = new TeamRequest
            {
                TeamName = "New",
                Description = "NewDesc",
                LeaderId = Guid.NewGuid(),
                ProjectId = team.ProjectId
            };

            _mockTeamRepo.Setup(r => r.GetTeamByTeamIdAsync(team.TeamId))
                .ReturnsAsync(team);

            _mockUserRepo.Setup(r => r.GetUserByOidcAsync(It.IsAny<string>()))
                .ReturnsAsync(user);

            _mockProjectRepo.Setup(r => r.GetProjectByIdAsync(team.ProjectId))
                .ReturnsAsync(project);

            _mockTeamRepo.Setup(r => r.UpdateTeamAsync(It.IsAny<Team>()))
                .ReturnsAsync(new OperationResult { Success = true, Message = "Updated", Data = team });

            var result = await _teamService.UpdateTeamAsync("oidcSub", team.TeamId, request);

            Assert.True(result.Success);
            Assert.Equal("Updated", result.Message);
            Assert.Equal("New", ((Team)result.Data).TeamName);
        }

        [Fact]
        public async Task GetTeamsByProjectId_ShouldFail_WhenUserNotFound()
        {
            _mockUserRepo.Setup(r => r.GetUserByOidcAsync(It.IsAny<string>()))
                .ReturnsAsync((User)null);

            var result = await _teamService.GetTeamsByProjectId("oidcSub", Guid.NewGuid());

            Assert.False(result.Success);
            Assert.Equal("User not found", result.Message);
        }

        [Fact]
        public async Task GetTeamsByProjectId_ShouldFail_WhenUserNotProjectOwner()
        {
            var user = new User { UserId = Guid.NewGuid() };
            var project = new Project { OwnerId = Guid.NewGuid() };

            _mockUserRepo.Setup(r => r.GetUserByOidcAsync(It.IsAny<string>()))
                .ReturnsAsync(user);

            _mockProjectRepo.Setup(r => r.GetProjectByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(project);

            var result = await _teamService.GetTeamsByProjectId("oidcSub", Guid.NewGuid());

            Assert.False(result.Success);
            Assert.Equal("You do not have access to this team", result.Message);
        }

        [Fact]
        public async Task GetTeamsByProjectId_ShouldReturnTeams_WhenAuthorized()
        {
            var user = new User { UserId = Guid.NewGuid() };
            var projectId = Guid.NewGuid();
            var project = new Project { OwnerId = user.UserId };
            var teams = new List<Team>
            {
                new Team { TeamId = Guid.NewGuid(), ProjectId = projectId, TeamName = "Team1", Description = "Desc", LeaderId = Guid.NewGuid() }
            };

            _mockUserRepo.Setup(r => r.GetUserByOidcAsync(It.IsAny<string>()))
                .ReturnsAsync(user);

            _mockProjectRepo.Setup(r => r.GetProjectByIdAsync(projectId))
                .ReturnsAsync(project);

            _mockTeamRepo.Setup(r => r.GetTeamsByProjectIdAsync(projectId))
                .ReturnsAsync(teams);

            var result = await _teamService.GetTeamsByProjectId("oidcSub", projectId);

            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            var data = Assert.IsAssignableFrom<List<Team>>(result.Data);
            Assert.Single(data);
            Assert.Equal("Team1", data[0].TeamName);
        }
    }
}
