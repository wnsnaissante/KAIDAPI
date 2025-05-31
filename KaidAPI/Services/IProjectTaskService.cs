using KaidAPI.Models;
using KaidAPI.ViewModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KaidAPI.Services
{
    public interface IProjectTaskService
    {
        Task<Guid> CreateProjectTaskAsync(ProjectTaskRequest request);
        Task<ProjectTask?> GetProjectTaskByIdAsync(string taskId);
        Task<List<ProjectTask>> GetAllProjectTasksAsync();
        Task<OperationResult> UpdateProjectTaskAsync(ProjectTask task, string oidcSub);
        Task<OperationResult> DeleteProjectTaskAsync(string taskId, string oidcSub);
        Task<object> GetTaskSummariesByTeamAsync(string teamName, string oidcSub);
        Task<OperationResult> GetTaskWorkloadAsync(string oidcSub, Guid teamId);
        Task<OperationResult> GetAvailableTasksAsync(string oidcSub, Guid teamId);
        Task<OperationResult> GetTaskPriorityDistributionAsync(string oidcSub, Guid teamId);
    }
}
