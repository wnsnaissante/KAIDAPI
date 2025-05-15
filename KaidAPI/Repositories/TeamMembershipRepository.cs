public class TeamMembershipRepository : ITeamMembershipRepository
{
    private readonly ServerDbContext _context;

    public TeamMembershipRepository(ServerDbContext context) {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<OperationResult> CreateTeamMembershipAsync(TeamMembership teamMembership) {
        _context.TeamMemberships.Add(teamMembership);
        await _context.SaveChangesAsync();
        return new OperationResult(true, "Team membership created successfully");
    }

}
