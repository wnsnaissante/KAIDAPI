using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using KaidAPI.Context;
using KaidAPI.Models;
using KaidAPI.Repositories;

public class FlagRepositoryTests
{
    private ServerDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ServerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ServerDbContext(options);
    }

    [Fact]
    public async Task CreateFlagAsync_ShouldAddFlagAndReturnId()
    {
        using var context = GetInMemoryDbContext();
        var repo = new FlagRepository(context);

        var flag = new Flag
        {
            FlagId = Guid.NewGuid(),
            ProjectId = Guid.NewGuid(),
            TeamId = Guid.NewGuid(),
            OwnerId = Guid.NewGuid(),
            FlagDescription = "Test flag",
            Status = FlagStatus.Todo,
            Reporter = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            Priority = 1
        };

        var result = await repo.CreateFlagAsync(flag);

        Assert.Equal(flag.FlagId, result);
        Assert.Single(context.Flags);
        Assert.Equal("Test flag", context.Flags.First().FlagDescription);
    }

    [Fact]
    public async Task GetFlagByFlagIdAsync_ShouldReturnCorrectFlag()
    {
        using var context = GetInMemoryDbContext();
        var repo = new FlagRepository(context);

        var flag = new Flag
        {
            FlagId = Guid.NewGuid(),
            ProjectId = Guid.NewGuid(),
            FlagDescription = "Find me"
        };
        context.Flags.Add(flag);
        await context.SaveChangesAsync();

        var foundFlag = await repo.GetFlagByFlagIdAsync(flag.FlagId);

        Assert.NotNull(foundFlag);
        Assert.Equal(flag.FlagId, foundFlag.FlagId);
        Assert.Equal("Find me", foundFlag.FlagDescription);
    }

    [Fact]
    public async Task UpdateFlagAsync_ShouldModifyExistingFlag()
    {
        using var context = GetInMemoryDbContext();
        var repo = new FlagRepository(context);

        var flag = new Flag
        {
            FlagId = Guid.NewGuid(),
            FlagDescription = "Original"
        };
        context.Flags.Add(flag);
        await context.SaveChangesAsync();

        flag.FlagDescription = "Updated";
        await repo.UpdateFlagAsync(flag);

        var updatedFlag = await context.Flags.FindAsync(flag.FlagId);
        Assert.Equal("Updated", updatedFlag.FlagDescription);
    }

    [Fact]
    public async Task DeleteFlagAsync_ShouldRemoveFlag()
    {
        using var context = GetInMemoryDbContext();
        var repo = new FlagRepository(context);

        var flag = new Flag
        {
            FlagId = Guid.NewGuid(),
            ProjectId = Guid.NewGuid(),
            TeamId = Guid.NewGuid(),
            OwnerId = Guid.NewGuid(),
            FlagDescription = "Test flag for deletion",  // 필수값 채우기
            Status = FlagStatus.Todo,
            Reporter = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            Priority = 1
        };
        context.Flags.Add(flag);
        await context.SaveChangesAsync();

        await repo.DeleteFlagAsync(flag.FlagId);

        var deletedFlag = await context.Flags.FindAsync(flag.FlagId);
        Assert.Null(deletedFlag);
    }


    [Fact]
    public async Task GetFlagsByProjectIdAsync_ShouldReturnFlagsForProject()
    {
        using var context = GetInMemoryDbContext();
        var repo = new FlagRepository(context);

        var projectId = Guid.NewGuid();

        var flags = new[]
        {
            new Flag { FlagId = Guid.NewGuid(), ProjectId = projectId, FlagDescription = "Flag 1" },
            new Flag { FlagId = Guid.NewGuid(), ProjectId = projectId, FlagDescription = "Flag 2" },
            new Flag { FlagId = Guid.NewGuid(), ProjectId = Guid.NewGuid(), FlagDescription = "Other Project" }
        };

        context.Flags.AddRange(flags);
        await context.SaveChangesAsync();

        var projectFlags = await repo.GetFlagsByProjectIdAsync(projectId);

        Assert.Equal(2, projectFlags.Count());
        Assert.All(projectFlags, f => Assert.Equal(projectId, f.ProjectId));
    }
}
