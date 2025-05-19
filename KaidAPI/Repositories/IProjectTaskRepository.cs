using KaidAPI.Models;

namespace KaidAPI.Repositories;

public interface IProjectTaskRepository
{
    Task<string> CreateProjectTaskAsync(ProjectTask task);
    Task<ProjectTask> GetProjectTaskByIdAsync(string taskId);
    Task<List<ProjectTask>> GetAllProjectTasksAsync();
    Task UpdateProjectTaskAsync(ProjectTask task);
    Task DeleteProjectTaskAsync(string taskId);
    Task<IEnumerable<ProjectTask>> GetProjectTasksByTeamAsync(string teamName);
}