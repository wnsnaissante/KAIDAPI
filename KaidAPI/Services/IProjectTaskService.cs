using KaidAPI.Models;
using KaidAPI.ViewModel;
using KaidAPI.ViewModel.Tasks;

namespace KaidAPI.Services
{
    public interface IProjectTaskService
    {
        Task<Guid> CreateProjectTaskAsync(ProjectTaskRequest request);
        Task<ProjectTask?> GetProjectTaskByIdAsync(Guid taskId);
        Task<List<ProjectTask>> GetAllProjectTasksAsync();
        Task<OperationResult> UpdateProjectTaskAsync(ProjectTask task, string oidcSub);
        Task<OperationResult> DeleteProjectTaskAsync(Guid taskId, string oidcSub);
        Task<object> GetTaskSummariesByTeamAsync(string teamName, string oidcSub);
        Task<List<TaskDistribution>> GetProjectTaskDistributionAsync(string oidcSub, Guid projectId);
        Task<OperationResult> GetTaskPriorityDistributionAsync(string oidcSub, Guid teamId);
        Task<List<AvailableTask>> GetAvailableTasksAsync(string oidcSub, Guid teamId);
        Task<List<TaskWorkload>> GetTeamTaskWorkloadAsync(string oidcSub, Guid teamId);
        Task<List<TaskPreview>> GetAssignedTasksAsync(string oidcSub, Guid projectId);
        Task<int> GetCompletedTasksPastWeekAsync(string oidcSub, Guid projectId);
        Task<int> GetUncompletedTasksPastWeekAsync(string oidcSub, Guid projectId);
        Task<int> GetLeftTasksCountAsync(string oidcSub, Guid projectId);
        Task<int> GetUrgentTasksCountAsync(string oidcSub, Guid projectId);
    }
}
