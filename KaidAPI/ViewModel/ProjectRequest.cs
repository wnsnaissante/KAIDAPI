namespace KaidAPI.ViewModel;

public class ProjectRequest
{
    public string ProjectName { get; set; }
    public string ProjectDescription { get; set; }
    public Guid? ProjectId { get; set; }
    public DateTime? DueDate { get; set; }
}