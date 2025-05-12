public interface ITeamMembershipRepository {
    Task<OperationResult> CreateTeamMembershipAsync(TeamMembership teamMembership);
}
