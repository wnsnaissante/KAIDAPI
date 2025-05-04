using System.ComponentModel.DataAnnotations.Schema;

namespace KaidAPI.Models;

public class Project
{
    public string ProjectId { get; set; }
    public string ProjectName { get; set; }
    public string ProjectDescription { get; set; }
    public string OwnerID { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime DueDate { get; set; }
    
    [ForeignKey("OwnerID")]
    public User Owner { get; set; }
}