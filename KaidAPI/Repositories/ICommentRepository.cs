using KaidAPI.Models;

namespace KaidAPI.Repositories;

public interface ICommentRepository
{
    Task<OperationResult> CreateCommentAsync(Comment comment);
    Task<OperationResult> DeleteCommentAsync(Guid commentId);
    Task<Comment> GetCommentById(Guid commentId);
    Task<List<Comment>> GetCommentsInTaskAsync(Guid taskId);
    Task<OperationResult> UpdateCommentAsync(Guid commentId, Comment comment);
}