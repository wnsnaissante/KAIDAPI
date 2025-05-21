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
        {
            return new OperationResult { Success = false, Message = "Flag not found" };
        }
        return new OperationResult { Success = true, Message = "Flag retrieved successfully", Data = flag };
    }

    public async Task<OperationResult> UpdateFlagAsync(string oidcSub, Guid flagId, FlagRequest flagRequest)
    {
        var flag = await _flagRepository.GetFlagByFlagIdAsync(flagId);
        var user = await _userRepository.GetUserByOidcAsync(oidcSub);
        if (user == null)
        {
            return new OperationResult { Success = false, Message = "User not found" };
        }

        var member = await _membershipRepository.GetMembershipByProjectIdAndUserIdAsync(flagRequest.ProjectId, user.UserId);
        if (member.RoleId != 1) // !TODO: Implement superior can update too
        {
            return new OperationResult { Success = false, Message = "You can't update this flag" };
        }
        flag.Status = flagRequest.Status;
        flag.Priority = flagRequest.Priority;
        
        await _flagRepository.UpdateFlagAsync(flag);
        return new OperationResult { Success = true, Message = "Flag updated successfully" };
        
    }

    public async Task<OperationResult> DeleteFlagAsync(string oidcSub, Guid flagId)
    {
        var flag = await _flagRepository.GetFlagByFlagIdAsync(flagId);
        var user = await _userRepository.GetUserByOidcAsync(oidcSub);

        return new OperationResult();
    }
}
