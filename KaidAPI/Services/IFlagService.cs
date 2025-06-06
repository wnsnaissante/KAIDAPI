using KaidAPI.Models;
using KaidAPI.ViewModel;

namespace KaidAPI.Services;

public interface IFlagService
{
    Task<Guid> CreateFlagAsync(String oidcSub, FlagRequest flagRequest);

    Task<OperationResult> UpdateFlagAsync(string oidcSub, Guid flagId, FlagRequest flagRequest);
    Task<OperationResult> DeleteFlagAsync(string oidcSub, Guid flagId);
    Task<OperationResult> GetFlagByIdAsync(string oidcSub, Guid flagId);
    Task<OperationResult> GetFlagsByProjectAsync(string oidcSub, Guid projectId);
    Task<OperationResult> GetRaisedFlagsCountAsync(string oidcSub, Guid projectId);
    Task<OperationResult> GetSolvedFlagsCountAsync(string oidcSub, Guid projectId);
    Task<OperationResult> GetUnsolvedFlagsCountAsync(string oidcSub, Guid projectId);
}
