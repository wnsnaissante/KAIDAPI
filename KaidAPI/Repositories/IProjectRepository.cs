using KaidAPI.Models;

namespace KaidAPI.Repositories;

public interface IProjectRepository
{
    Task<Guid> CreateProjectAsync(Project project);
    Task<Project> GetProjectByIdAsync(Guid projectId);
    Task<List<Project>> GetAllProjectsAsync();
    System.Threading.Tasks.Task UpdateProjectAsync(Project project);
    System.Threading.Tasks.Task DeleteProjectAsync(Guid projectId);
}