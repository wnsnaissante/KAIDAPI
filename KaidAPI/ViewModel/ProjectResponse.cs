namespace KaidAPI.ViewModel;

public class ProjectResponse
{
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; }
    public string? ProjectDescription { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DueDate { get; set; }
    public Guid OwnerId { get; set; }
}