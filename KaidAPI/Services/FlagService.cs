namespace KaidAPI.Services;

public class FlagService : IFlagService
{
    private readonly IFlagRepository _flagRepository;

    public FlagService(IFlagRepository flagRepository)
    {
        _flagRepository = flagRepository ?? throw new ArgumentNullException(nameof(flagRepository));
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

    public async Task<OperationResult> GetFlagByIdAsync(Guid flagId)
    {
        var flag = await _flagRepository.GetFlagByFlagIdAsync(flagId);
        if (flag == null)
        {
            return new OperationResult { Success = false, Message = "Flag not found" };
        }
        return new OperationResult { Success = true, Message = "Flag retrieved successfully", Data = flag };
    }

    public async Task<OperationResult> UpdateFlagAsync(Guid flagId, FlagRequest flagRequest)
    {
        var flag = await _flagRepository.GetFlagByFlagIdAsync(flagId);
        var user = await _userRepository.GetUserByOidcAsync(oidcSub);
        if (flag == null)
        {
            return new OperationResult { Success = false, Message = "Flag not found" };
        }

        if (user.Role != "Project Manager" && user.UserId != flag.OwnerId   )
        {
            return new OperationResult { Success = false, Message = "You are not authorized to update this flag" };
        }

        flag.Status = flagRequest.Status;
        flag.Priority = flagRequest.Priority;
        
        await _flagRepository.UpdateFlagAsync(flag);
        return new OperationResult { Success = true, Message = "Flag updated successfully" };
        
    }
    
}
