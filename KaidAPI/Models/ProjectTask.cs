namespace KaidAPI.Models;
using System.ComponentModel.DataAnnotations;

public class ProjectTask
{
    public string TaskId { get; set; }
    public string TaskName { get; set; }
    public string TaskDescription { get; set; }
    public Guid Assignee { get; set; }
    public string StatusId { get; set; }
    public int Priority { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}