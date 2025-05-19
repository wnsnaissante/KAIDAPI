namespace KaidAPI.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class ProjectTask
{
    [Key]
    public string TaskId { get; set; }
    public string TaskName { get; set; }
    public string TaskDescription { get; set; }
    public Guid Assignee { get; set; }
    public string StatusId { get; set; }
    public int Priority { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Guid ProjectId { get; set; }
    [ForeignKey("ProjectId")]
    public Project Project { get; set; }

    public Guid TeamId { get; set; }
    [ForeignKey("TeamId")]
    public Team Team { get; set; }
}