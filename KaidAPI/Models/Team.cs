namespace KaidAPI.Models;

public class Team
{
    public string TeamId { get; set; }
    public string ProjectId { get; set; }
    public string TeamName { get; set; }
    public string Description { get; set; }
    public Guid LeaderId { get; set; }
}