public class TeamRepository : ITeamRepository
{
    private readonly ServerDbContext _context;

    public TeamRepository(ServerDbContext context)
    {
        _context = context;
    }

    public async Task<OperationResult> CreateTeamAsync(Team team)
    {
        _context.Teams.Add(team);
        await _context.SaveChangesAsync();
        return new OperationResult(true, "Team created successfully");
    }

    public async Task<Team> GetTeamByTeamIdAsync(Guid teamId)
    {
        return await _context.Teams.FindAsync(teamId);
    }

    public async Task<List<Team>> GetTeamsByProjectIdAsync(Guid projectId)
    {
        return await _context.Teams.Where(t => t.ProjectId == projectId).ToListAsync();
    }

    public async Task<List<Team>> GetTeamByLeaderUserOidcAsync(string oidcSub)
    {
        return await _context.Teams.Where(t => t.LeaderId == oidcSub).ToListAsync();
    }

    public async Task<List<Team>> GetTeamByUserOidcAsync(string oidcSub)
    {
        var user = await GetUserByOidcAsync(oidcSub);
        if (user == null)
        {
            return new List<Team>();
        }

        // Find all teams that the user is a member of through odicsub
    }

    public async Task<OperationResult> DeleteTeamAsync(Guid teamId)
    {
        var team = await _context.Teams.FindAsync(teamId);
        if (team == null)
        {
            return new OperationResult(false, "Team not found");
        }
        _context.Teams.Remove(team);
        await _context.SaveChangesAsync();
        return new OperationResult(true, "Team deleted successfully");
    }

    public async Task<OperationResult> UpdateTeamAsync(Guid teamId, Team team)  
    {
        var existingTeam = await _context.Teams.FindAsync(teamId);
        if (existingTeam == null)
        {
            return new OperationResult(false, "Team not found");    
        }
        existingTeam.TeamName = team.TeamName;
        existingTeam.Description = team.Description;
        existingTeam.LeaderId = team.LeaderId;
        existingTeam.ProjectId = team.ProjectId;
        await _context.SaveChangesAsync();
        return new OperationResult(true, "Team updated successfully");
        }
}
