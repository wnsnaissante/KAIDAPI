namespace KaidAPI.DTOs;

public class ProjectResponse
{
    public string ProjectName { get; set; }
    public string? ProjectDescription { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime DueDate { get; set; }
    public string? OwnerName { get; set; }
}