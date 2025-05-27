using KaidAPI.Models;
using KaidAPI.ViewModel;
using KaidAPI.Repositories;
using Microsoft.Extensions.Logging;

namespace KaidAPI.Services
{
    public class ProjectTaskService : IProjectTaskService
    {
        private readonly IProjectTaskRepository _taskRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMembershipRepository _membershipRepository;
        private readonly ITeamRepository _teamRepository;
        private readonly ILogger<ProjectTaskService> _logger;

        public ProjectTaskService(
            IProjectTaskRepository taskRepository,
            IProjectRepository projectRepository,
            IUserRepository userRepository,
            IMembershipRepository membershipRepository,
            ITeamRepository teamRepository,
            ILogger<ProjectTaskService> logger)
        {
            _taskRepository = taskRepository;
            _projectRepository = projectRepository;
            _userRepository = userRepository;
            _membershipRepository = membershipRepository;
            _teamRepository = teamRepository;
            _logger = logger;
        }

        private async Task<(bool IsAuthorized, string Message, User? User)> ValidateProjectOwnership(Guid projectId, string oidcSub)
        {
            var user = await _userRepository.GetUserByOidcAsync(oidcSub);
            if (user is null)
                return (false, "User not found", null);

            var project = await _projectRepository.GetProjectByIdAsync(projectId);
            if (project is null)
                return (false, "Project not found", null);

            if (project.OwnerId != user.UserId)
                return (false, "No permission for this project", user);

            return (true, string.Empty, user);
        }

        private async Task<bool> TeamExistsAsync(Guid teamId)
        {
            var team = await _teamRepository.GetTeamByTeamIdAsync(teamId);
            return team != null;
        }

        public async Task<string> CreateProjectTaskAsync(ProjectTaskRequest request)
        {
            try
            {
                if (!await TeamExistsAsync(request.TeamId))
                    return null;

                var result = await _taskRepository.CreateProjectTaskAsync(request);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateProjectTaskAsync failed");
                return null;
            }
        }

        public async Task<ProjectTask?> GetProjectTaskByIdAsync(string taskId)
        {
            return await _taskRepository.GetProjectTaskByIdAsync(taskId);
        }

        public async Task<List<ProjectTask>> GetAllProjectTasksAsync()
        {
            return await _taskRepository.GetAllProjectTasksAsync();
        }

        public async Task<OperationResult> UpdateProjectTaskAsync(ProjectTask task, string oidcSub)
        {
            try
            {
                var (isAuthorized, message, _) = await ValidateProjectOwnership(task.ProjectId, oidcSub);
                if (!isAuthorized)
                    return new OperationResult { Success = false, Message = message };

                await _taskRepository.UpdateProjectTaskAsync(task);
                return new OperationResult { Success = true, Message = "Task updated successfully" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateProjectTaskAsync failed");
                return new OperationResult { Success = false, Message = "Unexpected error while updating task" };
            }
        }

        public async Task<OperationResult> DeleteProjectTaskAsync(string taskId, string oidcSub)
        {
            try
            {
                var task = await _taskRepository.GetProjectTaskByIdAsync(taskId);
                if (task is null)
                    return new OperationResult { Success = false, Message = "Task not found" };

                var (isAuthorized, message, _) = await ValidateProjectOwnership(task.ProjectId, oidcSub);
                if (!isAuthorized)
                    return new OperationResult { Success = false, Message = message };

                await _taskRepository.DeleteProjectTaskAsync(taskId);
                return new OperationResult { Success = true, Message = "Task deleted successfully" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteProjectTaskAsync failed");
                return new OperationResult { Success = false, Message = "Unexpected error while deleting task" };
            }
        }

        public async Task<object> GetTaskSummariesByTeamAsync(string teamName, string oidcSub)
        {
            try
            {
                var user = await _userRepository.GetUserByOidcAsync(oidcSub);
                if (user is null)
                    return new { Success = false, Message = "User not found" };

                var memberships = await _membershipRepository.GetActivatedMembershipsByUserIdAsync(user.UserId);
                var teamMembership = memberships.FirstOrDefault(m => m.Team?.TeamName == teamName);

                if (teamMembership is null && !memberships.Any(m => m.RoleId == 1))
                    return new { Success = false, Message = "Access denied to team tasks" };

                var tasks = await _taskRepository.GetProjectTasksByTeamAsync(teamName);

                var assigneeIds = tasks.Select(t => t.Assignee).Distinct().ToList();

                var assignees = new List<User>();
                foreach (var assigneeId in assigneeIds)
                {
                    var _user = await _userRepository.GetUserByIdAsync(assigneeId);
                    if (_user != null)
                        assignees.Add(_user);
                }

                return new
                {
                    Success = true,
                    Team = teamName,
                    Tasks = tasks.Select(t =>
                    {
                        var assigneeUser = assignees.FirstOrDefault(u => u.UserId == t.Assignee);
                        return new
                        {
                            TrackCode = t.TaskId,
                            Summary = t.TaskName,
                            Status = t.StatusId,
                            Assignee = assigneeUser?.Username ?? "Unassigned",
                            Deadline = t.DueDate.ToString("yyyy-MM-dd"),
                            CreatedDate = t.CreatedAt.ToString("yyyy-MM-dd"),
                            Priority = t.Priority,
                            Reporter = t.Team?.Leader?.Username ?? "N/A"
                        };
                    })
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetTaskSummariesByTeamAsync failed");
                return new { Success = false, Message = "Unexpected error while retrieving tasks" };
            }
        }
    }
}
