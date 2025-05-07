using KaidAPI.ViewModel;
using KaidAPI.Models;
using KaidAPI.Repositories;

namespace KaidAPI.Services;

public class ProjectService : IProjectService
{
    private readonly IUserRepository _userRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly ILogger<ProjectService> _logger;

    public ProjectService(IUserRepository userRepository,IProjectRepository projectRepository, ILogger<ProjectService> logger)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
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

    public async Task<OperationResult> EditProjectAsync(ProjectRequest projectRequest, string oidcSub)
    {
        try
        {
            var user = await _userRepository.GetUserByOidcAsync(oidcSub);

            if (user is null)
            {
                return new OperationResult { Success = false, Message = "User not found" };
            }

            var existingProject = await _projectRepository.GetProjectByIdAsync(projectRequest.ProjectId);
            if (existingProject == null)
            {
                return new OperationResult { Success = false, Message = "Project not found" };
            }
            
            if (existingProject.OwnerId != user.UserId)
            {
                return new OperationResult { Success = false, Message = "No permission to edit project" };
            }

            var project = new Project()
            {
                ProjectId = projectRequest.ProjectId,
                ProjectName = projectRequest.ProjectName,
                ProjectDescription = projectRequest.ProjectDescription,
                DueDate = projectRequest.DueDate,
                OwnerId = user.UserId
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

    public async Task<OperationResult> GetProjectByIdAsync(Guid projectId, string oidcSub)
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

    public async Task<OperationResult> GetAllProjectsAsync(string oidcSub)
    {
        try
        {
            var user = await _userRepository.GetUserByOidcAsync(oidcSub);
            if (user is null)
            {
                return new OperationResult { Success = false, Message = "User not found" };
            }
            var projects = await _projectRepository.GetAllProjectsAsync();
            if (projects.Count == 0)
            {
                return new OperationResult { Success = false, Message = "No projects found" };
            }
            if (projects.Any(p => p.OwnerId != user.UserId))
            {
                return new OperationResult { Success = false, Message = "No permission to get projects" };
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