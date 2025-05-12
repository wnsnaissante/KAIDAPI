using KaidAPI.Context;
using KaidAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace KaidAPI.Repositories;

public class ProjectTaskRepository : IProjectTaskRepository
{
    private readonly ServerDbContext _context;

    public ProjectTaskRepository(ServerDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<string> CreateProjectTaskAsync(ProjectTask task)
    {
        task.CreatedAt = DateTime.UtcNow;
        task.UpdatedAt = DateTime.UtcNow;
        _context.ProjectTasks.Add(task);
        await _context.SaveChangesAsync();
        return task.TaskID;
    }

    public async Task<ProjectTask> GetProjectTaskByIdAsync(string taskId)
    {
        return await _context.ProjectTasks.FindAsync(taskId);
    }

    public async Task<List<ProjectTask>> GetAllProjectTasksAsync()
    {
        return await _context.ProjectTasks.ToListAsync();
    }

    public async Task UpdateProjectTaskAsync(ProjectTask task)
    {
        var existing = await _context.ProjectTasks.FindAsync(task.TaskID);
        if (existing != null)
        {
            existing.TaskName = task.TaskName;
            existing.TaskDescription = task.TaskDescription;
            existing.Assignee = task.Assignee;
            existing.StatusID = task.StatusID;
            existing.Priority = task.Priority;
            existing.DueDate = task.DueDate;
            existing.UpdatedAt = DateTime.UtcNow;

            _context.ProjectTasks.Update(existing);
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteProjectTaskAsync(string taskId)
    {
        var task = await _context.ProjectTasks.FindAsync(taskId);
        if (task != null)
        {
            _context.ProjectTasks.Remove(task);
            await _context.SaveChangesAsync();
        }
    }
}