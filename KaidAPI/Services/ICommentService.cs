using KaidAPI.Models;
using KaidAPI.ViewModel;

namespace KaidAPI.Services;

public interface ICommentService
{
    Task<OperationResult> CreateCommentAsync(string oidcSub, CommentRequest comment);
    Task<OperationResult> DeleteCommentAsync(string oidcSub, Guid commentId);
    Task<OperationResult> UpdateCommentAsync(string oidcSub, Guid commentId, CommentRequest comment);
    Task<OperationResult> GetCommentByCommentIdAsync(string oidcSub, Guid commentId);
    Task<OperationResult> GetCommentsInTaskAsync(string oidcSub, Guid taskId);
}

