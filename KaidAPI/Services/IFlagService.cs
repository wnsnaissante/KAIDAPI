using KaidAPI.Models;
using KaidAPI.ViewModel;

namespace KaidAPI.Services;

public interface IFlagService
{
    Task<Guid> CreateFlagAsync(FlagRequest flagRequest);
    Task<OperationResult> GetFlagByIdAsync(string oidcSub, Guid flagId);

    Task<OperationResult> UpdateFlagAsync(string oidcSub, Guid flagId, FlagRequest flagRequest);
    Task<OperationResult> DeleteFlagAsync(string oidcSub, Guid flagId);
    Task<OperationResult> GetFlagByIdAsync(string oidcSub, Guid flagId);
    Task<OperationResult> GetFlagsByProjectAsync(string oidcSub, Guid projectId);
}
