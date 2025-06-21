using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using KaidAPI.Controllers;
using KaidAPI.Models;
using KaidAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace KaidAPI.Tests
{
    public class MembershipControllerTests
    {
        private readonly Mock<IMembershipService> _mockService;
        private readonly string _mockOidcSub = "test-user-sub";

        public MembershipControllerTests()
        {
            _mockService = new Mock<IMembershipService>();
        }

        private MembershipController CreateControllerWithClaims()
        {
            var controller = new MembershipController(_mockService.Object);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", _mockOidcSub)
            }, "mock"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
            return controller;
        }

        [Fact]
        public async Task DeleteMembershipAsync_ReturnsOkOnSuccess()
        {
            var membershipId = Guid.NewGuid();
            _mockService.Setup(s => s.DeleteMembershipAsync(_mockOidcSub, membershipId))
                .ReturnsAsync(new OperationResult { Success = true });

            var controller = CreateControllerWithClaims();
            var result = await controller.DeleteMembershipAsync(membershipId);

            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task GetMembershipAsync_ReturnsOkWithResult()
        {
            var expectedMembership = new Membership { ProjectMembershipId = Guid.NewGuid() };
            var operationResult = new OperationResult { Success = true, Data = expectedMembership };

            _mockService.Setup(s => s.GetMembershipAsync(_mockOidcSub, It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(operationResult);

            var controller = CreateControllerWithClaims();
            var actionResult = await controller.GetMembershipAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

            var result = Assert.IsType<OkObjectResult>(actionResult);
            var opResult = Assert.IsType<OperationResult>(result.Value);
            var actualMembership = Assert.IsType<Membership>(opResult.Data);

            Assert.Equal(expectedMembership.ProjectMembershipId, actualMembership.ProjectMembershipId);
        }

        [Fact]
        public async Task UpdateMembershipAsync_ReturnsOkOnSuccess()
        {
            var projectMembershipId = Guid.NewGuid();
            var request = new MemberRequest();
            _mockService.Setup(s => s.UpdateMembershipAsync(_mockOidcSub, projectMembershipId, request))
                .ReturnsAsync(new OperationResult { Success = true });

            var controller = CreateControllerWithClaims();
            var result = await controller.UpdateMembershipAsync(projectMembershipId, request);

            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task AcceptInvitationAsync_ReturnsOkOnSuccess()
        {
            var projectMembershipId = Guid.NewGuid();
            _mockService.Setup(s => s.AcceptInvitationAsync(_mockOidcSub, projectMembershipId))
                .ReturnsAsync(new OperationResult { Success = true });

            var controller = CreateControllerWithClaims();
            var result = await controller.AcceptInvitationAsync(projectMembershipId);

            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task DenyInvitationAsync_ReturnsOkOnSuccess()
        {
            var projectMembershipId = Guid.NewGuid();
            _mockService.Setup(s => s.DenyInvitationAsync(_mockOidcSub, projectMembershipId))
                .ReturnsAsync(new OperationResult { Success = true });

            var controller = CreateControllerWithClaims();
            var result = await controller.DenyInvitationAsync(projectMembershipId);

            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task GetMembersAsync_ReturnsOkWithResult()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var teamId = Guid.NewGuid();
            var members = new List<Membership>
            {
                new Membership { ProjectMembershipId = Guid.NewGuid() },
                new Membership { ProjectMembershipId = Guid.NewGuid() }
            };

            var operationResult = new OperationResult
            {
                Success = true,
                Data = members
            };

            _mockService.Setup(s => s.GetMembersAsync(_mockOidcSub, projectId, teamId))
                .ReturnsAsync(operationResult);

            var controller = CreateControllerWithClaims();

            // Act
            var result = await controller.GetMembersAsync(projectId, teamId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(operationResult, okResult.Value);
        }
    }
}