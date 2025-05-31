using KaidAPI.Models;
using KaidAPI.Repositories;
using KaidAPI.ViewModel;
using Microsoft.AspNetCore.Mvc;

public class TeamService : ITeamService {
    private readonly ITeamRepository _teamRepository;
    private readonly IUserRepository _userRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IMembershipRepository _membershipRepository;
    
    public TeamService(ITeamRepository teamRepository, IUserRepository userRepository, IProjectRepository projectRepository, IMembershipRepository membershipRepository) 
    {
        _teamRepository = teamRepository ?? throw new ArgumentNullException(nameof(teamRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
        _membershipRepository = membershipRepository ?? throw new ArgumentNullException(nameof(membershipRepository));
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

    public async Task<OperationResult> DeleteTeamAsync(string oidcSub, Guid teamId)
    {
        var user = await _userRepository.GetUserByOidcAsync(oidcSub);
        var team = await _teamRepository.GetTeamByTeamIdAsync(teamId);
        if (user == null) {
            return new OperationResult
            {
                Success = false,
                Message = "User not found"
            };
        }
        if (team == null)
        {
            return new OperationResult
            {
                Success = false,
                Message = "Team not found",
            };
        }

        var project = await _projectRepository.GetProjectByIdAsync(team.ProjectId);
        if (team.LeaderId != user.UserId && project.OwnerId != user.UserId)
        {
            return new OperationResult
            {
                Success = false,
                Message = "You do not have access to this team",
            };
        }

        return await _teamRepository.DeleteTeamAsync(teamId);
    }

    public async Task<OperationResult> UpdateTeamAsync(string oidcSub, Guid teamId, TeamRequest request)
    {
        var existingTeam = await _teamRepository.GetTeamByTeamIdAsync(teamId);
        var user = await _userRepository.GetUserByOidcAsync(oidcSub);
        if (existingTeam == null)
        {
            return new OperationResult
            {
                Success = false,
                Message = "Team not found"
            };
        }
        if (user == null) {
            return new OperationResult
            {
                Success = false,
                Message = "User not found"
            };
        }

        var project = await _projectRepository.GetProjectByIdAsync(existingTeam.ProjectId);
        if (project.OwnerId != user.UserId) {
            return new OperationResult
            {
                Success = false,
                Message = "You are not authorized to update this team"
            };
        }

        existingTeam.LeaderId = request.LeaderId.Value;
        existingTeam.ProjectId = request.ProjectId;
        existingTeam.TeamName = request.TeamName;
        existingTeam.Description = request.Description;
        
        return await _teamRepository.UpdateTeamAsync(existingTeam);
    }

    public async Task<OperationResult> GetTeamsByProjectId(string oidcSub, Guid projectId) {
        var user = await _userRepository.GetUserByOidcAsync(oidcSub);
        if (user == null) {
            return new OperationResult
            {
                Success = false,
                Message = "User not found"
            };
        }
        
        var membership = await _membershipRepository.GetMembershipByProjectIdAndUserIdAsync(projectId, user.UserId);
        if (membership == null && membership.RoleId != 1 && membership.RoleId != 2)
        {
            return new OperationResult
            {
                Success = false,
                Message = "You do not have access to this team"
            };
        }
        
        var projectOwner = await _projectRepository.GetProjectByIdAsync(projectId);
        if (user.UserId != projectOwner.OwnerId)
        {
            return new OperationResult
            {
                Success = false,
                Message = "You do not have access to this team"
            };
        }

        var teams = await _teamRepository.GetTeamsByProjectIdAsync(projectId);
        return new OperationResult
        {
            Success = true,
            Message = "Teams found",
            Data = teams
        };
    }
}
