using Xunit;
using Moq;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using KaidAPI.Models;
using KaidAPI.Repositories;
using KaidAPI.Services;

public class MembershipServiceTests
{
    private readonly Mock<IMembershipRepository> _membershipRepoMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly MembershipService _service;

    public MembershipServiceTests()
    {
        _membershipRepoMock = new Mock<IMembershipRepository>();
        _userRepoMock = new Mock<IUserRepository>();
        _service = new MembershipService(_membershipRepoMock.Object, _userRepoMock.Object);
    }

    private User GetTestUser(Guid userId) =>
        new User { UserId = userId };

    private Membership GetTestMembership(Guid userId, Guid membershipId, int roleId = 1) =>
        new Membership { ProjectMembershipId = membershipId, UserId = userId, RoleId = roleId, IsActivated = false };

    [Fact]
    public async Task CreateMembershipAsync_UserNotFound_ReturnsFailure()
    {
        _userRepoMock.Setup(x => x.GetUserByOidcAsync(It.IsAny<string>()))
                     .ReturnsAsync((User)null);

        var result = await _service.CreateMembershipAsync("oidc123", new MemberRequest());

        Assert.False(result.Success);
        Assert.Equal("User not found", result.Message);
    }

    [Fact]
    public async Task CreateMembershipAsync_ValidUser_CreatesMembership()
    {
        var userId = Guid.NewGuid();
        var oidcSub = "oidc123";

        _userRepoMock.Setup(x => x.GetUserByOidcAsync(oidcSub))
                     .ReturnsAsync(GetTestUser(userId));

        _membershipRepoMock.Setup(x => x.CreateMembershipAsync(It.IsAny<Membership>()))
                           .ReturnsAsync(new OperationResult { Success = true });

        var memberRequest = new MemberRequest
        {
            ProjectId = Guid.NewGuid(),
            UserId = userId,
            RoleId = 1
        };

        var result = await _service.CreateMembershipAsync(oidcSub, memberRequest);

        Assert.True(result.Success);
        _membershipRepoMock.Verify(x => x.CreateMembershipAsync(It.Is<Membership>(m =>
            m.UserId == memberRequest.UserId &&
            m.ProjectId == memberRequest.ProjectId)), Times.Once);
    }

    [Fact]
    public async Task DeleteMembershipAsync_UserNotFound_ReturnsFailure()
    {
        _userRepoMock.Setup(x => x.GetUserByOidcAsync(It.IsAny<string>()))
                     .ReturnsAsync((User)null);

        var result = await _service.DeleteMembershipAsync("oidc123", Guid.NewGuid());

        Assert.False(result.Success);
        Assert.Equal("User not found", result.Message);
    }

    [Fact]
    public async Task DeleteMembershipAsync_MembershipNotFound_ReturnsFailure()
    {
        var user = GetTestUser(Guid.NewGuid());
        var oidcSub = "oidc123";

        _userRepoMock.Setup(x => x.GetUserByOidcAsync(oidcSub))
                     .ReturnsAsync(user);

        _membershipRepoMock.Setup(x => x.GetMembershipByMembershipIdAsync(It.IsAny<Guid>()))
                           .ReturnsAsync((Membership)null);

        var result = await _service.DeleteMembershipAsync(oidcSub, Guid.NewGuid());

        Assert.False(result.Success);
        Assert.Equal("Membership not found", result.Message);
    }

    [Fact]
    public async Task DeleteMembershipAsync_UserNotAuthorized_ReturnsFailure()
    {
        var user = GetTestUser(Guid.NewGuid());
        var membership = GetTestMembership(Guid.NewGuid(), Guid.NewGuid(), roleId: 2); // roleId != 1
        var oidcSub = "oidc123";

        _userRepoMock.Setup(x => x.GetUserByOidcAsync(oidcSub))
                     .ReturnsAsync(user);
        _membershipRepoMock.Setup(x => x.GetMembershipByMembershipIdAsync(It.IsAny<Guid>()))
                           .ReturnsAsync(membership);

        var result = await _service.DeleteMembershipAsync(oidcSub, membership.ProjectMembershipId);

        Assert.False(result.Success);
        Assert.Equal("You are not authorized to delete this membership", result.Message);
    }

    [Fact]
    public async Task DeleteMembershipAsync_AuthorizedUser_DeletesMembership()
    {
        var userId = Guid.NewGuid();
        var user = GetTestUser(userId);
        var membership = GetTestMembership(userId, Guid.NewGuid(), roleId: 1);
        var oidcSub = "oidc123";

        _userRepoMock.Setup(x => x.GetUserByOidcAsync(oidcSub))
                     .ReturnsAsync(user);
        _membershipRepoMock.Setup(x => x.GetMembershipByMembershipIdAsync(It.IsAny<Guid>()))
                           .ReturnsAsync(membership);
        _membershipRepoMock.Setup(x => x.DeleteMembershipAsync(It.IsAny<Guid>()))
                           .ReturnsAsync(new OperationResult { Success = true });

        var result = await _service.DeleteMembershipAsync(oidcSub, membership.ProjectMembershipId);

        Assert.True(result.Success);
        _membershipRepoMock.Verify(x => x.DeleteMembershipAsync(membership.ProjectMembershipId), Times.Once);
    }

    [Fact]
    public async Task GetUserRoleByProjectIdAndUserIdAsync_UserNotFound_ReturnsFailure()
    {
        _userRepoMock.Setup(x => x.GetUserByOidcAsync(It.IsAny<string>()))
                     .ReturnsAsync((User)null);

        var result = await _service.GetUserRoleByProjectIdAndUserIdAsync("oidc123", Guid.NewGuid());

        Assert.False(result.Success);
        Assert.Equal("User not found", result.Message);
    }

    [Fact]
    public async Task GetUserRoleByProjectIdAndUserIdAsync_MembershipNotFound_ReturnsFailure()
    {
        var user = GetTestUser(Guid.NewGuid());
        var oidcSub = "oidc123";

        _userRepoMock.Setup(x => x.GetUserByOidcAsync(oidcSub))
                     .ReturnsAsync(user);
        _membershipRepoMock.Setup(x => x.GetMembershipByProjectIdAndUserIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                           .ReturnsAsync((Membership)null);

        var result = await _service.GetUserRoleByProjectIdAndUserIdAsync(oidcSub, Guid.NewGuid());

        Assert.False(result.Success);
        Assert.Equal("Membership not found", result.Message);
    }

    [Fact]
    public async Task GetUserRoleByProjectIdAndUserIdAsync_ReturnsRole()
    {
        var userId = Guid.NewGuid();
        var user = GetTestUser(userId);
        var membership = GetTestMembership(userId, Guid.NewGuid());
        membership.Role = new Role { RoleId = 1, RoleName = "Admin" };
        var oidcSub = "oidc123";

        _userRepoMock.Setup(x => x.GetUserByOidcAsync(oidcSub))
                     .ReturnsAsync(user);
        _membershipRepoMock.Setup(x => x.GetMembershipByProjectIdAndUserIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                           .ReturnsAsync(membership);

        var result = await _service.GetUserRoleByProjectIdAndUserIdAsync(oidcSub, Guid.NewGuid());

        Assert.True(result.Success);
        Assert.Equal("Membership found", result.Message);
        Assert.NotNull(result.Data);
        Assert.IsType<Role>(result.Data);
        Assert.Equal("Admin", ((Role)result.Data).RoleName);
    }

    [Fact]
    public async Task GetMembersAsync_UserNotFound_ReturnsFailure()
    {
        _userRepoMock.Setup(x => x.GetUserByOidcAsync(It.IsAny<string>()))
                     .ReturnsAsync((User)null);

        var result = await _service.GetMembersAsync("oidc123", Guid.NewGuid(), Guid.NewGuid());

        Assert.False(result.Success);
        Assert.Equal("User not found", result.Message);
    }

    [Fact]
    public async Task GetMembersAsync_ReturnsMatchedMemberships()
    {
        var user = GetTestUser(Guid.NewGuid());
        var projectId = Guid.NewGuid();
        var teamId = Guid.NewGuid();
        var oidcSub = "oidc123";

        var memberships = new List<Membership>
        {
            new Membership { ProjectMembershipId = Guid.NewGuid(), ProjectId = projectId, TeamId = teamId },
            new Membership { ProjectMembershipId = Guid.NewGuid(), ProjectId = projectId, TeamId = teamId },
            new Membership { ProjectMembershipId = Guid.NewGuid(), ProjectId = Guid.NewGuid(), TeamId = Guid.NewGuid() }
        };

        _userRepoMock.Setup(x => x.GetUserByOidcAsync(oidcSub))
                     .ReturnsAsync(user);

        _membershipRepoMock.Setup(x => x.GetAllMembershipsAsync())
                           .ReturnsAsync(memberships);

        var result = await _service.GetMembersAsync(oidcSub, projectId, teamId);

        Assert.True(result.Success);
        Assert.Equal("Success to get members", result.Message);
        Assert.Equal(2, ((List<Membership>)result.Data).Count);
    }

    [Fact]
    public async Task GetMembershipAsync_UserNotFound_ReturnsFailure()
    {
        _userRepoMock.Setup(x => x.GetUserByOidcAsync(It.IsAny<string>()))
                     .ReturnsAsync((User)null);

        var result = await _service.GetMembershipAsync("oidc123", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        Assert.False(result.Success);
        Assert.Equal("User not found", result.Message);
    }

    [Fact]
    public async Task GetMembershipAsync_MembershipNotFound_ReturnsFailure()
    {
        var user = GetTestUser(Guid.NewGuid());
        var oidcSub = "oidc123";

        _userRepoMock.Setup(x => x.GetUserByOidcAsync(oidcSub))
                     .ReturnsAsync(user);

        _membershipRepoMock.Setup(x => x.GetAllMembershipsAsync())
                           .ReturnsAsync(new List<Membership>());

        var result = await _service.GetMembershipAsync(oidcSub, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        Assert.False(result.Success);
        Assert.Equal("Membership not found", result.Message);
    }

    [Fact]
    public async Task GetMembershipAsync_ReturnsMembership()
    {
        var user = GetTestUser(Guid.NewGuid());
        var projectId = Guid.NewGuid();
        var teamId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var oidcSub = "oidc123";

        var membership = new Membership
        {
            ProjectMembershipId = Guid.NewGuid(),
            ProjectId = projectId,
            TeamId = teamId,
            UserId = userId
        };

        _userRepoMock.Setup(x => x.GetUserByOidcAsync(oidcSub))
                     .ReturnsAsync(user);
        _membershipRepoMock.Setup(x => x.GetAllMembershipsAsync())
                           .ReturnsAsync(new List<Membership> { membership });

        var result = await _service.GetMembershipAsync(oidcSub, projectId, teamId, userId);

        Assert.True(result.Success);
        Assert.Equal("Success to find Membership", result.Message);
        var data = Assert.IsAssignableFrom<IEnumerable<Membership>>(result.Data);
        Assert.Contains(data, m => m.ProjectMembershipId == membership.ProjectMembershipId);
    }

    [Fact]
    public async Task UpdateMembershipAsync_UserNotFound_ReturnsFailure()
    {
        _userRepoMock.Setup(x => x.GetUserByOidcAsync(It.IsAny<string>()))
                     .ReturnsAsync((User)null);

        var result = await _service.UpdateMembershipAsync("oidc123", Guid.NewGuid(), new MemberRequest());

        Assert.False(result.Success);
        Assert.Equal("User not found", result.Message);
    }

    [Fact]
    public async Task UpdateMembershipAsync_UserNotAuthorized_ReturnsAccessDenied()
    {
        var user = GetTestUser(Guid.NewGuid());
        var membership = GetTestMembership(user.UserId, Guid.NewGuid(), roleId: 2); // not roleId=1 (admin)
        var oidcSub = "oidc123";

        _userRepoMock.Setup(x => x.GetUserByOidcAsync(oidcSub)).ReturnsAsync(user);
        _membershipRepoMock.Setup(x => x.GetMembershipByMembershipIdAsync(It.IsAny<Guid>())).ReturnsAsync(membership);

        var result = await _service.UpdateMembershipAsync(oidcSub, membership.ProjectMembershipId, new MemberRequest());

        Assert.False(result.Success);
        Assert.Equal("Access denied", result.Message);
    }

    [Fact]
    public async Task UpdateMembershipAsync_Success()
    {
        var user = GetTestUser(Guid.NewGuid());
        var membershipId = Guid.NewGuid();
        var membership = GetTestMembership(user.UserId, membershipId, roleId: 1);
        var oidcSub = "oidc123";

        _userRepoMock.Setup(x => x.GetUserByOidcAsync(oidcSub)).ReturnsAsync(user);
        _membershipRepoMock.Setup(x => x.GetMembershipByMembershipIdAsync(membershipId)).ReturnsAsync(membership);
        _membershipRepoMock.Setup(x => x.UpdateMembershipAsync(membershipId, It.IsAny<Membership>()))
                           .Returns(Task.CompletedTask);

        var memberRequest = new MemberRequest
        {
            TeamId = Guid.NewGuid(),
            UserId = user.UserId,
            SuperiorId = Guid.NewGuid(),
            RoleId = 1,
            IsActivated = true,
            Status = "Updated"
        };

        var result = await _service.UpdateMembershipAsync(oidcSub, membershipId, memberRequest);

        Assert.True(result.Success);
        Assert.Equal("Membership has been edited successfully", result.Message);
        _membershipRepoMock.Verify(x => x.UpdateMembershipAsync(membershipId, It.IsAny<Membership>()), Times.Once);
    }

    [Fact]
    public async Task AcceptInvitationAsync_UserNotFound_ReturnsFailure()
    {
        _userRepoMock.Setup(x => x.GetUserByOidcAsync(It.IsAny<string>())).ReturnsAsync((User)null);

        var result = await _service.AcceptInvitationAsync("oidc123", Guid.NewGuid());

        Assert.False(result.Success);
        Assert.Equal("User not found", result.Message);
    }

    [Fact]
    public async Task AcceptInvitationAsync_UserIdMismatch_ReturnsAccessDenied()
    {
        var user = GetTestUser(Guid.NewGuid());
        var membership = GetTestMembership(Guid.NewGuid(), Guid.NewGuid()); // 다른 UserId
        var oidcSub = "oidc123";

        _userRepoMock.Setup(x => x.GetUserByOidcAsync(oidcSub)).ReturnsAsync(user);
        _membershipRepoMock.Setup(x => x.GetMembershipByMembershipIdAsync(It.IsAny<Guid>())).ReturnsAsync(membership);

        var result = await _service.AcceptInvitationAsync(oidcSub, membership.ProjectMembershipId);

        Assert.False(result.Success);
        Assert.Equal("Access denied", result.Message);
    }

    [Fact]
    public async Task AcceptInvitationAsync_Success()
    {
        var userId = Guid.NewGuid();
        var user = GetTestUser(userId);
        var membership = GetTestMembership(userId, Guid.NewGuid());
        var oidcSub = "oidc123";

        _userRepoMock.Setup(x => x.GetUserByOidcAsync(oidcSub)).ReturnsAsync(user);
        _membershipRepoMock.Setup(x => x.GetMembershipByMembershipIdAsync(It.IsAny<Guid>())).ReturnsAsync(membership);
        _membershipRepoMock.Setup(x => x.UpdateMembershipAsync(It.IsAny<Guid>(), It.IsAny<Membership>())).Returns(Task.CompletedTask);

        var result = await _service.AcceptInvitationAsync(oidcSub, membership.ProjectMembershipId);

        Assert.True(result.Success);
        Assert.Equal("Invitation Acceptance Successful", result.Message);
        _membershipRepoMock.Verify(x => x.UpdateMembershipAsync(membership.ProjectMembershipId, It.Is<Membership>(m => m.IsActivated)), Times.Once);
    }

    [Fact]
    public async Task DenyInvitationAsync_UserNotFound_ReturnsFailure()
    {
        _userRepoMock.Setup(x => x.GetUserByOidcAsync(It.IsAny<string>())).ReturnsAsync((User)null);

        var result = await _service.DenyInvitationAsync("oidc123", Guid.NewGuid());

        Assert.False(result.Success);
        Assert.Equal("User not found", result.Message);
    }

    [Fact]
    public async Task DenyInvitationAsync_UserIdMismatch_ReturnsAccessDenied()
    {
        var user = GetTestUser(Guid.NewGuid());
        var membership = GetTestMembership(Guid.NewGuid(), Guid.NewGuid()); // 다른 UserId
        var oidcSub = "oidc123";

        _userRepoMock.Setup(x => x.GetUserByOidcAsync(oidcSub)).ReturnsAsync(user);
        _membershipRepoMock.Setup(x => x.GetMembershipByMembershipIdAsync(It.IsAny<Guid>())).ReturnsAsync(membership);

        var result = await _service.DenyInvitationAsync(oidcSub, membership.ProjectMembershipId);

        Assert.False(result.Success);
        Assert.Equal("Access denied", result.Message);
    }

    [Fact]
    public async Task DenyInvitationAsync_Success()
    {
        var userId = Guid.NewGuid();
        var user = GetTestUser(userId);
        var membership = GetTestMembership(userId, Guid.NewGuid());
        var oidcSub = "oidc123";

        _userRepoMock.Setup(x => x.GetUserByOidcAsync(oidcSub)).ReturnsAsync(user);
        _membershipRepoMock.Setup(x => x.GetMembershipByMembershipIdAsync(It.IsAny<Guid>())).ReturnsAsync(membership);
        _membershipRepoMock.Setup(x => x.DeleteMembershipAsync(It.IsAny<Guid>())).ReturnsAsync(new OperationResult { Success = true });

        var result = await _service.DenyInvitationAsync(oidcSub, membership.ProjectMembershipId);

        Assert.True(result.Success);
        Assert.Equal("Successfully rejected invitation", result.Message);
        _membershipRepoMock.Verify(x => x.DeleteMembershipAsync(membership.ProjectMembershipId), Times.Once);
    }
}
