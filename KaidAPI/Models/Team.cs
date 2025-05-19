namespace KaidAPI.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Team
{
    public Guid TeamId { get; set; }
    public Guid ProjectId { get; set; }
    public string TeamName { get; set; }
    public string Description { get; set; }

    public Guid LeaderId { get; set; }
    [ForeignKey("LeaderId")]
    public User Leader { get; set; }
}