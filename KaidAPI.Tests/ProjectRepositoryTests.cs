using System;
using System.Threading.Tasks;
using KaidAPI.Context;
using KaidAPI.Models;
using KaidAPI.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace KaidAPI.Tests.Repositories
{
    public class ProjectRepositoryTests
    {
        private ServerDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ServerDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new ServerDbContext(options);
        }

        [Fact]
        public async Task CreateProjectAsync_ShouldAddProject()
        {
            var context = GetInMemoryDbContext();
            var repo = new ProjectRepository(context);

            var project = new Project
            {
                ProjectId = Guid.NewGuid(),
                ProjectName = "Test Project",
                ProjectDescription = "Description",
                OwnerId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(10)
            };

            var id = await repo.CreateProjectAsync(project);

            var savedProject = await context.Projects.FindAsync(id);

            Assert.NotNull(savedProject);
            Assert.Equal("Test Project", savedProject.ProjectName);
        }

        [Fact]
        public async Task GetProjectByIdAsync_ShouldReturnProject()
        {
            var context = GetInMemoryDbContext();
            var repo = new ProjectRepository(context);

            var project = new Project
            {
                ProjectId = Guid.NewGuid(),
                ProjectName = "Get Test",
                ProjectDescription = "Desc",
                OwnerId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow
            };

            await context.Projects.AddAsync(project);
            await context.SaveChangesAsync();

            var result = await repo.GetProjectByIdAsync(project.ProjectId);

            Assert.NotNull(result);
            Assert.Equal("Get Test", result.ProjectName);
        }

        [Fact]
        public async Task UpdateProjectAsync_ShouldModifyProject()
        {
            var context = GetInMemoryDbContext();
            var repo = new ProjectRepository(context);

            var project = new Project
            {
                ProjectId = Guid.NewGuid(),
                ProjectName = "Before Update",
                ProjectDescription = "Desc",
                OwnerId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow
            };

            await context.Projects.AddAsync(project);
            await context.SaveChangesAsync();

            project.ProjectName = "After Update";
            project.ProjectDescription = "Updated Desc";
            project.DueDate = DateTime.UtcNow.AddDays(5);

            await repo.UpdateProjectAsync(project);

            var updatedProject = await context.Projects.FindAsync(project.ProjectId);

            Assert.Equal("After Update", updatedProject.ProjectName);
            Assert.Equal("Updated Desc", updatedProject.ProjectDescription);
            Assert.NotNull(updatedProject.DueDate);
        }

        [Fact]
        public async Task DeleteProjectAsync_ShouldRemoveProject()
        {
            var context = GetInMemoryDbContext();
            var repo = new ProjectRepository(context);

            var project = new Project
            {
                ProjectId = Guid.NewGuid(),
                ProjectName = "To Be Deleted",
                ProjectDescription = "Desc",
                OwnerId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow
            };

            await context.Projects.AddAsync(project);
            await context.SaveChangesAsync();

            await repo.DeleteProjectAsync(project.ProjectId);

            var deletedProject = await context.Projects.FindAsync(project.ProjectId);

            Assert.Null(deletedProject);
        }
    }
}
