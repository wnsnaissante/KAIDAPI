namespace KaidAPI.Models;

public class Team
{
    public string TeamID { get; set; }
    public string ProjectID { get; set; }
    public string TeamName { get; set; }
    public string Description { get; set; }
    public Guid LeaderID { get; set; }
}