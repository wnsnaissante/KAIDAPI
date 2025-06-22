using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using KaidAPI.Controllers;
using KaidAPI.Services;
using KaidAPI.Models;
using KaidAPI.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace KaidAPI.Tests
{
    public class CommentControllerTests
    {
        private readonly Mock<ICommentService> _mockCommentService;
        private readonly CommentController _controller;

        public CommentControllerTests()
        {
            _mockCommentService = new Mock<ICommentService>();
            _controller = new CommentController(_mockCommentService.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", "test-user-id")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Fact]
        public async Task CreateComment_ReturnsOk_WhenSuccess()
        {
            var request = new CommentRequest { CommentText = "Test", TaskId = Guid.NewGuid() };
            _mockCommentService.Setup(x => x.CreateCommentAsync("test-user-id", request))
                .ReturnsAsync(new OperationResult { Success = true, Message = "Created" });

            var result = await _controller.CreateComment(request);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Created", okResult.Value);
        }

        [Fact]
        public async Task DeleteComment_ReturnsOk_WhenSuccess()
        {
            var commentId = Guid.NewGuid();
            _mockCommentService.Setup(x => x.DeleteCommentAsync("test-user-id", commentId))
                .ReturnsAsync(new OperationResult { Success = true, Message = "Deleted" });

            var result = await _controller.DeleteComment(commentId);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Deleted", okResult.Value);
        }

        [Fact]
        public async Task UpdateComment_ReturnsOk_WhenSuccess()
        {
            var commentId = Guid.NewGuid();
            var request = new CommentRequest { CommentText = "Updated", TaskId = Guid.NewGuid() };
            _mockCommentService.Setup(x => x.UpdateCommentAsync("test-user-id", commentId, request))
                .ReturnsAsync(new OperationResult { Success = true, Message = "Updated" });

            var result = await _controller.UpdateComment(commentId, request);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Updated", okResult.Value);
        }

        [Fact]
        public async Task GetCommentByCommentId_ReturnsComment_WhenSuccess()
        {
            var commentId = Guid.NewGuid();
            var response = new CommentResponse
            {
                OwnerName = "È«±æµ¿",
                TaskId = Guid.NewGuid(),
                CommentText = "³»¿ë",
                CommentDate = DateTime.UtcNow
            };
            _mockCommentService.Setup(x => x.GetCommentByCommentIdAsync("test-user-id", commentId))
                .ReturnsAsync(new OperationResult { Success = true, Data = response });

            var result = await _controller.GetCommentByCommentId(commentId);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(response, okResult.Value);
        }

        [Fact]
        public async Task GetCommentsInTask_ReturnsList_WhenSuccess()
        {
            var taskId = Guid.NewGuid();
            var responses = new List<CommentResponse>
            {
                new CommentResponse
                {
                    OwnerName = "È«±æµ¿",
                    TaskId = taskId,
                    CommentText = "³»¿ë",
                    CommentDate = DateTime.UtcNow
                }
            };
            _mockCommentService.Setup(x => x.GetCommentsInTaskAsync("test-user-id", taskId))
                .ReturnsAsync(new OperationResult { Success = true, Data = responses });

            var result = await _controller.GetCommentsInTask(taskId);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(responses, okResult.Value);
        }
    }
}