using KaidAPI.Models;

namespace KaidAPI.Repositories;

public interface ITeamRepository
{
    Task<OperationResult> CreateTeamAsync(Team team);
    Task<Team> GetTeamByTeamIdAsync(Guid teamId);
    Task<List<Team>> GetTeamsByProjectIdAsync(Guid projectId);
    Task<List<Team>> GetTeamByLeaderUserOidcAsync(string oidcSub);
    Task<List<Team>> GetTeamByUserOidcAsync(string oidcSub);
    Task<OperationResult> DeleteTeamAsync(Guid teamId);
    Task<OperationResult> UpdateTeamAsync(Guid teamId, Team team);
}
