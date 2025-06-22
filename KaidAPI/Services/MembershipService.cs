using KaidAPI.Models;
using KaidAPI.Repositories;

namespace KaidAPI.Services;

public class MembershipService : IMembershipService
{
    private readonly IMembershipRepository _membershipRepository;
    private readonly IUserRepository _userRepository;
    
    public MembershipService(IMembershipRepository membershipRepository, IUserRepository userRepository)
    {
        _membershipRepository = membershipRepository;
        _userRepository =  userRepository;
    }

    public async Task<OperationResult> CreateMembershipAsync(string oidcSub, MemberRequest memberRequest)
    {
        var user = await _userRepository.GetUserByOidcAsync(oidcSub);
        if (user == null) {
            return new OperationResult
            {
                Success = false,
                Message = "User not found"
            };
        }

        var newMember = new Membership
        {
            ProjectMembershipId = Guid.NewGuid(),
            ProjectId = memberRequest.ProjectId,
            UserId = memberRequest.UserId,
            JoinedAt = DateTime.UtcNow,
            RoleId = memberRequest.RoleId,
            SuperiorId = memberRequest.SuperiorId ?? memberRequest.UserId, // 자기 자신을 SuperiorId로 설정
        };
        
        await _membershipRepository.CreateMembershipAsync(newMember);
        return new OperationResult
        {
            Success = true,
            Message = "Membership created successfully"
        };
    }

    public async Task<OperationResult> DeleteMembershipAsync(string oidcSub, Guid membershipId) {
        var user = await _userRepository.GetUserByOidcAsync(oidcSub);
        if (user == null) {
            return new OperationResult
            {
                Success = false,
                Message = "User not found"
            };
        }

        var membership = await _membershipRepository.GetMembershipByMembershipIdAsync(membershipId);
        if (membership == null) {
            return new OperationResult
            {
                Success = false,
                Message = "Membership not found"
            };
        }

        if (membership.UserId != user.UserId || membership.RoleId != 1) {
            return new OperationResult
            {
                Success = false,
                Message = "You are not authorized to delete this membership"
            };
        }

        return await _membershipRepository.DeleteMembershipAsync(membershipId); 
    }

    public async Task<OperationResult> GetUserRoleByProjectIdAndUserIdAsync(string oidcSub, Guid projectId) {
        var user = await _userRepository.GetUserByOidcAsync(oidcSub);
        if (user == null) {
            return new OperationResult
            {
                Success = false,
                Message = "User not found"
            };
        }

        var membership = await _membershipRepository.GetMembershipByProjectIdAndUserIdAsync(projectId, user.UserId);
        if (membership == null) {
            return new OperationResult
            {
                Success = false,
                Message = "Membership not found"
            };
        }

        return new OperationResult
        {
            Success = true,
            Message = "Membership found",
            Data = membership.Role
        };
    }

    public async Task<OperationResult> GetMembersAsync(string oidcSub, Guid projectId, Guid teamId)
    {
        var user = await _userRepository.GetUserByOidcAsync(oidcSub);
        if (user == null)
        {
            return new OperationResult
            {
                Success = false,
                Message = "User not found"
            };
        }

        var allMemberships = await _membershipRepository.GetAllMembershipsAsync();

        var matchedMemberships = allMemberships
            .Where(m => m.ProjectId == projectId && m.TeamId == teamId)
            .ToList();

        return new OperationResult
        {
            Success = true,
            Message = "Success to get members",
            Data = matchedMemberships
        }; 
    }

    public async Task<OperationResult> GetMembershipAsync(string oidcSub, Guid projectId, Guid teamId, Guid userId)
    {
        var user = await _userRepository.GetUserByOidcAsync(oidcSub);
        if (user == null)
            return new OperationResult { Success = false, Message = "User not found" };

        var allMemberships = await _membershipRepository.GetAllMembershipsAsync();
        var matchedMemberships = allMemberships.Where(m =>
            m.ProjectId == projectId &&
            m.TeamId == teamId &&
            m.UserId == userId
        ).ToList();

        if (matchedMemberships.Count == 0)
        {
            return new OperationResult
            {
                Success = false,
                Message = "Membership not found"
            };
        }

        return new OperationResult
        {
            Success = true,
            Message = "Success to find Membership",
            Data = matchedMemberships
        };
    }

    public async Task<OperationResult> UpdateMembershipAsync(string oidcSub, Guid membershipId, MemberRequest memberRequest)
    {
        var user = await _userRepository.GetUserByOidcAsync(oidcSub);
        if (user == null)
        {
            return new OperationResult
            {
                Success = false,
                Message = "User not found"
            };
        }

        var userMembership = await _membershipRepository.GetMembershipByMembershipIdAsync(membershipId);
        if (userMembership.RoleId != 1) 
        {
            return new OperationResult
            {
                Success = false,
                Message = "Access denied"
            };
        }

        var newMember = new Membership
        {
            ProjectMembershipId = membershipId,
            TeamId = memberRequest.TeamId,
            UserId = memberRequest.UserId,
            SuperiorId = memberRequest.SuperiorId,
            RoleId = memberRequest.RoleId,
            IsActivated = memberRequest.IsActivated,
            Status = memberRequest.Status
        };

        await _membershipRepository.UpdateMembershipAsync(membershipId, newMember);
        return new OperationResult
        {
            Success = true,
            Message = "Membership has been edited successfully"
        };
    }

    public async Task<OperationResult> AcceptInvitationAsync(string oidcSub, Guid membershipId)
    {
        var user = await _userRepository.GetUserByOidcAsync(oidcSub);
        if (user == null)
        {
            return new OperationResult
            {
                Success = false,
                Message = "User not found"
            };
        }

        var userMembership = await _membershipRepository.GetMembershipByMembershipIdAsync(membershipId);
        if (userMembership.UserId != user.UserId)
        {
            return new OperationResult
            {
                Success = false,
                Message = "Access denied"
            };
        }

        userMembership.IsActivated = true;

        await _membershipRepository.UpdateMembershipAsync(membershipId, userMembership);
        return new OperationResult
        {
            Success = true,
            Message = "Invitation Acceptance Successful"
        };
    }

    public async Task<OperationResult> DenyInvitationAsync(string oidcSub, Guid membershipId)
    {
        var user = await _userRepository.GetUserByOidcAsync(oidcSub);
        if (user == null)
        {
            return new OperationResult
            {
                Success = false,
                Message = "User not found"
            };
        }

        var userMembership = await _membershipRepository.GetMembershipByMembershipIdAsync(membershipId);
        if (userMembership.UserId != user.UserId)
        {
            return new OperationResult
            {
                Success = false,
                Message = "Access denied"
            };
        }

        userMembership.IsActivated = true;

        await _membershipRepository.DeleteMembershipAsync(membershipId);
        return new OperationResult
        {
            Success = true,
            Message = "Successfully rejected invitation"
        };
    }
}