using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KaidAPI.Models;

public class Membership
{
    [Key]
    public Guid ProjectMembershipId { get; set; }
    [Required]
    public Guid ProjectId { get; set; }
    [Required]
    public Guid UserId { get; set; }
    [Required]
    public Guid? SuperiorId { get; set; }
    [Required]
    public int RoleId { get; set; }

    public Guid? TeamId { get; set; }
    public DateTime JoinedAt { get; set; }

    [ForeignKey("RoleId")]
    public Role Role { get; set; }

    [ForeignKey("ProjectId")]
    public Project Project { get; set; }
    
    [ForeignKey("TeamId")]
    public Team Team { get; set; }

    [ForeignKey("SuperiorId")]
    public User Superior { get; set; }
    
    [MaxLength(20)]
    public string Status { get; set; }
}

