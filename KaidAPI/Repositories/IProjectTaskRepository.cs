using KaidAPI.Models;
using KaidAPI.ViewModel;

namespace KaidAPI.Repositories;

public interface IProjectTaskRepository
{
    Task<Guid> CreateProjectTaskAsync(ProjectTaskRequest request);
    Task<ProjectTask?> GetProjectTaskByIdAsync(string taskId);
    Task<List<ProjectTask>> GetAllProjectTasksAsync();
    Task UpdateProjectTaskAsync(ProjectTask task);
    Task DeleteProjectTaskAsync(string taskId);
    Task<IEnumerable<ProjectTask>> GetProjectTasksByTeamAsync(string teamName);
    Task<IEnumerable<ProjectTask>> GetProjectTasksByTeamIdAsync(Guid teamId);
}