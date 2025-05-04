namespace KaidAPI.Models;

public class Task
{
    public string TaskID { get; set; }
    public string TaskName { get; set; }
    public string TaskDescription { get; set; }
    public Guid Assignee { get; set; }
    public string StatusID { get; set; }
    public int Priority { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}