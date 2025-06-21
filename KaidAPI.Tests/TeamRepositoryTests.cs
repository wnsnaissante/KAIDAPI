using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KaidAPI.Context;
using KaidAPI.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace KaidAPI.Tests
{
    public class TeamRepositoryTests : IDisposable
    {
        private readonly ServerDbContext _context;
        private readonly TeamRepository _repository;

        public TeamRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ServerDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ServerDbContext(options);
            _repository = new TeamRepository(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task CreateTeamAsync_ShouldAddTeamAndReturnSuccess()
        {
            var team = new Team
            {
                TeamId = Guid.NewGuid(),
                ProjectId = Guid.NewGuid(),
                TeamName = "Test Team",
                Description = "Desc",
                LeaderId = Guid.NewGuid()
            };

            var result = await _repository.CreateTeamAsync(team);

            Assert.True(result.Success);
            Assert.Equal("Team created successfully", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(team.TeamId, ((Team)result.Data).TeamId);

            var teamInDb = await _context.Teams.FindAsync(team.TeamId);
            Assert.NotNull(teamInDb);
            Assert.Equal(team.TeamName, teamInDb.TeamName);
        }

        [Fact]
        public async Task GetTeamByTeamIdAsync_ShouldReturnTeam()
        {
            var team = new Team
            {
                TeamId = Guid.NewGuid(),
                ProjectId = Guid.NewGuid(),
                TeamName = "Test Team 2",
                Description = "Desc",
                LeaderId = Guid.NewGuid()
            };
            _context.Teams.Add(team);
            await _context.SaveChangesAsync();

            var fetchedTeam = await _repository.GetTeamByTeamIdAsync(team.TeamId);

            Assert.NotNull(fetchedTeam);
            Assert.Equal(team.TeamName, fetchedTeam.TeamName);
        }

        [Fact]
        public async Task GetTeamsByProjectIdAsync_ShouldReturnTeamsList()
        {
            var projectId = Guid.NewGuid();

            var team1 = new Team { TeamId = Guid.NewGuid(), ProjectId = projectId, TeamName = "Team1", Description = "D1", LeaderId = Guid.NewGuid() };
            var team2 = new Team { TeamId = Guid.NewGuid(), ProjectId = projectId, TeamName = "Team2", Description = "D2", LeaderId = Guid.NewGuid() };
            var team3 = new Team { TeamId = Guid.NewGuid(), ProjectId = Guid.NewGuid(), TeamName = "OtherProjectTeam", Description = "D3", LeaderId = Guid.NewGuid() };

            _context.Teams.AddRange(team1, team2, team3);
            await _context.SaveChangesAsync();

            var teams = await _repository.GetTeamsByProjectIdAsync(projectId);

            Assert.NotNull(teams);
            Assert.Equal(2, teams.Count);
            Assert.All(teams, t => Assert.Equal(projectId, t.ProjectId));
        }

        [Fact]
        public async Task DeleteTeamAsync_ShouldRemoveTeam_WhenTeamExists()
        {
            var team = new Team { TeamId = Guid.NewGuid(), ProjectId = Guid.NewGuid(), TeamName = "ToDelete", Description = "Desc", LeaderId = Guid.NewGuid() };
            _context.Teams.Add(team);
            await _context.SaveChangesAsync();

            var result = await _repository.DeleteTeamAsync(team.TeamId);

            Assert.True(result.Success);
            Assert.Equal("Team deleted successfully", result.Message);

            var deleted = await _context.Teams.FindAsync(team.TeamId);
            Assert.Null(deleted);
        }

        [Fact]
        public async Task DeleteTeamAsync_ShouldReturnFail_WhenTeamNotFound()
        {
            var result = await _repository.DeleteTeamAsync(Guid.NewGuid());

            Assert.False(result.Success);
            Assert.Equal("Team not found", result.Message);
        }

        [Fact]
        public async Task UpdateTeamAsync_ShouldUpdateTeamAndReturnSuccess()
        {
            var team = new Team { TeamId = Guid.NewGuid(), ProjectId = Guid.NewGuid(), TeamName = "OldName", Description = "OldDesc", LeaderId = Guid.NewGuid() };
            _context.Teams.Add(team);
            await _context.SaveChangesAsync();

            team.TeamName = "NewName";
            team.Description = "NewDesc";

            var result = await _repository.UpdateTeamAsync(team);

            Assert.True(result.Success);
            Assert.Equal("Team updated successfully", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal("NewName", ((Team)result.Data).TeamName);

            var updatedTeam = await _context.Teams.FindAsync(team.TeamId);
            Assert.Equal("NewName", updatedTeam.TeamName);
            Assert.Equal("NewDesc", updatedTeam.Description);
        }
    }
}
