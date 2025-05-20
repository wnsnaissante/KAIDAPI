using KaidAPI.Models;

namespace KaidAPI.Repositories;

public interface IProjectRepository
{
    Task<Guid> CreateProjectAsync(Project project);
    Task<Project> GetProjectByIdAsync(Guid projectId);
    Task<OperationResult> UpdateProjectAsync(Project project);
    Task<OperationResult> DeleteProjectAsync(Guid projectId);
}