using KaidAPI.Context;
using KaidAPI.Models;
using KaidAPI.Repositories;
using Microsoft.EntityFrameworkCore;

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
        return new OperationResult
        {
            Success = true,
            Message = "Team created successfully",
            Data = team
        };
    }

    public async Task<Team> GetTeamByTeamIdAsync(Guid teamId)
    {
        return await _context.Teams.FindAsync(teamId);
    }

    public async Task<List<Team>> GetTeamsByProjectIdAsync(Guid projectId)
    {
        return await _context.Teams.Where(t => t.ProjectId == projectId).ToListAsync();
    }

    public async Task<OperationResult> DeleteTeamAsync(Guid teamId)
    {
        var team = await _context.Teams.FindAsync(teamId);
        if (team == null)
        {
            return new OperationResult
            {
                Success = false,
                Message = "Team not found"
            };
        }

        _context.Teams.Remove(team);
        await _context.SaveChangesAsync();
        return new OperationResult
        {
            Success = true,
            Message = "Team deleted successfully"
        };
    }

    public async Task<OperationResult> UpdateTeamAsync(Team team)
    {
        _context.Teams.Update(team);
        await _context.SaveChangesAsync();

        return new OperationResult
        {
            Success = true,
            Message = "Team updated successfully",
            Data = team
        };
    }
}