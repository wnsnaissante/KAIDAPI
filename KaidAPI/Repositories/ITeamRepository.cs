using KaidAPI.Models;

namespace KaidAPI.Repositories;

public interface ITeamRepository
{
    Task<OperationResult> CreateTeamAsync(Team team);
    Task<Team> GetTeamByTeamIdAsync(Guid teamId);
    Task<List<Team>> GetTeamsByProjectIdAsync(Guid projectId);
    Task<OperationResult> DeleteTeamAsync(Guid teamId);
    Task<OperationResult> UpdateTeamAsync(Team team);
}
