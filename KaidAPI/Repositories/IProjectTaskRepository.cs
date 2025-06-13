using KaidAPI.Models;
using KaidAPI.ViewModel;

namespace KaidAPI.Repositories;

public interface IProjectTaskRepository
{
    Task<Guid> CreateProjectTaskAsync(ProjectTaskRequest request);
    Task<ProjectTask?> GetProjectTaskByIdAsync(Guid taskId);
    Task<List<ProjectTask>> GetAllProjectTasksAsync();
    Task UpdateProjectTaskAsync(ProjectTask task);
    Task DeleteProjectTaskAsync(Guid taskId);
    Task<IEnumerable<ProjectTask>> GetProjectTasksByTeamAsync(string teamName);
    Task<IEnumerable<ProjectTask>> GetProjectTasksByTeamIdAsync(Guid teamId);
    Task<IEnumerable<ProjectTask>> GetAssignedTasksAsync(Guid userId);
    Task<IEnumerable<ProjectTask>> GetProjectTasksByProjectIdAsync(Guid projectId);
}