public class TeamMembership
{
    public Guid TeamId { get; set; }
    public Guid UserId { get; set; }
    public int RoleId { get; set; }
    public DateTime JoinedAt { get; set; }
}

