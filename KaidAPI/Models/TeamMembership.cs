using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("TeamMemberships")]
public class TeamMembership
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid MembershipId { get; set; }
    
    public Guid TeamId { get; set; }
    public Guid UserId { get; set; }
    public int RoleId { get; set; }
    public DateTime JoinedAt { get; set; }
}

