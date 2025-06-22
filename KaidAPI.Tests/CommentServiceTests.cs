using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KaidAPI.Models;
using KaidAPI.Repositories;
using KaidAPI.Services;
using KaidAPI.ViewModel;
using Moq;
using Xunit;

public class CommentServiceTests
{
    private readonly Mock<ICommentRepository> _mockCommentRepo;
    private readonly Mock<IUserRepository> _mockUserRepo;
    private readonly CommentService _service;

    public CommentServiceTests()
    {
        _mockCommentRepo = new Mock<ICommentRepository>();
        _mockUserRepo = new Mock<IUserRepository>();
        _service = new CommentService(_mockCommentRepo.Object, _mockUserRepo.Object);
    }

    [Fact]
    public async Task CreateCommentAsync_ReturnsSuccess_WhenUserExists()
    {
        var oidcSub = "user123";
        var user = new User { UserId = Guid.NewGuid() };
        var commentRequest = new CommentRequest
        {
            TaskId = Guid.NewGuid(),
            CommentText = "Hello"
        };
        _mockUserRepo.Setup(r => r.GetUserByOidcAsync(oidcSub)).ReturnsAsync(user);
        _mockCommentRepo.Setup(r => r.CreateCommentAsync(It.IsAny<Comment>()))
            .ReturnsAsync(new OperationResult { Success = true, Message = "Comment created" });

        var result = await _service.CreateCommentAsync(oidcSub, commentRequest);

        Assert.True(result.Success);
        Assert.Equal("Comment created", result.Message);
        _mockCommentRepo.Verify(r => r.CreateCommentAsync(It.Is<Comment>(c =>
            c.TaskId == commentRequest.TaskId &&
            c.CommentText == commentRequest.CommentText &&
            c.OwnerId == user.UserId
        )), Times.Once);
    }

    [Fact]
    public async Task CreateCommentAsync_ReturnsFail_WhenUserNotFound()
    {
        _mockUserRepo.Setup(r => r.GetUserByOidcAsync(It.IsAny<string>())).ReturnsAsync((User)null);

        var result = await _service.CreateCommentAsync("unknown", new CommentRequest());

        Assert.False(result.Success);
        Assert.Equal("User not found", result.Message);
    }

    [Fact]
    public async Task DeleteCommentAsync_ReturnsSuccess_WhenOwnerMatches()
    {
        var userId = Guid.NewGuid();
        var commentId = Guid.NewGuid();
        var oidcSub = "user123";

        var user = new User { UserId = userId };
        var comment = new Comment { CommentId = commentId, OwnerId = userId };

        _mockUserRepo.Setup(r => r.GetUserByOidcAsync(oidcSub)).ReturnsAsync(user);
        _mockCommentRepo.Setup(r => r.GetCommentById(commentId)).ReturnsAsync(comment);
        _mockCommentRepo.Setup(r => r.DeleteCommentAsync(commentId))
            .ReturnsAsync(new OperationResult { Success = true, Message = "Comment deleted" });

        var result = await _service.DeleteCommentAsync(oidcSub, commentId);

        Assert.True(result.Success);
        Assert.Equal("Comment deleted", result.Message);
    }

    [Fact]
    public async Task DeleteCommentAsync_ReturnsFail_WhenUserNotOwner()
    {
        var userId = Guid.NewGuid();
        var commentOwnerId = Guid.NewGuid();
        var commentId = Guid.NewGuid();
        var oidcSub = "user123";

        var user = new User { UserId = userId };
        var comment = new Comment { CommentId = commentId, OwnerId = commentOwnerId };

        _mockUserRepo.Setup(r => r.GetUserByOidcAsync(oidcSub)).ReturnsAsync(user);
        _mockCommentRepo.Setup(r => r.GetCommentById(commentId)).ReturnsAsync(comment);

        var result = await _service.DeleteCommentAsync(oidcSub, commentId);

        Assert.False(result.Success);
        Assert.Equal("You are not the owner of this comment", result.Message);
    }

    [Fact]
    public async Task DeleteCommentAsync_ReturnsFail_WhenCommentNotFound()
    {
        var oidcSub = "user123";
        var commentId = Guid.NewGuid();
        var user = new User { UserId = Guid.NewGuid() };

        _mockUserRepo.Setup(r => r.GetUserByOidcAsync(oidcSub)).ReturnsAsync(user);
        _mockCommentRepo.Setup(r => r.GetCommentById(commentId)).ReturnsAsync((Comment)null);

        var result = await _service.DeleteCommentAsync(oidcSub, commentId);

        Assert.False(result.Success);
        Assert.Equal("Comment not found", result.Message);
    }

    [Fact]
    public async Task UpdateCommentAsync_ReturnsSuccess_WhenOwnerMatches()
    {
        var userId = Guid.NewGuid();
        var commentId = Guid.NewGuid();
        var oidcSub = "user123";
        var existingComment = new Comment
        {
            CommentId = commentId,
            OwnerId = userId,
            TaskId = Guid.NewGuid(),
            CommentText = "Old text",
            CommentDate = DateTime.UtcNow.AddDays(-1)
        };

        var updateRequest = new CommentRequest
        {
            CommentText = "Updated text",
            TaskId = existingComment.TaskId
        };

        var user = new User { UserId = userId };

        _mockUserRepo.Setup(r => r.GetUserByOidcAsync(oidcSub)).ReturnsAsync(user);
        _mockCommentRepo.Setup(r => r.GetCommentById(commentId)).ReturnsAsync(existingComment);
        _mockCommentRepo.Setup(r => r.UpdateCommentAsync(commentId, It.IsAny<Comment>()))
            .ReturnsAsync(new OperationResult { Success = true, Message = "Comment updated" });

        var result = await _service.UpdateCommentAsync(oidcSub, commentId, updateRequest);

        Assert.True(result.Success);
        Assert.Equal("Comment updated", result.Message);
        _mockCommentRepo.Verify(r => r.UpdateCommentAsync(commentId, It.Is<Comment>(c =>
            c.CommentText == updateRequest.CommentText &&
            c.OwnerId == userId &&
            c.TaskId == existingComment.TaskId
        )), Times.Once);
    }

    [Fact]
    public async Task UpdateCommentAsync_ReturnsFail_WhenUserNotOwner()
    {
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var commentId = Guid.NewGuid();
        var oidcSub = "user123";
        var existingComment = new Comment
        {
            CommentId = commentId,
            OwnerId = otherUserId,
            TaskId = Guid.NewGuid(),
            CommentText = "Old text",
            CommentDate = DateTime.UtcNow.AddDays(-1)
        };
        var updateRequest = new CommentRequest
        {
            CommentText = "Updated text"
        };
        var user = new User { UserId = userId };

        _mockUserRepo.Setup(r => r.GetUserByOidcAsync(oidcSub)).ReturnsAsync(user);
        _mockCommentRepo.Setup(r => r.GetCommentById(commentId)).ReturnsAsync(existingComment);

        var result = await _service.UpdateCommentAsync(oidcSub, commentId, updateRequest);

        Assert.False(result.Success);
        Assert.Equal("You are not the owner of this comment", result.Message);
    }

    [Fact]
    public async Task UpdateCommentAsync_ReturnsFail_WhenCommentNotFound()
    {
        var oidcSub = "user123";
        var commentId = Guid.NewGuid();
        var updateRequest = new CommentRequest { CommentText = "Updated" };
        var user = new User { UserId = Guid.NewGuid() };

        _mockUserRepo.Setup(r => r.GetUserByOidcAsync(oidcSub)).ReturnsAsync(user);
        _mockCommentRepo.Setup(r => r.GetCommentById(commentId)).ReturnsAsync((Comment)null);

        var result = await _service.UpdateCommentAsync(oidcSub, commentId, updateRequest);

        Assert.False(result.Success);
        Assert.Equal("Comment not found", result.Message);
    }

    [Fact]
    public async Task GetCommentByCommentIdAsync_ReturnsComment_WhenFound()
    {
        var oidcSub = "user123";
        var commentId = Guid.NewGuid();
        var user = new User { UserId = Guid.NewGuid() };
        var comment = new Comment
        {
            CommentId = commentId,
            TaskId = Guid.NewGuid(),
            CommentText = "Hello",
            CommentDate = DateTime.UtcNow
        };

        _mockUserRepo.Setup(r => r.GetUserByOidcAsync(oidcSub)).ReturnsAsync(user);
        _mockCommentRepo.Setup(r => r.GetCommentById(commentId)).ReturnsAsync(comment);

        var result = await _service.GetCommentByCommentIdAsync(oidcSub, commentId);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        var response = Assert.IsType<CommentResponse>(result.Data);
        Assert.Equal(comment.TaskId, response.TaskId);
        Assert.Equal(comment.CommentText, response.CommentText);
    }

    [Fact]
    public async Task GetCommentByCommentIdAsync_ReturnsFail_WhenNotFound()
    {
        var oidcSub = "user123";
        var commentId = Guid.NewGuid();
        var user = new User { UserId = Guid.NewGuid() };

        _mockUserRepo.Setup(r => r.GetUserByOidcAsync(oidcSub)).ReturnsAsync(user);
        _mockCommentRepo.Setup(r => r.GetCommentById(commentId)).ReturnsAsync((Comment)null);

        var result = await _service.GetCommentByCommentIdAsync(oidcSub, commentId);

        Assert.False(result.Success);
        Assert.Equal("Comment not found", result.Message);
    }

    [Fact]
    public async Task GetCommentsInTaskAsync_ReturnsComments()
    {
        var oidcSub = "user123";
        var taskId = Guid.NewGuid();
        var user = new User { UserId = Guid.NewGuid() };
        var comments = new List<Comment>
        {
            new Comment { TaskId = taskId, CommentText = "C1", CommentDate = DateTime.UtcNow },
            new Comment { TaskId = taskId, CommentText = "C2", CommentDate = DateTime.UtcNow }
        };

        _mockUserRepo.Setup(r => r.GetUserByOidcAsync(oidcSub)).ReturnsAsync(user);
        _mockCommentRepo.Setup(r => r.GetCommentsInTaskAsync(taskId)).ReturnsAsync(comments);

        var result = await _service.GetCommentsInTaskAsync(oidcSub, taskId);

        Assert.True(result.Success);
        var data = Assert.IsType<List<CommentResponse>>(result.Data);
        Assert.Equal(2, data.Count);
        Assert.All(data, c => Assert.Equal(taskId, c.TaskId));
    }

    [Fact]
    public async Task GetCommentsInTaskAsync_ReturnsFail_WhenUserNotFound()
    {
        var oidcSub = "user123";
        var taskId = Guid.NewGuid();

        _mockUserRepo.Setup(r => r.GetUserByOidcAsync(oidcSub)).ReturnsAsync((User)null);

        var result = await _service.GetCommentsInTaskAsync(oidcSub, taskId);

        Assert.False(result.Success);
        Assert.Equal("User not found", result.Message);
    }
}
