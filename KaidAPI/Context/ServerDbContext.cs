using System.Collections.Immutable;
using KaidAPI.Models;
using Microsoft.EntityFrameworkCore;
using TaskStatus = KaidAPI.Models.TaskStatus;

namespace KaidAPI.Context;

public class ServerDbContext : DbContext
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

        // Membership 엔티티 설정
        modelBuilder.Entity<Membership>(entity =>
        {
            entity.Property(m => m.IsActivated)
                  .HasDefaultValue(false);

            entity.Property(m => m.ProjectMembershipId)
                  .HasColumnType("char(36)");

            entity.Property(m => m.ProjectId)
                  .HasColumnType("char(36)");

            entity.Property(m => m.UserId)
                  .HasColumnType("char(36)");

            entity.Property(m => m.SuperiorId)
                  .HasColumnType("char(36)");

            entity.Property(m => m.TeamId)
                  .HasColumnType("char(36)");
        });

        // 다른 엔티티 구성 (예: TaskStatus, Role)
        modelBuilder.ApplyConfiguration(new TaskStatusConfiguration());
        modelBuilder.ApplyConfiguration(new RoleConfiguration());
    }
}