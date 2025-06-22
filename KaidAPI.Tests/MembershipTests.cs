using System;
using System.Linq;
using System.Threading.Tasks;
using KaidAPI.Context;
using KaidAPI.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class MembershipTests
{
    private async Task<ServerDbContext> CreateInMemoryDbContextAsync()
    {
        var options = new DbContextOptionsBuilder<ServerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new ServerDbContext(options);

        context.Memberships.Add(new Membership
        {
            ProjectMembershipId = Guid.NewGuid(),
            ProjectId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            SuperiorId = null,
            TeamId = null,
            IsActivated = true,
            Status = "Active"
        });

        await context.SaveChangesAsync();
        return context;
    }

    [Fact]
    public async Task Should_Read_Membership_With_Null_SuperiorId_And_TeamId()
    {
        // Arrange
        var context = await CreateInMemoryDbContextAsync();

        // Act
        var membership = await context.Memberships.FirstOrDefaultAsync(m => m.SuperiorId == null && m.TeamId == null);

        // Assert
        Assert.NotNull(membership);
        Assert.Null(membership.SuperiorId);
        Assert.Null(membership.TeamId);
    }
}
