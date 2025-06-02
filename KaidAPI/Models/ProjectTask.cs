namespace KaidAPI.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

public class ProjectTask
{
    [Key]
    public Guid TaskId { get; set; }
    public string TaskName { get; set; }
    public string TaskDescription { get; set; }
    public Guid Assignee { get; set; }
    public int StatusId { get; set; }
    public int Priority { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public Guid ProjectId { get; set; }

    [ForeignKey("ProjectId")]
    [JsonIgnore]
    public Project? Project { get; set; }

    public Guid TeamId { get; set; }

    [ForeignKey("TeamId")]
    [JsonIgnore]
    public Team? Team { get; set; }
}