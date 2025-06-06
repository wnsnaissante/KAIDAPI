using System.ComponentModel.DataAnnotations.Schema;

namespace KaidAPI.Models;

public enum FlagStatus
{
    Unsolved,
    Solved,
    Todo,
    Delayed
}

public class Flag
{
    public Guid FlagId { get; set; }
    public Guid ProjectId { get; set; }
    public Guid TeamId { get; set; }
    public Guid OwnerId { get; set; }
    public string FlagDescription { get; set; }
    public FlagStatus Status { get; set; }
    public Guid Reporter { get; set; }
    public DateTime CreatedAt { get; set; }
    public int Priority { get; set; }
    
    [ForeignKey("ProjectId")]
    public Project Project { get; set; }

    [ForeignKey("TeamId")]
    public Team Team { get; set; }

    [ForeignKey("OwnerId")]
    public User Owner { get; set; }
}
