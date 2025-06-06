using KaidAPI.ViewModel;
using KaidAPI.Models;

namespace KaidAPI.Services;

public interface IProjectService
{
    Task<OperationResult> CreateNewProjectAsync(ProjectRequest projectRequest, string oidcSub);
    Task<OperationResult> DeleteProjectAsync(Guid projectId, string oidcSub);
    Task<OperationResult> UpdateProjectAsync(ProjectRequest projectRequest, string oidcSub, Guid projectId);
    Task<OperationResult> GetProjectByProjectIdAsync(string oidcSub, Guid projectId);
    Task<OperationResult> GetProjectsByUserIdAsync(string oidcSub);
    Task<OperationResult> GetInvitationsByUserIdAsync(string oidcSub);
    Task<OperationResult> GetProjectDeadlineAsync(string oidcSub, Guid projectId);
}