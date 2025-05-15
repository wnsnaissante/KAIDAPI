public interface ITeamService {
    Task<Guid> CreateTeamAsync(TeamRequest request);
    Task<List<Team>> GetTeamsByUserOidcAsync(string oidcSub);
    Task<OperationResult> UpdateTeamAsync(Guid teamId, TeamRequest request);
    Task<OperationResult> DeleteTeamAsync(Guid teamId);
}


