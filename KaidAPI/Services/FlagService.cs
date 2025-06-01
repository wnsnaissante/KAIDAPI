using KaidAPI.Models;
using KaidAPI.Repositories;
using KaidAPI.ViewModel;

namespace KaidAPI.Services;

public class FlagService : IFlagService
{
    private readonly IFlagRepository _flagRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMembershipRepository _membershipRepository;
    
    public FlagService(IFlagRepository flagRepository, IUserRepository userRepository, IMembershipRepository membershipRepository)
    {
        _flagRepository = flagRepository ?? throw new ArgumentNullException(nameof(flagRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _membershipRepository = membershipRepository ?? throw new ArgumentNullException(nameof(membershipRepository));
    }

    public async Task<Guid> CreateFlagAsync(FlagRequest flagRequest)
    {
        var flag = new Flag
        {
            FlagId = Guid.NewGuid(),
            FlagDescription = flagRequest.FlagDescription,
            Status = flagRequest.Status,
            Reporter = flagRequest.Reporter,
            CreatedAt = DateTime.UtcNow,
            Priority = flagRequest.Priority
        };

        return await _flagRepository.CreateFlagAsync(flag);
    }

    public async Task<OperationResult> GetFlagByIdAsync(string oidcSub, Guid flagId)
    {
        var flag = await _flagRepository.GetFlagByFlagIdAsync(flagId);
        if (flag == null)
            return new OperationResult { Success = false, Message = "Flag not found" };

        var user = await _userRepository.GetUserByOidcAsync(oidcSub);
        if (user == null)
            return new OperationResult { Success = false, Message = "User not found" };

        if (flag.OwnerId == user.UserId)
            return new OperationResult { Success = true, Data = flag };

        var ownerMembership = await _membershipRepository.GetMembershipByProjectIdAndUserIdAsync(flag.ProjectId, flag.OwnerId);
        var userMembership = await _membershipRepository.GetMembershipByProjectIdAndUserIdAsync(flag.ProjectId, user.UserId);

        if (ownerMembership == null || userMembership == null)
            return new OperationResult { Success = false, Message = "Membership not found" };

        if (userMembership.RoleId == 1 || userMembership.RoleId < ownerMembership.RoleId)
            return new OperationResult { Success = true, Message = "Flag retrieved successfully", Data = flag };

        return new OperationResult { Success = false, Message = "Access denied" };
    }

    public async Task<OperationResult> UpdateFlagAsync(string oidcSub, Guid flagId, FlagRequest flagRequest)
    {
        var flag = await _flagRepository.GetFlagByFlagIdAsync(flagId);
        if (flag == null)
        {
            return new OperationResult { Success = false, Message = "Flag not found" };
        }

        var currentUser = await _userRepository.GetUserByOidcAsync(oidcSub);
        if (currentUser == null)
        {
            return new OperationResult { Success = false, Message = "User not found" };
        }

        if (flag.OwnerId != currentUser.UserId)
        {
            var ownerMembership = await _membershipRepository.GetMembershipByProjectIdAndUserIdAsync(flag.ProjectId, flag.OwnerId);
            var currentMembership = await _membershipRepository.GetMembershipByProjectIdAndUserIdAsync(flag.ProjectId, currentUser.UserId);

            if (ownerMembership == null || currentMembership == null)
            {
                return new OperationResult { Success = false, Message = "Membership not found" };
            }

            if (currentMembership.RoleId != 1 && ownerMembership.RoleId <= currentMembership.RoleId)
            {
                return new OperationResult { Success = false, Message = "You are not authorized to update this flag" };
            }
        }

        flag.Status = flagRequest.Status;
        flag.Priority = flagRequest.Priority;
        flag.FlagDescription = flagRequest.FlagDescription;
        flag.Reporter = flagRequest.Reporter;

        await _flagRepository.UpdateFlagAsync(flag);

        return new OperationResult { Success = true, Message = "Flag updated successfully" };
    }

    public async Task<OperationResult> DeleteFlagAsync(string oidcSub, Guid flagId)
    {
        var flag = await _flagRepository.GetFlagByFlagIdAsync(flagId);
        var user = await _userRepository.GetUserByOidcAsync(oidcSub);

        return new OperationResult();
    }

    public async Task<OperationResult> GetFlagsByProjectAsync(string oidcSub, Guid projectId)
    {
        var user = await _userRepository.GetUserByOidcAsync(oidcSub);
        if (user == null)
            return new OperationResult { Success = false, Message = "User not found" };

        var flags = await _flagRepository.GetFlagsByProjectIdAsync(projectId);
        var accessibleFlags = new List<Flag>();

        foreach (var flag in flags)
        {
            if (flag.OwnerId == user.UserId)
            {
                accessibleFlags.Add(flag);
                continue;
            }

            var ownerMembership = await _membershipRepository.GetMembershipByProjectIdAndUserIdAsync(flag.ProjectId, flag.OwnerId);
            var userMembership = await _membershipRepository.GetMembershipByProjectIdAndUserIdAsync(flag.ProjectId, user.UserId);

            if (userMembership.RoleId == 1 || userMembership.RoleId < ownerMembership.RoleId)
                accessibleFlags.Add(flag);
        }

        return new OperationResult { Success = true, Data = accessibleFlags };
    }

}
