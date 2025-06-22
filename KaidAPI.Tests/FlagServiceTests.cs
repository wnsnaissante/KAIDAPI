using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KaidAPI.Models;
using KaidAPI.Repositories;
using KaidAPI.Services;
using KaidAPI.ViewModel;
using Moq;
using Xunit;

public class FlagServiceTests
{
    private readonly Mock<IFlagRepository> _flagRepoMock = new();
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IMembershipRepository> _membershipRepoMock = new();

    private FlagService CreateService()
    {
        return new FlagService(_flagRepoMock.Object, _userRepoMock.Object, _membershipRepoMock.Object);
    }

    [Fact]
    public async Task CreateFlagAsync_ShouldReturnNewGuid()
    {
        // Arrange
        var oidcSub = "user-oidc";
        var userId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var teamId = Guid.NewGuid();
        var superiorId = Guid.NewGuid();

        var flagRequest = new FlagRequest
        {
            ProjectId = projectId,
            FlagDescription = "Test Flag",
            TeamId = teamId,
        };

        _userRepoMock.Setup(x => x.GetUserByOidcAsync(oidcSub))
            .ReturnsAsync(new User { UserId = userId });

        _membershipRepoMock.Setup(x => x.GetMembershipByProjectIdAndUserIdAsync(projectId, userId))
            .ReturnsAsync(new Membership { SuperiorId = superiorId });

        _flagRepoMock.Setup(x => x.CreateFlagAsync(It.IsAny<Flag>()))
            .ReturnsAsync(Guid.NewGuid());

        var service = CreateService();

        // Act
        var result = await service.CreateFlagAsync(oidcSub, flagRequest);

        // Assert
        Assert.IsType<Guid>(result);
        _flagRepoMock.Verify(x => x.CreateFlagAsync(It.IsAny<Flag>()), Times.Once);
    }

    [Fact]
    public async Task GetFlagByIdAsync_FlagNotFound_ShouldReturnFailure()
    {
        // Arrange
        var oidcSub = "oidc";
        var flagId = Guid.NewGuid();

        _flagRepoMock.Setup(x => x.GetFlagByFlagIdAsync(flagId))
            .ReturnsAsync((Flag)null);

        var service = CreateService();

        // Act
        var result = await service.GetFlagByIdAsync(oidcSub, flagId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Flag not found", result.Message);
    }

    [Fact]
    public async Task GetFlagByIdAsync_UserNotFound_ShouldReturnFailure()
    {
        var oidcSub = "oidc";
        var flagId = Guid.NewGuid();

        _flagRepoMock.Setup(x => x.GetFlagByFlagIdAsync(flagId))
            .ReturnsAsync(new Flag { OwnerId = Guid.NewGuid() });

        _userRepoMock.Setup(x => x.GetUserByOidcAsync(oidcSub))
            .ReturnsAsync((User)null);

        var service = CreateService();

        var result = await service.GetFlagByIdAsync(oidcSub, flagId);

        Assert.False(result.Success);
        Assert.Equal("User not found", result.Message);
    }

    [Fact]
    public async Task GetFlagByIdAsync_AccessDenied_ShouldReturnFailure()
    {
        var oidcSub = "oidc";
        var userId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var flagId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        var flag = new Flag
        {
            OwnerId = ownerId,
            ProjectId = projectId
        };

        var user = new User { UserId = userId };

        var ownerMembership = new Membership { RoleId = 3 };
        var userMembership = new Membership { RoleId = 3 };

        _flagRepoMock.Setup(x => x.GetFlagByFlagIdAsync(flagId)).ReturnsAsync(flag);
        _userRepoMock.Setup(x => x.GetUserByOidcAsync(oidcSub)).ReturnsAsync(user);
        _membershipRepoMock.Setup(x => x.GetMembershipByProjectIdAndUserIdAsync(projectId, ownerId)).ReturnsAsync(ownerMembership);
        _membershipRepoMock.Setup(x => x.GetMembershipByProjectIdAndUserIdAsync(projectId, userId)).ReturnsAsync(userMembership);

        var service = CreateService();

        var result = await service.GetFlagByIdAsync(oidcSub, flagId);

        Assert.False(result.Success);
        Assert.Equal("Access denied", result.Message);
    }

    [Fact]
    public async Task UpdateFlagAsync_FlagNotFound_ShouldReturnFailure()
    {
        var oidcSub = "oidc";
        var flagId = Guid.NewGuid();
        var flagRequest = new FlagRequest();

        _flagRepoMock.Setup(x => x.GetFlagByFlagIdAsync(flagId))
            .ReturnsAsync((Flag)null);

        var service = CreateService();

        var result = await service.UpdateFlagAsync(oidcSub, flagId, flagRequest);

        Assert.False(result.Success);
        Assert.Equal("Flag not found", result.Message);
    }

    [Fact]
    public async Task UpdateFlagAsync_UserNotFound_ShouldReturnFailure()
    {
        var oidcSub = "oidc";
        var flagId = Guid.NewGuid();
        var flagRequest = new FlagRequest();

        _flagRepoMock.Setup(x => x.GetFlagByFlagIdAsync(flagId))
            .ReturnsAsync(new Flag { OwnerId = Guid.NewGuid() });

        _userRepoMock.Setup(x => x.GetUserByOidcAsync(oidcSub))
            .ReturnsAsync((User)null);

        var service = CreateService();

        var result = await service.UpdateFlagAsync(oidcSub, flagId, flagRequest);

        Assert.False(result.Success);
        Assert.Equal("User not found", result.Message);
    }

    [Fact]
    public async Task UpdateFlagAsync_UnauthorizedUser_ShouldReturnFailure()
    {
        var oidcSub = "oidc";
        var flagId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();
        var flagRequest = new FlagRequest { Status = FlagStatus.Solved, Priority = 5, FlagDescription = "Desc" };
        var projectId = Guid.NewGuid();

        var flag = new Flag { OwnerId = ownerId, ProjectId = projectId };
        var currentUser = new User { UserId = currentUserId };
        var ownerMembership = new Membership { RoleId = 2 };
        var currentMembership = new Membership { RoleId = 2 };

        _flagRepoMock.Setup(x => x.GetFlagByFlagIdAsync(flagId)).ReturnsAsync(flag);
        _userRepoMock.Setup(x => x.GetUserByOidcAsync(oidcSub)).ReturnsAsync(currentUser);
        _membershipRepoMock.Setup(x => x.GetMembershipByProjectIdAndUserIdAsync(projectId, ownerId)).ReturnsAsync(ownerMembership);
        _membershipRepoMock.Setup(x => x.GetMembershipByProjectIdAndUserIdAsync(projectId, currentUserId)).ReturnsAsync(currentMembership);

        var service = CreateService();

        var result = await service.UpdateFlagAsync(oidcSub, flagId, flagRequest);

        Assert.False(result.Success);
        Assert.Equal("You are not authorized to update this flag", result.Message);
    }

    [Fact]
    public async Task DeleteFlagAsync_ShouldCallRepositoryDelete()
    {
        var oidcSub = "oidc";
        var flagId = Guid.NewGuid();

        _flagRepoMock.Setup(x => x.GetFlagByFlagIdAsync(flagId)).ReturnsAsync(new Flag());
        _userRepoMock.Setup(x => x.GetUserByOidcAsync(oidcSub)).ReturnsAsync(new User());

        _flagRepoMock.Setup(x => x.DeleteFlagAsync(flagId)).Returns(Task.CompletedTask).Verifiable();

        var service = CreateService();

        var result = await service.DeleteFlagAsync(oidcSub, flagId);

        Assert.True(result.Success);
        Assert.Equal("Flag deleted successfully", result.Message);
        _flagRepoMock.Verify(x => x.DeleteFlagAsync(flagId), Times.Once);
    }

    [Fact]
    public async Task GetFlagsByProjectAsync_ShouldReturnAccessibleFlags()
    {
        var oidcSub = "oidc";
        var projectId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();

        var flags = new List<Flag>
        {
            new Flag { OwnerId = userId, ProjectId = projectId },
            new Flag { OwnerId = ownerId, ProjectId = projectId }
        };

        var user = new User { UserId = userId };
        var ownerMembership = new Membership { RoleId = 3 };
        var userMembership = new Membership { RoleId = 1 }; // admin

        _userRepoMock.Setup(x => x.GetUserByOidcAsync(oidcSub)).ReturnsAsync(user);
        _flagRepoMock.Setup(x => x.GetFlagsByProjectIdAsync(projectId)).ReturnsAsync(flags);
        _membershipRepoMock.Setup(x => x.GetMembershipByProjectIdAndUserIdAsync(projectId, ownerId)).ReturnsAsync(ownerMembership);
        _membershipRepoMock.Setup(x => x.GetMembershipByProjectIdAndUserIdAsync(projectId, userId)).ReturnsAsync(userMembership);

        var service = CreateService();

        var result = await service.GetFlagsByProjectAsync(oidcSub, projectId);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        var accessibleFlags = Assert.IsAssignableFrom<List<Flag>>(result.Data);
        Assert.Equal(2, accessibleFlags.Count);
    }
}
