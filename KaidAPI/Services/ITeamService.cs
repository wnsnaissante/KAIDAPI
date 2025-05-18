using KaidAPI.Models;
using KaidAPI.ViewModel;

public interface ITeamService {
    Task<Guid> CreateTeamAsync(TeamRequest request);
    Task<OperationResult> UpdateTeamAsync(Guid requesterId, Guid teamId, TeamRequest request);
    Task<OperationResult> DeleteTeamAsync(Guid requesterId, Guid teamId);
}


