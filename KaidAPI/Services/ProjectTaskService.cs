using KaidAPI.Models;
using KaidAPI.ViewModel;
using KaidAPI.Repositories;
using KaidAPI.ViewModel.Tasks;

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

        public async Task<Guid> CreateProjectTaskAsync(ProjectTaskRequest request)
        {
            try
            {
                if (!await TeamExistsAsync(request.TeamId))
                    return Guid.Empty;

                var result = await _taskRepository.CreateProjectTaskAsync(request);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateProjectTaskAsync failed");
                return Guid.Empty;
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
        
        // PM View's Team Tasks Distribution
        public async Task<List<TaskDistribution>> GetProjectTaskDistributionAsync(string oidcSub, Guid projectId) {
            var user = await _userRepository.GetUserByOidcAsync(oidcSub);
            if (user is null)
                return new List<TaskDistribution>();

            var userMembership = await _membershipRepository.GetMembershipByProjectIdAndUserIdAsync(projectId, user.UserId);

            if (userMembership is null)
                return new List<TaskDistribution>();

            if (userMembership.RoleId == 1) {
                var tasks = await _taskRepository.GetProjectTasksByProjectIdAsync(projectId);
                var teamTaskDistribution = tasks.GroupBy(t => t.Team?.TeamName)
                    .Select(g => new TaskDistribution {
                        TeamName = g.Key ?? "N/A",
                        Percent = (float)(g.Count() * 100.0 / tasks.Count())
                    }).ToList();

                return teamTaskDistribution;
            }
            else
            {
                return new List<TaskDistribution>();
            }
        }


        // General View & TL View's Task Priority Distribution
        public async Task<OperationResult> GetTaskPriorityDistributionAsync(string oidcSub, Guid teamId)
        {
            try
            {
                var user = await _userRepository.GetUserByOidcAsync(oidcSub);
                if (user is null)
                    return new OperationResult { Success = false, Message = "User not found" };

                var memberships = await _membershipRepository.GetMembershipsByUserIdAsync(user.UserId);
                var userMembership = await _membershipRepository.GetMembershipByProjectIdAndUserIdAsync(teamId, user.UserId);

                if (userMembership is null)
                    return new OperationResult { Success = false, Message = "Access denied to team tasks" };
                
                if (userMembership.RoleId == 1) {
                    return new OperationResult { Success = false, Message = "Access denied to team tasks" };
                }
                if (userMembership.RoleId == 2) {
                    var tasks = await _taskRepository.GetProjectTasksByTeamIdAsync(teamId);
                    var priorityDistribution = tasks.GroupBy(t => t.Priority)
                        .Select(g => new {
                            Priority = g.Key,
                            Count = g.Count()
                        })
                        .OrderBy(p => p.Priority)
                        .ToList();
                    return new OperationResult { Success = true, Data = priorityDistribution };
                } else if (userMembership.RoleId == 3) {
                    var tasks = await _taskRepository.GetAllProjectTasksAsync();
                    var userTasks = tasks.Where(t => t.Assignee == user.UserId).ToList();
                    var priorityDistribution = userTasks.GroupBy(t => t.Priority)
                        .Select(g => new {
                            Priority = g.Key,
                            Count = g.Count()
                        })
                        .OrderBy(p => p.Priority)
                        .ToList();
                    return new OperationResult { Success = true, Data = priorityDistribution };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetTaskPriorityDistributionAsync failed");
                return new OperationResult { Success = false, Message = "Unexpected error while retrieving task priority distribution" };
            }
            return new OperationResult { Success = false, Message = "Unexpected error while retrieving task priority distribution" };
        }

        // General View & TL View's Available Tasks
        public async Task<List<AvailableTask>> GetAvailableTasksAsync(string oidcSub, Guid teamId)
        {
            try
            {
                var user = await _userRepository.GetUserByOidcAsync(oidcSub);
                if (user is null)
                    return new List<AvailableTask>();

                var memberships = await _membershipRepository.GetMembershipsByUserIdAsync(user.UserId);
                var userMembership = await _membershipRepository.GetMembershipByProjectIdAndUserIdAsync(teamId, user.UserId);

                if (userMembership is null)
                    return new List<AvailableTask>();

                var tasks = await _taskRepository.GetProjectTasksByTeamIdAsync(teamId);

                var availableTasks = tasks.Where(t => t.StatusId == (int)TaskStatusEnum.Todo && t.Assignee == null).ToList(); 

                return availableTasks.Select(t => new AvailableTask {
                    TaskName = t.TaskName,
                    TaskDescription = t.TaskDescription,
                    Priority = t.Priority,
                    DueDate = t.DueDate
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAvailableTasksAsync failed");
                return new List<AvailableTask>();
            }
        }
        
        // TL View's Task Workload
        public async Task<List<TaskWorkload>> GetTeamTaskWorkloadAsync(string oidcSub, Guid teamId)
        {
            try
            {
                var user = await _userRepository.GetUserByOidcAsync(oidcSub);
                if (user is null)
                    return new List<TaskWorkload>();

                var memberships = await _membershipRepository.GetMembershipsByUserIdAsync(user.UserId);
                var userMembership = await _membershipRepository.GetMembershipByProjectIdAndUserIdAsync(teamId, user.UserId);
                
                if (userMembership is null)
                    return new List<TaskWorkload>();

                var tasks = await _taskRepository.GetProjectTasksByTeamIdAsync(teamId);

                var totalCount = tasks.Count();
                var workload = tasks.GroupBy(t => t.Assignee)
                    .Select(g => new {
                        Assignee = g.Key,
                        Count = g.Count(),
                        Percent = totalCount == 0 ? 0 : (g.Count() * 100.0) / totalCount
                    })
                    .OrderBy(g => g.Assignee)
                    .ToList();

                return workload.Select(w => new TaskWorkload {
                    Username = _userRepository.GetUserNameByIdAsync(w.Assignee).Result,
                    Percent = (float)w.Percent
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetTeamTaskWorkloadAsync failed");
                return new List<TaskWorkload>();
            }
        }
        
        // General View's Assigned Tasks
        public async Task<List<TaskPreview>> GetAssignedTasksAsync(string oidcSub, Guid projectId) 
        {
            var user = await _userRepository.GetUserByOidcAsync(oidcSub);
            if (user is null)
                return new List<TaskPreview>();

            var memberships = await _membershipRepository.GetMembershipsByUserIdAsync(user.UserId);
            var userMembership = await _membershipRepository.GetMembershipByProjectIdAndUserIdAsync(projectId, user.UserId);

            if (userMembership is null)
                return new List<TaskPreview>();

            var assignedTasks = await _taskRepository.GetAssignedTasksAsync(user.UserId);
            var taskPreviews = assignedTasks.Select(t => new TaskPreview {
                TaskName = t.TaskName,
                TaskDescription = t.TaskDescription,
                Priority = t.Priority,
                DueDate = t.DueDate
            }).ToList();
            return taskPreviews;
        }

        public async Task<int> GetCompletedTasksPastWeekAsync(string oidcSub, Guid projectId) 
        {
            var user = await _userRepository.GetUserByOidcAsync(oidcSub);
            if (user is null)
                return 0;

            var memberships = await _membershipRepository.GetMembershipsByUserIdAsync(user.UserId);
            var userMembership = await _membershipRepository.GetMembershipByProjectIdAndUserIdAsync(projectId, user.UserId);

            if (userMembership is null)
                return 0;

            var tasks = await _taskRepository.GetProjectTasksByProjectIdAsync(projectId);

            if (userMembership.RoleId == 2) {
                var teamUserIds = memberships.Select(m => m.UserId).ToList();
                var completedTasks = tasks.Where(t => t.StatusId == (int)TaskStatusEnum.Finished && t.CreatedAt >= DateTime.Now.AddDays(-7) && teamUserIds.Contains(t.Assignee)).ToList();
                return completedTasks.Count();
            } else if (userMembership.RoleId == 3) {
                var completedTasks = tasks.Where(t => t.StatusId == (int)TaskStatusEnum.Finished && t.CreatedAt >= DateTime.Now.AddDays(-7) && t.Assignee == user.UserId).ToList();
                return completedTasks.Count();
            } else {
                return 0;
            }
        }

        public async Task<int> GetUncompletedTasksPastWeekAsync(string oidcSub, Guid projectId) 
        {
            var user = await _userRepository.GetUserByOidcAsync(oidcSub);
            if (user is null)
                return 0;

            var memberships = await _membershipRepository.GetMembershipsByUserIdAsync(user.UserId);
            var userMembership = await _membershipRepository.GetMembershipByProjectIdAndUserIdAsync(projectId, user.UserId);

            if (userMembership is null)
                return 0;

            var tasks = await _taskRepository.GetProjectTasksByProjectIdAsync(projectId);

            if (userMembership.RoleId == 2) {
                var teamUserIds = memberships.Select(m => m.UserId).ToList();
                var uncompletedTasks = tasks.Where(t => t.StatusId != (int)TaskStatusEnum.Finished && t.CreatedAt >= DateTime.Now.AddDays(-7) && teamUserIds.Contains(t.Assignee)).ToList();
                return uncompletedTasks.Count();
            } else if (userMembership.RoleId == 3) {
                var uncompletedTasks = tasks.Where(t => t.StatusId != (int)TaskStatusEnum.Finished && t.CreatedAt >= DateTime.Now.AddDays(-7) && t.Assignee == user.UserId).ToList();
                return uncompletedTasks.Count();
            } else {
                return 0;
            }
        }

        public async Task<int> GetLeftTasksCountAsync(string oidcSub, Guid projectId)
        {
            var user = await _userRepository.GetUserByOidcAsync(oidcSub);
            if (user is null)
                return 0;

            var memberships = await _membershipRepository.GetMembershipsByUserIdAsync(user.UserId);
            var userMembership = await _membershipRepository.GetMembershipByProjectIdAndUserIdAsync(projectId, user.UserId);

            if (userMembership is null)
                return 0;

            var tasks = await _taskRepository.GetProjectTasksByProjectIdAsync(projectId);

            if (userMembership.RoleId == 2) {
                var teamUserIds = memberships.Select(m => m.UserId).ToList();
                var leftTasks = tasks.Where(t => t.StatusId != (int)TaskStatusEnum.Finished && teamUserIds.Contains(t.Assignee)).ToList();
                return leftTasks.Count();
            } else if (userMembership.RoleId == 3) {
                var leftTasks = tasks.Where(t => t.StatusId != (int)TaskStatusEnum.Finished && t.Assignee == user.UserId).ToList();
                return leftTasks.Count();
            } else {
                return 0;
            }
        }

        public async Task<int> GetUrgentTasksCountAsync(string oidcSub, Guid projectId) {
            var user = await _userRepository.GetUserByOidcAsync(oidcSub);
            if (user is null)
                return 0;

            var memberships = await _membershipRepository.GetMembershipsByUserIdAsync(user.UserId);
            var userMembership = await _membershipRepository.GetMembershipByProjectIdAndUserIdAsync(projectId, user.UserId);

            if (userMembership is null)
                return 0;

            var tasks = await _taskRepository.GetProjectTasksByProjectIdAsync(projectId);

            if (userMembership.RoleId == 2) {
                var teamUserIds = memberships.Select(m => m.UserId).ToList();
                var urgentTasks = tasks.Where(t => 
                    t.DueDate <= DateTime.Now.AddDays(7) && 
                    teamUserIds.Contains(t.Assignee)).ToList();
                return urgentTasks.Count();
            } else if (userMembership.RoleId == 3) {
                var urgentTasks = tasks.Where(t => t.DueDate <= DateTime.Now.AddDays(7) && t.Assignee == user.UserId).ToList();
                return urgentTasks.Count();
            } else {
                return 0;
            }
        }
    }
}
