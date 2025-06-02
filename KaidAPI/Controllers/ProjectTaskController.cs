using KaidAPI.Models;
using System.Security.Claims;
using KaidAPI.ViewModel;
using KaidAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KaidAPI.Controllers;

[Authorize]
[Route("api/v1/[controller]")]
[ApiController]
public class ProjectTaskController : ControllerBase
{
    private readonly IProjectTaskService _taskService;

    public ProjectTaskController(IProjectTaskService taskService)
    {
        _taskService = taskService;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateTask([FromBody] ProjectTaskRequest request)
    {
        var oidcSub = User.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");

        if (string.IsNullOrEmpty(oidcSub))
        {
            return Unauthorized("User does not have an access token.");
        }
        var result = await _taskService.CreateProjectTaskAsync(request);
        if (result == null)
            return BadRequest("Error");

        return Ok(new { TaskId = result });
    }

    [HttpGet("{taskId}")]
    public async Task<IActionResult> GetTaskById(string taskId)
    {
        var task = await _taskService.GetProjectTaskByIdAsync(taskId);
        if (task == null) return NotFound();
        return Ok(task);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllTasks()
    {
        var tasks = await _taskService.GetAllProjectTasksAsync();
        return Ok(tasks);
    }

    [HttpPut("{taskId}")]
    public async Task<IActionResult> UpdateTask(Guid taskId, [FromBody] ProjectTask updatedTask)
    {
        var oidcSub = User.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");

        if (string.IsNullOrEmpty(oidcSub))
        {
            return Unauthorized("User does not have an access token.");
        }
        updatedTask.TaskId = taskId;
        var result = await _taskService.UpdateProjectTaskAsync(updatedTask, oidcSub);
        if (!result.Success)
            return BadRequest(result.Message);

        return NoContent();
    }

    [HttpDelete("{taskId}")]
    public async Task<IActionResult> DeleteTask(string taskId)
    {
        var oidcSub = User.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");

        if (string.IsNullOrEmpty(oidcSub))
        {
            return Unauthorized("User does not have an access token.");
        }
        var result = await _taskService.DeleteProjectTaskAsync(taskId, oidcSub);
        if (!result.Success)
            return BadRequest(result.Message);

        return NoContent();
    }

    [HttpGet("priority-distribution")]
    public async Task<IActionResult> GetTaskPriorityDistribution([FromQuery] Guid teamId) {
        var oidcSub = User.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");

        if (string.IsNullOrEmpty(oidcSub))
        {
            return Unauthorized("User does not have an access token.");
        }   

        var result = await _taskService.GetTaskPriorityDistributionAsync(oidcSub, teamId);
        if (!result.Success) {
            return BadRequest(result.Message);
        }

        var dict = ((IEnumerable<dynamic>)result.Data)
            .ToDictionary(x => x.Priority.ToString(), x => (int)x.Count);

        return Ok(dict);
    }

    [HttpGet("available-tasks")]
    public async Task<IActionResult> GetAvailableTasks([FromQuery] Guid teamId) {
        var oidcSub = User.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");

        if (string.IsNullOrEmpty(oidcSub))
        {
            return Unauthorized("User does not have an access token.");
        }

        var result = await _taskService.GetAvailableTasksAsync(oidcSub, teamId);
        if (!result.Success) {
            return BadRequest(result.Message);
        }

        var tasks = result.Data as IEnumerable<KaidAPI.Models.ProjectTask>;
        var dtoList = tasks?.Select(t => new KaidAPI.ViewModel.Tasks.AvailableTask
        {
            TaskId = t.TaskId,
            TaskName = t.TaskName,
            TaskDescription = t.TaskDescription,
            Priority = t.Priority,
            DueDate = t.DueDate,
        }).ToList();

        return Ok(dtoList);
    }

    [HttpGet("task-workload")]
    public async Task<IActionResult> GetTaskWorkload([FromQuery] Guid teamId) {
        var oidcSub = User.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier ");

        if (string.IsNullOrEmpty(oidcSub))
        {
            return Unauthorized("User does not have an access token.");
        }

        var result = await _taskService.GetTaskWorkloadAsync(oidcSub, teamId);
        if (!result.Success) {
            return BadRequest(result.Message);
        }

        return Ok(result.Data);
    }
}
