using KaidAPI.Context;
using KaidAPI.Models;
using KaidAPI.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace KaidAPI.Repositories;

public class CommentRepository : ICommentRepository
{
    private readonly ServerDbContext _context;

    public CommentRepository(ServerDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }
    
    public async Task<OperationResult> CreateCommentAsync(Comment comment)
    {
        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();
        return new OperationResult
        {
            Success = true,
            Message = "Comment created"
        };
    }

    public async Task<OperationResult> DeleteCommentAsync(Guid commentId)
    {
        var comment = await _context.Comments.FindAsync(commentId);
        if (comment == null)
        {
            return new OperationResult
            {
                Success = false,
                Message = "Comment not found"
            };
        }
        
        _context.Comments.Remove(comment);
        await _context.SaveChangesAsync();
        return new OperationResult
        {
            Success = true,
            Message = "Comment deleted"
        };
    }

    public async Task<Comment> GetCommentById(Guid commentId)
    {
        return await _context.Comments.FindAsync(commentId);
    }

    public async Task<List<Comment>> GetCommentsInTaskAsync(Guid taskId)
    {
        return await _context.Comments.Where(c => c.TaskId == taskId).ToListAsync();
    }

    public async Task<OperationResult> UpdateCommentAsync(Guid commentId, Comment comment)
    {
        var existingComment = await _context.Comments.FindAsync(commentId);
        if (existingComment == null)
        {
            return new OperationResult
            {
                Success = false,
                Message = "Comment not found"
            };
        }

        existingComment.CommentText = comment.CommentText;
        existingComment.CommentDate = comment.CommentDate;
        _context.Comments.Update(existingComment);
        await _context.SaveChangesAsync();
        
        return new OperationResult
        {
            Success = true,
            Message = "Comment updated"
        };
    }
}