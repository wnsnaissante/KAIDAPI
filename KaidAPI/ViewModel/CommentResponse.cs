namespace KaidAPI.ViewModel;

public class CommentResponse
{
    public string OwnerName { get; set; }
    public Guid TaskId { get; set; }
    public string CommentText { get; set; }
    public DateTime CommentDate { get; set; }
}


