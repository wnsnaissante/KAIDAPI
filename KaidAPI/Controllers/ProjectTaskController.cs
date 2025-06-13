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
    public async Task<IActionResult> GetTaskById(Guid taskId)
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
    public async Task<IActionResult> DeleteTask(Guid taskId)
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

    // PM View's Project Tasks Distribution
    [HttpGet("project-task-distribution")]
    public async Task<IActionResult> GetProjectTaskDistribution([FromQuery] Guid projectId) {
        var oidcSub = User.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");

        if (string.IsNullOrEmpty(oidcSub))
        {
            return Unauthorized("User does not have an access token.");
        }

        var result = await _taskService.GetProjectTaskDistributionAsync(oidcSub, projectId);
        return Ok(result);
    }

    // General View & TL View's Task Priority Distribution
    [HttpGet("task-priority-distribution")]
    public async Task<IActionResult> GetTaskPriorityDistribution([FromQuery] Guid teamId) {
        var oidcSub = User.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");

        if (string.IsNullOrEmpty(oidcSub))
        {
            return Unauthorized("User does not have an access token.");
        }

        var result = await _taskService.GetTaskPriorityDistributionAsync(oidcSub, teamId);
        return Ok(result);
    }

    // General View & TL View's Available Tasks
    [HttpGet("available-tasks")]
    public async Task<IActionResult> GetAvailableTasks([FromQuery] Guid teamId) {
        var oidcSub = User.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        if (string.IsNullOrEmpty(oidcSub))
        {
            return Unauthorized("User does not have an access token.");
        }

        var result = await _taskService.GetAvailableTasksAsync(oidcSub, teamId);
        return Ok(result);
    }

    // TL View's Task Workload
    [HttpGet("team-task-workload")]
    public async Task<IActionResult> GetTeamTaskWorkload([FromQuery] Guid teamId) {
        var oidcSub = User.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        if (string.IsNullOrEmpty(oidcSub))
        {
            return Unauthorized("User does not have an access token.");
        }

        var result = await _taskService.GetTeamTaskWorkloadAsync(oidcSub, teamId);
        return Ok(result);
    }

    [HttpGet("completed-tasks-past-week")]
    public async Task<IActionResult> GetCompletedTasksPastWeek([FromQuery] Guid projectId) {
        var oidcSub = User.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        if (string.IsNullOrEmpty(oidcSub))
        {
            return Unauthorized("User does not have an access token.");
        }

        var result = await _taskService.GetCompletedTasksPastWeekAsync(oidcSub, projectId);
        return Ok(result);
    }

    [HttpGet("uncompleted-tasks-past-week")]
    public async Task<IActionResult> GetUncompletedTasksPastWeek([FromQuery] Guid projectId) {
        var oidcSub = User.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        if (string.IsNullOrEmpty(oidcSub))
        {
            return Unauthorized("User does not have an access token.");
        }

        var result = await _taskService.GetUncompletedTasksPastWeekAsync(oidcSub, projectId);
        return Ok(result);
    }

    [HttpGet("left-tasks")]
    public async Task<IActionResult> GetLeftTasks([FromQuery] Guid projectId) {
        var oidcSub = User.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        if (string.IsNullOrEmpty(oidcSub))
        {
            return Unauthorized("User does not have an access token.");
        }

        var result = await _taskService.GetLeftTasksCountAsync(oidcSub, projectId);
        return Ok(result);
    }

    [HttpGet("urgent-tasks")]
    public async Task<IActionResult> GetUrgentTasks([FromQuery] Guid projectId) {
        var oidcSub = User.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        if (string.IsNullOrEmpty(oidcSub))
        {
            return Unauthorized("User does not have an access token.");
        }

        var result = await _taskService.GetUrgentTasksCountAsync(oidcSub, projectId);

        return Ok(result);
    }

    [HttpGet("assigned-tasks")]
    public async Task<IActionResult> GetAssignedTasks([FromQuery] Guid projectId) {
        var oidcSub = User.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        if (string.IsNullOrEmpty(oidcSub))
        {
            return Unauthorized("User does not have an access token.");
        }

        var result = await _taskService.GetAssignedTasksAsync(oidcSub, projectId);
        return Ok(result);
    }
}
