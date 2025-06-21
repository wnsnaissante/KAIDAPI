using KaidAPI.Models;
using KaidAPI.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;
using KaidAPI.Context;

namespace KaidAPI.Tests.Repositories
{
    public class ProjectTaskRepositoryTests
    {
        private DbContextOptions<ServerDbContext> CreateNewInMemoryOptions()
        {
            return new DbContextOptionsBuilder<ServerDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public async Task GetProjectTaskByIdAsync_ShouldReturnTask()
        {
            var options = CreateNewInMemoryOptions();
            var taskId = Guid.NewGuid();

            using (var context = new ServerDbContext(options))
            {
                var task = new ProjectTask
                {
                    TaskId = taskId,
                    TaskName = "Test Task",
                    TaskDescription = "Description",
                    ProjectId = Guid.NewGuid(),
                    Assignee = Guid.NewGuid(),
                    DueDate = DateTime.UtcNow.AddDays(7),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    StatusId = 1,
                    Priority = 2
                };
                await context.ProjectTasks.AddAsync(task);
                await context.SaveChangesAsync();
            }

            using (var context = new ServerDbContext(options))
            {
                var repository = new ProjectTaskRepository(context);
                var result = await repository.GetProjectTaskByIdAsync(taskId);

                Assert.NotNull(result);
                Assert.Equal("Test Task", result.TaskName);
            }
        }

        [Fact]
        public async Task UpdateProjectTaskAsync_ShouldUpdateExistingTask()
        {
            var options = CreateNewInMemoryOptions();
            var taskId = Guid.NewGuid();

            using (var context = new ServerDbContext(options))
            {
                var task = new ProjectTask
                {
                    TaskId = taskId,
                    TaskName = "Old Name",
                    TaskDescription = "Old Description",
                    ProjectId = Guid.NewGuid(),
                    Assignee = Guid.NewGuid(),
                    DueDate = DateTime.UtcNow.AddDays(7),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    StatusId = 1,
                    Priority = 1
                };
                await context.ProjectTasks.AddAsync(task);
                await context.SaveChangesAsync();
            }

            using (var context = new ServerDbContext(options))
            {
                var repository = new ProjectTaskRepository(context);
                var taskToUpdate = await repository.GetProjectTaskByIdAsync(taskId);
                taskToUpdate.TaskName = "New Name";

                await repository.UpdateProjectTaskAsync(taskToUpdate);
                var updatedTask = await repository.GetProjectTaskByIdAsync(taskId);

                Assert.Equal("New Name", updatedTask.TaskName);
            }
        }

        [Fact]
        public async Task DeleteProjectTaskAsync_ShouldRemoveTask()
        {
            var options = CreateNewInMemoryOptions();
            var taskId = Guid.NewGuid();

            using (var context = new ServerDbContext(options))
            {
                var task = new ProjectTask
                {
                    TaskId = taskId,
                    TaskName = "Task To Delete",
                    TaskDescription = "Description",
                    ProjectId = Guid.NewGuid(),
                    Assignee = Guid.NewGuid(),
                    DueDate = DateTime.UtcNow.AddDays(3),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    StatusId = 2,
                    Priority = 3
                };
                await context.ProjectTasks.AddAsync(task);
                await context.SaveChangesAsync();
            }

            using (var context = new ServerDbContext(options))
            {
                var repository = new ProjectTaskRepository(context);
                await repository.DeleteProjectTaskAsync(taskId);
                var deleted = await repository.GetProjectTaskByIdAsync(taskId);
                Assert.Null(deleted);
            }
        }

        [Fact]
        public async Task GetProjectTasksByTeamIdAsync_ShouldReturnTasks()
        {
            var options = CreateNewInMemoryOptions();
            var teamId = Guid.NewGuid();

            using (var context = new ServerDbContext(options))
            {
                var user = new User
                {
                    UserId = Guid.NewGuid(),
                    Email = "leader@example.com",
                    Username = "Leader"
                };

                var team = new Team
                {
                    TeamId = teamId,
                    TeamName = "QA Team",
                    Leader = user,
                    Description = "Test Description"
                };

                var task = new ProjectTask
                {
                    TaskId = Guid.NewGuid(),
                    TaskName = "Team Task",
                    TaskDescription = "Team task description",
                    ProjectId = Guid.NewGuid(),
                    TeamId = team.TeamId,
                    Team = team,
                    Assignee = Guid.NewGuid(),
                    DueDate = DateTime.UtcNow.AddDays(5),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    StatusId = 1,
                    Priority = 1
                };

                await context.Users.AddAsync(user);
                await context.Teams.AddAsync(team);
                await context.ProjectTasks.AddAsync(task);
                await context.SaveChangesAsync();
            }

            using (var context = new ServerDbContext(options))
            {
                var repository = new ProjectTaskRepository(context);
                var tasks = await repository.GetProjectTasksByTeamIdAsync(teamId);

                Assert.Single(tasks);
                Assert.Equal("Team Task", tasks.First().TaskName);
            }
        }
    }
}