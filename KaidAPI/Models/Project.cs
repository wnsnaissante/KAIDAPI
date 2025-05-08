using System.ComponentModel.DataAnnotations.Schema;

namespace KaidAPI.Models;

public class Project
{
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; }
    public string ProjectDescription { get; set; }
    public Guid OwnerId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DueDate { get; set; }
    
    [ForeignKey("OwnerId")]
    public User Owner { get; set; }
}