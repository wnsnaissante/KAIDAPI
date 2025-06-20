using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using KaidAPI.Models;

namespace KaidAPI.Models;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasKey(r => r.RoleId);
        
        builder.HasData(
            new Role 
            { 
                RoleId = 1, 
                RoleName = "Admin", 
                RoleDescription = "Project Administrator" 
            },
            new Role 
            { 
                RoleId = 2, 
                RoleName = "Manager", 
                RoleDescription = "Team Manager" 
            },
            new Role 
            { 
                RoleId = 3, 
                RoleName = "Member", 
                RoleDescription = "Team Member" 
            }
        );
    }
} 