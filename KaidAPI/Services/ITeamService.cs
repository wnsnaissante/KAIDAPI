using KaidAPI.Models;
using KaidAPI.ViewModel;

public interface ITeamService {
    Task<Guid> CreateTeamAsync(TeamRequest request);
    Task<OperationResult> UpdateTeamAsync(string oidcSub, Guid teamId, TeamRequest request);
    Task<OperationResult> DeleteTeamAsync(string oidcSub, Guid teamId);
    Task<OperationResult> GetTeamsByProjectId(string oidcSub, Guid projectId);
}


