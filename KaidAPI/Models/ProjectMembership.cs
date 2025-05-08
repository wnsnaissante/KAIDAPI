public class ProjectMembership
{
    [Key]
    public Guid ProjectMembershipId { get; set; }
    [Required]
    public Guid ProjectId { get; set; }
    [Required]
    public Guid UserId { get; set; }
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
}

