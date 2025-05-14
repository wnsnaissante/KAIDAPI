using KaidAPI.Models;
using KaidAPI.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KaidAPI.Controllers;

[Authorize]
//[AllowAnonymous]
[Route("api/[controller]")]
[ApiController]
public class ProjectTaskController : ControllerBase
{
    private readonly IProjectTaskRepository _taskRepository;

    public ProjectTaskController(IProjectTaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    [HttpPost]
    public async Task<IActionResult> CreateTask([FromBody] ProjectTask task)
    {
        var id = await _taskRepository.CreateProjectTaskAsync(task);
        return Ok(new { TaskId = id });
    }

    [HttpGet("{taskId}")]
    public async Task<IActionResult> GetTaskById(string taskId)
    {
        var task = await _taskRepository.GetProjectTaskByIdAsync(taskId);
        if (task == null) return NotFound();
        return Ok(task);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllTasks()
    {
        var tasks = await _taskRepository.GetAllProjectTasksAsync();
        return Ok(tasks);
    }

    [HttpPut("{taskId}")]
    public async Task<IActionResult> UpdateTask(string taskId, [FromBody] ProjectTask updatedTask)
    {
        updatedTask.TaskId = taskId;
        await _taskRepository.UpdateProjectTaskAsync(updatedTask);
        return NoContent();
    }

    [HttpDelete("{taskId}")]
    public async Task<IActionResult> DeleteTask(string taskId)
    {
        await _taskRepository.DeleteProjectTaskAsync(taskId);
        return NoContent();
    }
}