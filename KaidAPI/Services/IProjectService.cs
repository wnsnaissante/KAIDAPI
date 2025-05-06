using KaidAPI.ViewModel;
using KaidAPI.Models;

namespace KaidAPI.Services;

public interface IProjectService
{
    Task<OperationResult> CreateNewProjectAsync(ProjectRequest projectRequest, string oidcSub);
    // Task<OperationResult> DeleteProjectAsync(ProjectRequest projectRequest);
    Task<OperationResult> EditProjectAsync(ProjectRequest projectRequest, string oidcSub);
}