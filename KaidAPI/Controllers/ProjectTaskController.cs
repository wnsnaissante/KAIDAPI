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
    public async Task<IActionResult> UpdateTask(string taskId, [FromBody] ProjectTask updatedTask)
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
}
