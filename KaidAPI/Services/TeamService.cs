using KaidAPI.Models;
using KaidAPI.Repositories;
using KaidAPI.ViewModel;

public class TeamService : ITeamService {
    private readonly ITeamRepository _teamRepository;
    private readonly IUserRepository _userRepository;
    private readonly IProjectRepository _projectRepository;

    public TeamService(ITeamRepository teamRepository, IUserRepository userRepository, IProjectRepository projectRepository) {
        _teamRepository = teamRepository;
        _userRepository = userRepository;
        _projectRepository = projectRepository;
    }

    public async Task<Guid> CreateTeamAsync(TeamRequest request) {
        var team = new Team {
            TeamId = Guid.NewGuid(),
            TeamName = request.TeamName,
            Description = request.Description,
            ProjectId = request.ProjectId,
            LeaderId = request.LeaderId.Value
        };

        await _teamRepository.CreateTeamAsync(team);
        return team.TeamId;
    }

    public async Task<OperationResult> DeleteTeamAsync(Guid requesterId, Guid teamId)
    {
        var team = await _teamRepository.GetTeamByTeamIdAsync(teamId);
        if (team == null)
        {
            return new OperationResult
            {
                Success = false,
                Message = "Team not found",
            };
        }

        var project = await _projectRepository.GetProjectByIdAsync(team.ProjectId);
        if (team.LeaderId != requesterId && project.OwnerId != requesterId)
        {
            return new OperationResult
            {
                Success = false,
                Message = "You do not have access to this team",
            };
        }

        return await _teamRepository.DeleteTeamAsync(teamId);
    }

    public async Task<OperationResult> UpdateTeamAsync(Guid requesterId, Guid teamId, TeamRequest request)
    {
        var existingTeam = await _teamRepository.GetTeamByTeamIdAsync(teamId);
        if (existingTeam == null)
        {
            return new OperationResult
            {
                Success = false,
                Message = "Team not found"
            };
        }
        
        existingTeam.LeaderId = request.LeaderId.Value;
        existingTeam.ProjectId = request.ProjectId;
        existingTeam.TeamName = request.TeamName;
        existingTeam.Description = request.Description;
        
        return await _teamRepository.UpdateTeamAsync(existingTeam);
    }
}
