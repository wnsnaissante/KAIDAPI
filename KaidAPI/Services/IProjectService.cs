using KaidAPI.ViewModel;
using KaidAPI.Models;

namespace KaidAPI.Services;

public interface IProjectService
{
    Task<OperationResult> CreateNewProjectAsync(ProjectRequest projectRequest, string oidcSub);
    Task<OperationResult> DeleteProjectAsync(Guid projectId, string oidcSub);
    Task<OperationResult> UpdateProjectAsync(ProjectRequest projectRequest, string oidcSub);
    Task<OperationResult> GetProjectByIdAsync(Guid projectId, string oidcSub);
    Task<OperationResult> GetAllProjectsAsync(string oidcSub);
}