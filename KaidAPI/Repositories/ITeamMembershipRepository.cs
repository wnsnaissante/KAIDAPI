using KaidAPI.Models;

namespace KaidAPI.Repositories;

public interface ITeamMembershipRepository {
    Task<OperationResult> CreateTeamMembershipAsync(TeamMembership teamMembership);
}
