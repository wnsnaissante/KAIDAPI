using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using KaidAPI.Controllers;
using KaidAPI.Models;
using KaidAPI.Services;
using KaidAPI.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using KaidAPI.ViewModel.Tasks;

namespace KaidAPI.Tests
{
    public class ProjectTaskControllerTests
    {
        private readonly Mock<IProjectTaskService> mockTaskService;
        private readonly ProjectTaskController controller;

        public ProjectTaskControllerTests()
        {
            mockTaskService = new Mock<IProjectTaskService>();
            controller = new ProjectTaskController(mockTaskService.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", "test-oidc-sub")
            }, "mock"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Fact]
        public async Task CreateTask_ReturnsOk_WithTaskId()
        {
            var request = new ProjectTaskRequest
            {
                TaskName = "Test Task",
                ProjectId = Guid.NewGuid(),
                TeamId = Guid.NewGuid(),
                DueDate = DateTime.UtcNow.AddDays(7)
            };

            var expectedTaskId = Guid.NewGuid();
            mockTaskService.Setup(x => x.CreateProjectTaskAsync(request))
                .ReturnsAsync(expectedTaskId);

            var result = await controller.CreateTask(request);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsAssignableFrom<object>(okResult.Value);

            // okResult.Value는 익명 객체 { TaskId = Guid }
            var taskIdProperty = value.GetType().GetProperty("TaskId");
            Assert.NotNull(taskIdProperty);
            var actualTaskId = (Guid)taskIdProperty.GetValue(value);
            Assert.Equal(expectedTaskId, actualTaskId);
        }

        [Fact]
        public async Task CreateTask_ReturnsUnauthorized_WhenNoOidcSub()
        {
            controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity()); // empty user

            var request = new ProjectTaskRequest();
            var result = await controller.CreateTask(request);

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task GetTaskById_ReturnsOk_WhenTaskExists()
        {
            var taskId = Guid.NewGuid();
            var task = new ProjectTask { TaskId = taskId, TaskName = "Sample" };

            mockTaskService.Setup(x => x.GetProjectTaskByIdAsync(taskId))
                .ReturnsAsync(task);

            var result = await controller.GetTaskById(taskId);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(task, okResult.Value);
        }

        [Fact]
        public async Task GetTaskById_ReturnsNotFound_WhenTaskDoesNotExist()
        {
            var taskId = Guid.NewGuid();

            mockTaskService.Setup(x => x.GetProjectTaskByIdAsync(taskId))
                .ReturnsAsync((ProjectTask?)null);

            var result = await controller.GetTaskById(taskId);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task UpdateTask_ReturnsNoContent_OnSuccess()
        {
            var taskId = Guid.NewGuid();
            var updatedTask = new ProjectTask { TaskName = "Updated" };

            mockTaskService.Setup(x => x.UpdateProjectTaskAsync(It.IsAny<ProjectTask>(), "test-oidc-sub"))
                .ReturnsAsync(new OperationResult { Success = true });

            var result = await controller.UpdateTask(taskId, updatedTask);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task UpdateTask_ReturnsBadRequest_OnFailure()
        {
            var taskId = Guid.NewGuid();
            var updatedTask = new ProjectTask { TaskName = "Fail Update" };

            mockTaskService.Setup(x => x.UpdateProjectTaskAsync(It.IsAny<ProjectTask>(), "test-oidc-sub"))
                .ReturnsAsync(new OperationResult { Success = false, Message = "Failed to update" });

            var result = await controller.UpdateTask(taskId, updatedTask);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Failed to update", badRequest.Value);
        }

        [Fact]
        public async Task DeleteTask_ReturnsNoContent_OnSuccess()
        {
            var taskId = Guid.NewGuid();

            mockTaskService.Setup(x => x.DeleteProjectTaskAsync(taskId, "test-oidc-sub"))
                .ReturnsAsync(new OperationResult { Success = true });

            var result = await controller.DeleteTask(taskId);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteTask_ReturnsBadRequest_OnFailure()
        {
            var taskId = Guid.NewGuid();

            mockTaskService.Setup(x => x.DeleteProjectTaskAsync(taskId, "test-oidc-sub"))
                .ReturnsAsync(new OperationResult { Success = false, Message = "Failed to delete" });

            var result = await controller.DeleteTask(taskId);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Failed to delete", badRequest.Value);
        }

        // 이하 예시: ProjectTaskController의 여러 Get 분기 메서드에 대한 기본 성공 테스트들

        [Fact]
        public async Task GetProjectTaskDistribution_ReturnsOk()
        {
            mockTaskService.Setup(x => x.GetProjectTaskDistributionAsync("test-oidc-sub", It.IsAny<Guid>()))
                .ReturnsAsync(new List<TaskDistribution>());

            var result = await controller.GetProjectTaskDistribution(Guid.NewGuid());

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetTaskPriorityDistribution_ReturnsOk()
        {
            mockTaskService.Setup(x => x.GetTaskPriorityDistributionAsync("test-oidc-sub", It.IsAny<Guid>()))
                .ReturnsAsync(new OperationResult { Success = true });

            var result = await controller.GetTaskPriorityDistribution(Guid.NewGuid());

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetAvailableTasks_ReturnsOk()
        {
            mockTaskService.Setup(x => x.GetAvailableTasksAsync("test-oidc-sub", It.IsAny<Guid>()))
                .ReturnsAsync(new List<AvailableTask>());

            var result = await controller.GetAvailableTasks(Guid.NewGuid());

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetTeamTaskWorkload_ReturnsOk()
        {
            mockTaskService.Setup(x => x.GetTeamTaskWorkloadAsync("test-oidc-sub", It.IsAny<Guid>()))
                .ReturnsAsync(new List<TaskWorkload>());

            var result = await controller.GetTeamTaskWorkload(Guid.NewGuid());

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetCompletedTasksPastWeek_ReturnsOk()
        {
            mockTaskService.Setup(x => x.GetCompletedTasksPastWeekAsync("test-oidc-sub", It.IsAny<Guid>()))
                .ReturnsAsync(10);

            var result = await controller.GetCompletedTasksPastWeek(Guid.NewGuid());

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetUncompletedTasksPastWeek_ReturnsOk()
        {
            mockTaskService.Setup(x => x.GetUncompletedTasksPastWeekAsync("test-oidc-sub", It.IsAny<Guid>()))
                .ReturnsAsync(5);

            var result = await controller.GetUncompletedTasksPastWeek(Guid.NewGuid());

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetLeftTasks_ReturnsOk()
        {
            mockTaskService.Setup(x => x.GetLeftTasksCountAsync("test-oidc-sub", It.IsAny<Guid>()))
                .ReturnsAsync(7);

            var result = await controller.GetLeftTasks(Guid.NewGuid());

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetUrgentTasks_ReturnsOk()
        {
            mockTaskService.Setup(x => x.GetUrgentTasksCountAsync("test-oidc-sub", It.IsAny<Guid>()))
                .ReturnsAsync(3);

            var result = await controller.GetUrgentTasks(Guid.NewGuid());

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetAssignedTasks_ReturnsOk()
        {
            mockTaskService.Setup(x => x.GetAssignedTasksAsync("test-oidc-sub", It.IsAny<Guid>()))
                .ReturnsAsync(new List<TaskPreview>());

            var result = await controller.GetAssignedTasks(Guid.NewGuid());

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task Methods_ReturnUnauthorized_IfOidcSubMissing()
        {
            controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity()); // empty user

            var taskId = Guid.NewGuid();

            Assert.IsType<UnauthorizedObjectResult>(await controller.CreateTask(new ProjectTaskRequest()));
            Assert.IsType<UnauthorizedObjectResult>(await controller.UpdateTask(taskId, new ProjectTask()));
            Assert.IsType<UnauthorizedObjectResult>(await controller.DeleteTask(taskId));
            Assert.IsType<UnauthorizedObjectResult>(await controller.GetProjectTaskDistribution(Guid.NewGuid()));
            Assert.IsType<UnauthorizedObjectResult>(await controller.GetTaskPriorityDistribution(Guid.NewGuid()));
            Assert.IsType<UnauthorizedObjectResult>(await controller.GetAvailableTasks(Guid.NewGuid()));
            Assert.IsType<UnauthorizedObjectResult>(await controller.GetTeamTaskWorkload(Guid.NewGuid()));
            Assert.IsType<UnauthorizedObjectResult>(await controller.GetCompletedTasksPastWeek(Guid.NewGuid()));
            Assert.IsType<UnauthorizedObjectResult>(await controller.GetUncompletedTasksPastWeek(Guid.NewGuid()));
            Assert.IsType<UnauthorizedObjectResult>(await controller.GetLeftTasks(Guid.NewGuid()));
            Assert.IsType<UnauthorizedObjectResult>(await controller.GetUrgentTasks(Guid.NewGuid()));
            Assert.IsType<UnauthorizedObjectResult>(await controller.GetAssignedTasks(Guid.NewGuid()));
            Assert.IsType<UnauthorizedObjectResult>(await controller.GetTaskById(taskId));
        }
    }
}
