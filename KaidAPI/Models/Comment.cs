namespace KaidAPI.Models;

public class Comment
{
    public Guid CommentId { get; set; }
    public Guid TaskId { get; set; }
    public Guid OwnerId { get; set; }
    public string CommentText { get; set; }
    public DateTime CommentDate { get; set; }
}