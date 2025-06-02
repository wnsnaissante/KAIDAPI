namespace KaidAPI.ViewModel.Tasks;

public class AvailableTask
{
    public Guid TaskId { get; set; }
    public string TaskName { get; set; }
    public string TaskDescription { get; set; }
    public int Priority { get; set; }
    public DateTime DueDate { get; set; }
}
