using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KaidAPI.Context;
using KaidAPI.Models;
using KaidAPI.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class CommentRepositoryTests : IDisposable
{
    private readonly ServerDbContext _context;
    private readonly CommentRepository _repository;

    public CommentRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ServerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new ServerDbContext(options);
        _repository = new CommentRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task CreateCommentAsync_ShouldAddComment()
    {
        var comment = new Comment
        {
            CommentId = Guid.NewGuid(),
            TaskId = Guid.NewGuid(),
            CommentText = "Test comment",
            CommentDate = DateTime.UtcNow
        };

        var result = await _repository.CreateCommentAsync(comment);

        Assert.True(result.Success);
        Assert.Equal("Comment created", result.Message);
        Assert.Equal(1, await _context.Comments.CountAsync());
        var saved = await _context.Comments.FirstAsync();
        Assert.Equal("Test comment", saved.CommentText);
    }

    [Fact]
    public async Task GetCommentById_ShouldReturnComment()
    {
        var comment = new Comment
        {
            CommentId = Guid.NewGuid(),
            TaskId = Guid.NewGuid(),
            CommentText = "Find me",
            CommentDate = DateTime.UtcNow
        };
        await _context.Comments.AddAsync(comment);
        await _context.SaveChangesAsync();

        var found = await _repository.GetCommentById(comment.CommentId);

        Assert.NotNull(found);
        Assert.Equal("Find me", found.CommentText);
    }

    [Fact]
    public async Task GetCommentsInTaskAsync_ShouldReturnAllForTask()
    {
        var taskId = Guid.NewGuid();

        var comments = new List<Comment>
        {
            new() { CommentId = Guid.NewGuid(), TaskId = taskId, CommentText = "C1", CommentDate = DateTime.UtcNow },
            new() { CommentId = Guid.NewGuid(), TaskId = taskId, CommentText = "C2", CommentDate = DateTime.UtcNow },
            new() { CommentId = Guid.NewGuid(), TaskId = Guid.NewGuid(), CommentText = "Other", CommentDate = DateTime.UtcNow }
        };
        await _context.Comments.AddRangeAsync(comments);
        await _context.SaveChangesAsync();

        var result = await _repository.GetCommentsInTaskAsync(taskId);

        Assert.Equal(2, result.Count);
        Assert.All(result, c => Assert.Equal(taskId, c.TaskId));
    }

    [Fact]
    public async Task UpdateCommentAsync_ShouldUpdateIfExists()
    {
        var comment = new Comment
        {
            CommentId = Guid.NewGuid(),
            TaskId = Guid.NewGuid(),
            CommentText = "Old Text",
            CommentDate = DateTime.UtcNow.AddDays(-1)
        };
        await _context.Comments.AddAsync(comment);
        await _context.SaveChangesAsync();

        var updatedComment = new Comment
        {
            CommentText = "New Text",
            CommentDate = DateTime.UtcNow
        };

        var result = await _repository.UpdateCommentAsync(comment.CommentId, updatedComment);

        Assert.True(result.Success);
        Assert.Equal("Comment updated", result.Message);

        var dbComment = await _context.Comments.FindAsync(comment.CommentId);
        Assert.Equal("New Text", dbComment.CommentText);
        Assert.Equal(updatedComment.CommentDate, dbComment.CommentDate);
    }

    [Fact]
    public async Task UpdateCommentAsync_ShouldFailIfNotExists()
    {
        var result = await _repository.UpdateCommentAsync(Guid.NewGuid(), new Comment());

        Assert.False(result.Success);
        Assert.Equal("Comment not found", result.Message);
    }

    [Fact]
    public async Task DeleteCommentAsync_ShouldRemoveComment()
    {
        var comment = new Comment
        {
            CommentId = Guid.NewGuid(),
            TaskId = Guid.NewGuid(),
            CommentText = "To be deleted",
            CommentDate = DateTime.UtcNow
        };
        await _context.Comments.AddAsync(comment);
        await _context.SaveChangesAsync();

        var result = await _repository.DeleteCommentAsync(comment.CommentId);

        Assert.True(result.Success);
        Assert.Equal("Comment deleted", result.Message);
        var found = await _context.Comments.FindAsync(comment.CommentId);
        Assert.Null(found);
    }

    [Fact]
    public async Task DeleteCommentAsync_ShouldFailIfNotFound()
    {
        var result = await _repository.DeleteCommentAsync(Guid.NewGuid());

        Assert.False(result.Success);
        Assert.Equal("Comment not found", result.Message);
    }
}
