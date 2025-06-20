using System.Collections.Immutable;
using KaidAPI.Models;
using Microsoft.EntityFrameworkCore;
using TaskStatus = KaidAPI.Models.TaskStatus;

namespace KaidAPI.Context;

public class ServerDbContext: DbContext
{
    public ServerDbContext(DbContextOptions<ServerDbContext> options) : base(options) { }
    
    public DbSet<User> Users { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<ProjectTask> ProjectTasks { get; set; }
    public DbSet<Team> Teams { get; set; }
    public DbSet<Membership> Memberships { get; set; }
    public DbSet<Flag> Flags { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<TaskStatus> TaskStatuses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Membership>()
            .Property(m => m.IsActivated)
            .HasDefaultValue(false);
        modelBuilder.ApplyConfiguration(new TaskStatusConfiguration());
    }
}