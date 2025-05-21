using KaidAPI.Context;
using KaidAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace KaidAPI.Repositories;

public class ProjectRepository: IProjectRepository
{
    private readonly ServerDbContext _context;

    public ProjectRepository(ServerDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Guid> CreateProjectAsync(Project project)
    {
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();
        return project.ProjectId; 
    }
    
    public async Task<Project> GetProjectByIdAsync(Guid projectId)
    {
        return await _context.Projects.FindAsync(projectId);
    }
    
    public async Task UpdateProjectAsync(Project project)
    {
        var existing = await _context.Projects.FindAsync(project.ProjectId);
        if (existing != null)
        {
            existing.ProjectName = project.ProjectName;
            existing.ProjectDescription = project.ProjectDescription;
            existing.DueDate = project.DueDate;

            _context.Projects.Update(existing);
            await _context.SaveChangesAsync();
        }
    }
    
    public async Task DeleteProjectAsync(Guid projectId)
    {
        var project = await _context.Projects.FindAsync(projectId);
        if (project != null)
        {
            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();
        }
    }
}