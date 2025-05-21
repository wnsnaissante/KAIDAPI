using System.Collections.Immutable;
using KaidAPI.Models;
using Microsoft.EntityFrameworkCore;

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
}