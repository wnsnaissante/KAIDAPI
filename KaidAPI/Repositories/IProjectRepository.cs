using KaidAPI.Models;

namespace KaidAPI.Repositories;

public interface IProjectRepository
{
    Task<Guid> CreateProjectAsync(Project project);
    Task<Project> GetProjectByIdAsync(Guid projectId);
    Task UpdateProjectAsync(Project project);
    Task DeleteProjectAsync(Guid projectId);
}