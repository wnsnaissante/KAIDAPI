namespace KaidAPI.Models;

public class Team
{
    public Guid TeamId { get; set; }
    public Guid ProjectId { get; set; }
    public string TeamName { get; set; }
    public string Description { get; set; }
    public Guid LeaderId { get; set; }
}