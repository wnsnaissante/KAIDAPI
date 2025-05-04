using System.Collections.Immutable;
using KaidAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace KaidAPI.Context;

public class ServerDbContext: DbContext
{
    public ServerDbContext(DbContextOptions<ServerDbContext> options) : base(options) { }
    
    public DbSet<User> Users { get; set; }
    public DbSet<Project> Projects { get; set; }
}