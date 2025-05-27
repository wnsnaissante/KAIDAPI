using KaidAPI.ViewModel;
using KaidAPI.Models;
using KaidAPI.Repositories;

namespace KaidAPI.Services;

public class ProjectService : IProjectService
{
    private readonly IUserRepository _userRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IMembershipRepository _membershipRepository;
    private readonly ILogger<ProjectService> _logger;

    public ProjectService(IUserRepository userRepository,IProjectRepository projectRepository,IMembershipRepository membershipRepository, ILogger<ProjectService> logger)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
        _membershipRepository = membershipRepository ?? throw new ArgumentNullException(nameof(membershipRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<OperationResult> CreateNewProjectAsync(ProjectRequest projectRequest, string oidcSub)
    {
        try
        {
            var user = await _userRepository.GetUserByOidcAsync(oidcSub);
            if (user is null)
            {
                return new OperationResult
                {
                    Success = false,
                    Message = "User not found"
                };
            }

            var newProject = new Project
            {
                ProjectId = Guid.NewGuid(),
                ProjectName = projectRequest.ProjectName,
                ProjectDescription = projectRequest.ProjectDescription,
                CreatedAt = DateTime.UtcNow,
                DueDate = projectRequest.DueDate,
                OwnerId = user.UserId
            };

            var projectId = await _projectRepository.CreateProjectAsync(newProject);

            var membership = new Membership
            {
                UserId = user.UserId,
                ProjectId = projectId,
                ProjectMembershipId = Guid.NewGuid(),
                RoleId = 1,
                JoinedAt = DateTime.UtcNow,
                Status = "Active",
                IsActivated = true,
                SuperiorId = user.UserId
            };
            
            await _membershipRepository.CreateMembershipAsync(membership);

            return new OperationResult
            {
                Success = true,
                Message = "Project created successfully",
                Data = projectId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create project");
            return new OperationResult
            {
                Success = false,
                Message = "Internal server error"
            };
        }
    }

    public async Task<OperationResult> UpdateProjectAsync(ProjectRequest projectRequest, string oidcSub, Guid projectId)
    {
        try
        {
            var user = await _userRepository.GetUserByOidcAsync(oidcSub);

            if (user is null)
            {
                return new OperationResult { Success = false, Message = "User not found" };
            }

            var existingProject = await _projectRepository.GetProjectByIdAsync(projectId);
            if (existingProject is null)
            {
                return new OperationResult { Success = false, Message = "Project not found" };
            }
            
            if (existingProject.OwnerId != user.UserId)
            {
                return new OperationResult { Success = false, Message = "No permission to edit project" };
            }

            var project = new Project()
            {
                ProjectId = projectId,
                ProjectName = projectRequest.ProjectName,
                ProjectDescription = projectRequest.ProjectDescription,
                DueDate = projectRequest.DueDate,
                OwnerId = user.UserId,
                CreatedAt = existingProject.CreatedAt
            };
            await _projectRepository.UpdateProjectAsync(project);
            return new OperationResult { Success = true, Message = "Project updated successfully" };
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Project update Failed");
            return new OperationResult { Success = false, Message = "Error Update project" };
        }
    }

    public async Task<OperationResult> DeleteProjectAsync(Guid projectId, string oidcSub)
    {
        try
        {
            var user = await _userRepository.GetUserByOidcAsync(oidcSub);
            if (user is null)
            {
                return new OperationResult { Success = false, Message = "User not found" };
            }
            var existingProject = await _projectRepository.GetProjectByIdAsync(projectId);
            if (existingProject == null)
            {
                return new OperationResult { Success = false, Message = "Project not found" };
            }
            if (existingProject.OwnerId != user.UserId)
            {
                return new OperationResult { Success = false, Message = "No permission to delete project" };
            }
            await _projectRepository.DeleteProjectAsync(projectId);
            return new OperationResult { Success = true, Message = "Project deleted successfully" };
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Project delete Failed");
            return new OperationResult { Success = false, Message = "Error Delete project" };
        }
    }

    public async Task<OperationResult> GetProjectByProjectIdAsync(string oidcSub, Guid projectId)
    {
        try
        {
            var user = await _userRepository.GetUserByOidcAsync(oidcSub);
            if (user is null)
            {
                return new OperationResult { Success = false, Message = "User not found" };
            }
            var project = await _projectRepository.GetProjectByIdAsync(projectId);
            if (project == null)
            {
                return new OperationResult { Success = false, Message = "Project not found" };
            }
            if (project.OwnerId != user.UserId)
            {
                return new OperationResult { Success = false, Message = "No permission to get project" };
            }
            return new OperationResult { Success = true, Message = "Project retrieved successfully", Data = project };
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Project get Failed");
            return new OperationResult { Success = false, Message = "Error Get project" };
        }
    }

    public async Task<OperationResult> GetProjectsByUserIdAsync(string oidcSub)
    {
        try
        {
            var user = await _userRepository.GetUserByOidcAsync(oidcSub);
            if (user is null)
            {
                return new OperationResult { Success = false, Message = "User not found" };
            }
            var projects = new List<ProjectResponse>();
            
            var memberships = await _membershipRepository.GetActivatedMembershipsByUserIdAsync(user.UserId);
            
            foreach (var membership in memberships)
            {
                var project = await _projectRepository.GetProjectByIdAsync(membership.ProjectId);
                var projectResponse = new ProjectResponse
                {
                    ProjectId = project.ProjectId,
                    ProjectName = project.ProjectName,
                    ProjectDescription = project.ProjectDescription,
                    DueDate = project.DueDate,
                    OwnerId = user.UserId,
                    CreatedAt = project.CreatedAt
                };
                projects.Add(projectResponse);
            }

            return new OperationResult { Success = true, Message = "Projects retrieved successfully", Data = projects };
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Projects get Failed");
            return new OperationResult { Success = false, Message = "Error Get projects" };
        }
    }

    public async Task<OperationResult> GetInvitationsByUserIdAsync(string oidcSub)
    {
        try
        {
            var user = await _userRepository.GetUserByOidcAsync(oidcSub);
            if (user is null)
            {
                return new OperationResult { Success = false, Message = "User not found" };
            }
            var projects = new List<ProjectResponse>();

            var memberships = await _membershipRepository.GetDeactivatedMembershipsByUserIdAsync(user.UserId);

            foreach (var membership in memberships)
            {
                var project = await _projectRepository.GetProjectByIdAsync(membership.ProjectId);
                var projectResponse = new ProjectResponse
                {
                    ProjectId = project.ProjectId,
                    ProjectName = project.ProjectName,
                    ProjectDescription = project.ProjectDescription,
                    DueDate = project.DueDate,
                    OwnerId = user.UserId,
                    CreatedAt = project.CreatedAt
                };
                projects.Add(projectResponse);
            }

            return new OperationResult { Success = true, Message = "Projects retrieved successfully", Data = projects };
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Projects get Failed");
            return new OperationResult { Success = false, Message = "Error Get projects" };
        }
    }
}