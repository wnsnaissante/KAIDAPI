using Xunit;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using KaidAPI.Context;
using KaidAPI.Models;
using KaidAPI.Repositories;

public class MembershipRepositoryTests
{
    private ServerDbContext GetDbContextWithMockData()
    {
        var options = new DbContextOptionsBuilder<ServerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new ServerDbContext(options);

        // Mock 데이터 삽입
        context.Memberships.AddRange(new List<Membership>
        {
            new Membership
            {
                ProjectMembershipId = Guid.NewGuid(),
                UserId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                ProjectId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                IsActivated = true,
                Status = "Activated",
                RoleId = 1
            },
            new Membership
            {
                ProjectMembershipId = Guid.NewGuid(),
                UserId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                ProjectId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                IsActivated = false,
                Status = "Deactivated",
                RoleId = 2
            },
            new Membership
            {
                ProjectMembershipId = Guid.NewGuid(),
                UserId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                ProjectId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                IsActivated = true,
                Status = "Activated",
                RoleId = 1
            },
        });

        context.SaveChanges();

        return context;
    }

    [Fact]
    public async Task CreateMembershipAsync_AddsMembershipSuccessfully()
    {
        var context = GetDbContextWithMockData();
        var repository = new MembershipRepository(context);

        var membership = new Membership
        {
            ProjectMembershipId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            ProjectId = Guid.NewGuid(),
            IsActivated = true,
            Status = "Activated",
            RoleId = 1
        };

        var result = await repository.CreateMembershipAsync(membership);

        Assert.True(result.Success);
        Assert.Contains(context.Memberships, m => m.ProjectMembershipId == membership.ProjectMembershipId);
    }

    [Fact]
    public async Task GetActivatedMembershipsByUserIdAsync_ReturnsOnlyActivated()
    {
        var context = GetDbContextWithMockData();
        var repository = new MembershipRepository(context);

        var userId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

        var results = await repository.GetActivatedMembershipsByUserIdAsync(userId);

        Assert.All(results, m => Assert.True(m.IsActivated));
        Assert.All(results, m => Assert.Equal(userId, m.UserId));
    }

    [Fact]
    public async Task GetDeactivatedMembershipsByUserIdAsync_ReturnsOnlyDeactivated()
    {
        var context = GetDbContextWithMockData();
        var repository = new MembershipRepository(context);

        var userId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

        var results = await repository.GetDeactivatedMembershipsByUserIdAsync(userId);

        Assert.All(results, m => Assert.False(m.IsActivated));
        Assert.All(results, m => Assert.Equal(userId, m.UserId));
    }

    [Fact]
    public async Task GetMembershipByProjectIdAndUserIdAsync_ReturnsCorrectMembership()
    {
        var context = GetDbContextWithMockData();
        var repository = new MembershipRepository(context);

        var targetMembership = context.Memberships.First();

        var membership = await repository.GetMembershipByProjectIdAndUserIdAsync(targetMembership.ProjectId, targetMembership.UserId);

        Assert.NotNull(membership);
        Assert.Equal(targetMembership.ProjectMembershipId, membership.ProjectMembershipId);
    }

    [Fact]
    public async Task GetMembershipByMembershipIdAsync_ReturnsCorrectMembership()
    {
        var context = GetDbContextWithMockData();
        var repository = new MembershipRepository(context);

        var targetMembership = context.Memberships.First();

        var membership = await repository.GetMembershipByMembershipIdAsync(targetMembership.ProjectMembershipId);

        Assert.NotNull(membership);
        Assert.Equal(targetMembership.ProjectMembershipId, membership.ProjectMembershipId);
    }

    [Fact]
    public async Task GetMembershipsByUserIdAsync_ReturnsOnlyActivatedMemberships()
    {
        var context = GetDbContextWithMockData();
        var repository = new MembershipRepository(context);

        var userId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

        var memberships = await repository.GetMembershipsByUserIdAsync(userId);

        Assert.All(memberships, m => Assert.True(m.IsActivated));
        Assert.All(memberships, m => Assert.Equal(userId, m.UserId));
    }

    [Fact]
    public async Task DeleteMembershipAsync_DeletesMembershipSuccessfully()
    {
        var context = GetDbContextWithMockData();
        var repository = new MembershipRepository(context);

        var membershipToDelete = context.Memberships.First();

        var result = await repository.DeleteMembershipAsync(membershipToDelete.ProjectMembershipId);

        Assert.True(result.Success);
        Assert.DoesNotContain(context.Memberships, m => m.ProjectMembershipId == membershipToDelete.ProjectMembershipId);
    }

    [Fact]
    public async Task DeleteMembershipAsync_ReturnsFailureIfNotFound()
    {
        var context = GetDbContextWithMockData();
        var repository = new MembershipRepository(context);

        var nonExistentId = Guid.NewGuid();

        var result = await repository.DeleteMembershipAsync(nonExistentId);

        Assert.False(result.Success);
        Assert.Equal("Membership not found", result.Message);
    }

    [Fact]
    public async Task GetAllMembershipsAsync_ReturnsAllMemberships()
    {
        var context = GetDbContextWithMockData();
        var repository = new MembershipRepository(context);

        var memberships = await repository.GetAllMembershipsAsync();

        Assert.Equal(context.Memberships.Count(), memberships.Count());
    }

    [Fact]
    public async Task UpdateMembershipAsync_UpdatesExistingMembership()
    {
        var context = GetDbContextWithMockData();
        var repository = new MembershipRepository(context);

        var existingMembership = context.Memberships.First();

        var updatedMembership = new Membership
        {
            ProjectMembershipId = existingMembership.ProjectMembershipId,
            UserId = existingMembership.UserId,
            ProjectId = existingMembership.ProjectId,
            IsActivated = !existingMembership.IsActivated, // 상태 변경
            Status = "UpdatedStatus",
            RoleId = existingMembership.RoleId,
            TeamId = existingMembership.TeamId,
            SuperiorId = existingMembership.SuperiorId
        };

        await repository.UpdateMembershipAsync(existingMembership.ProjectMembershipId, updatedMembership);

        var reloaded = await repository.GetMembershipByMembershipIdAsync(existingMembership.ProjectMembershipId);

        Assert.NotNull(reloaded);
        Assert.Equal(updatedMembership.IsActivated, reloaded.IsActivated);
        Assert.Equal("UpdatedStatus", reloaded.Status);
    }

    [Fact]
    public async Task GetMembershipsWithNullNullableGuids_ShouldNotThrowAndReturnCorrectData()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ServerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new ServerDbContext(options);

        var userId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

        // SuperiorId와 TeamId를 null로 둔 Membership 추가
        context.Memberships.Add(new Membership
        {
            ProjectMembershipId = Guid.NewGuid(),
            UserId = userId,
            ProjectId = Guid.NewGuid(),
            IsActivated = true,
            Status = "Activated",
            RoleId = 1,
            SuperiorId = null,
            TeamId = null
        });

        await context.SaveChangesAsync();

        var repository = new MembershipRepository(context);

        // Act
        var memberships = await repository.GetMembershipsByUserIdAsync(userId);

        // Assert
        Assert.Single(memberships);
        var membership = memberships.First();
        Assert.Null(membership.SuperiorId);
        Assert.Null(membership.TeamId);
    }
}
