using KaidAPI.Models;
using KaidAPI.Repositories;
using KaidAPI.ViewModel;

namespace KaidAPI.Services;

public class CommentService : ICommentService
{
    private readonly ICommentRepository _commentRepository;
    private readonly IUserRepository _userRepository;

    public CommentService(ICommentRepository commentRepository, IUserRepository userRepository)
    {
        _commentRepository = commentRepository ?? throw new ArgumentNullException(nameof(commentRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    public async Task<OperationResult> CreateCommentAsync(string oidcSub, CommentRequest comment)
    {
        var user = await _userRepository.GetUserByOidcAsync(oidcSub);
        if (user == null)
        {
            return new OperationResult { Success = false, Message = "User not found" };
        }
        var userId = user.UserId;
        
        var newComment = new Comment
        {
            CommentId = Guid.NewGuid(),
            TaskId = comment.TaskId,
            OwnerId = userId,
            CommentText = comment.CommentText,
            CommentDate = DateTime.UtcNow
        };

        return await _commentRepository.CreateCommentAsync(newComment);
    }

    public async Task<OperationResult> DeleteCommentAsync(string oidcSub, Guid commentId)
    {
        var user = await _userRepository.GetUserByOidcAsync(oidcSub);
        if (user == null)
        {
            return new OperationResult { Success = false, Message = "User not found" };
        }
        var userId = user.UserId;

        var comment = await _commentRepository.GetCommentById(commentId);
        if (comment == null)
        {
            return new OperationResult { Success = false, Message = "Comment not found" };
        }

        if (comment.OwnerId != userId)
        {
            return new OperationResult { Success = false, Message = "You are not the owner of this comment" };
        }

        return await _commentRepository.DeleteCommentAsync(commentId);
    }

    public async Task<OperationResult> UpdateCommentAsync(string oidcSub, Guid commentId, CommentRequest commentRequest)
    {
        var user = await _userRepository.GetUserByOidcAsync(oidcSub);
        if (user == null)
        {
            return new OperationResult { Success = false, Message = "User not found" };
        }
        var userId = user.UserId;

        var existingComment = await _commentRepository.GetCommentById(commentId);
        if (existingComment == null)
        {
            return new OperationResult { Success = false, Message = "Comment not found" };
        }

        if (existingComment.OwnerId != userId)
        {
            return new OperationResult { Success = false, Message = "You are not the owner of this comment" };
        }

        var updatedComment = new Comment
        {
            CommentId = existingComment.CommentId,
            TaskId = existingComment.TaskId,
            OwnerId = existingComment.OwnerId,
            CommentText = commentRequest.CommentText,
            CommentDate = existingComment.CommentDate
        };

        return await _commentRepository.UpdateCommentAsync(commentId, updatedComment);
    }

    public async Task<OperationResult> GetCommentByCommentIdAsync(string oidcSub, Guid commentId)
    {
        var user = await _userRepository.GetUserByOidcAsync(oidcSub);
        if (user == null)
        {
            return new OperationResult { Success = false, Message = "User not found" };
        }

        var comment = await _commentRepository.GetCommentById(commentId);
        if (comment == null)
        {
            return new OperationResult { Success = false, Message = "Comment not found" };
        }

        return new OperationResult { Success = true, Data = new CommentResponse
        {
            TaskId = comment.TaskId,
            CommentText = comment.CommentText,
            CommentDate = comment.CommentDate
        }
        };
    }

    public async Task<OperationResult> GetCommentsInTaskAsync(string oidcSub, Guid taskId)
    {
        var user = await _userRepository.GetUserByOidcAsync(oidcSub);
        if (user == null)
        {
            return new OperationResult { Success = false, Message = "User not found" };
        }

        var comments = await _commentRepository.GetCommentsInTaskAsync(taskId);
        return new OperationResult { Success = true, Data = comments.Select(c => new CommentResponse
        {
            TaskId = c.TaskId,
            CommentText = c.CommentText,
            CommentDate = c.CommentDate
        }).ToList() };
    }
}

