using System;
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

public class FlagControllerTests
{
    private readonly Mock<IFlagService> _flagServiceMock = new();
    private readonly FlagController _controller;

    public FlagControllerTests()
    {
        _controller = new FlagController(_flagServiceMock.Object);
    }

    private void SetUserOidcSub(string oidcSub)
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", oidcSub)
        }, "mock"));
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [Fact]
    public async Task CreateFlagAsync_ReturnsOkResult()
    {
        // Arrange
        var oidcSub = "user-oidc";
        SetUserOidcSub(oidcSub);

        var flagRequest = new FlagRequest();
        var flagId = Guid.NewGuid();

        _flagServiceMock.Setup(s => s.CreateFlagAsync(oidcSub, flagRequest))
            .ReturnsAsync(flagId);

        // Act
        var result = await _controller.CreateFlagAsync(flagRequest);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(flagId, okResult.Value);
    }

    [Fact]
    public async Task CreateFlagAsync_Unauthorized_WhenNoOidcSub()
    {
        // Arrange
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext() // No user set
        };

        var flagRequest = new FlagRequest();

        // Act
        var result = await _controller.CreateFlagAsync(flagRequest);

        // Assert
        var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal("User does not have an access token.", unauthorized.Value);
    }

    [Fact]
    public async Task DeleteFlagAsync_ReturnsOkResult()
    {
        var oidcSub = "oidc";
        SetUserOidcSub(oidcSub);

        var flagId = Guid.NewGuid();
        var operationResult = new OperationResult { Success = true, Message = "Deleted" };

        _flagServiceMock.Setup(s => s.DeleteFlagAsync(oidcSub, flagId))
            .ReturnsAsync(operationResult);

        var result = await _controller.DeleteFlagAsync(flagId);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(operationResult, okResult.Value);
    }

    [Fact]
    public async Task UpdateFlagAsync_ReturnsOkResult()
    {
        var oidcSub = "oidc";
        SetUserOidcSub(oidcSub);

        var flagId = Guid.NewGuid();
        var flagRequest = new FlagRequest();
        var operationResult = new OperationResult { Success = true };

        _flagServiceMock.Setup(s => s.UpdateFlagAsync(oidcSub, flagId, flagRequest))
            .ReturnsAsync(operationResult);

        var result = await _controller.UpdateFlagAsync(flagId, flagRequest);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(operationResult, okResult.Value);
    }

    [Fact]
    public async Task GetFlagByIdAsync_ReturnsOk_WhenSuccess()
    {
        var oidcSub = "oidc";
        SetUserOidcSub(oidcSub);

        var flagId = Guid.NewGuid();
        var flag = new Flag { FlagId = flagId };
        var operationResult = new OperationResult { Success = true, Data = flag };

        _flagServiceMock.Setup(s => s.GetFlagByIdAsync(oidcSub, flagId))
            .ReturnsAsync(operationResult);

        var result = await _controller.GetFlagByIdAsync(flagId);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(flag, okResult.Value);
    }

    [Fact]
    public async Task GetFlagByIdAsync_ReturnsForbid_WhenFailed()
    {
        var oidcSub = "oidc";
        SetUserOidcSub(oidcSub);

        var flagId = Guid.NewGuid();
        var operationResult = new OperationResult { Success = false, Message = "Denied" };

        _flagServiceMock.Setup(s => s.GetFlagByIdAsync(oidcSub, flagId))
            .ReturnsAsync(operationResult);

        var result = await _controller.GetFlagByIdAsync(flagId);

        var forbidResult = Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task GetFlagsByProjectAsync_ReturnsOk_WhenSuccess()
    {
        var oidcSub = "oidc";
        SetUserOidcSub(oidcSub);

        var projectId = Guid.NewGuid();
        var flags = new[] { new Flag(), new Flag() };
        var operationResult = new OperationResult { Success = true, Data = flags };

        _flagServiceMock.Setup(s => s.GetFlagsByProjectAsync(oidcSub, projectId))
            .ReturnsAsync(operationResult);

        var result = await _controller.GetFlagsByProjectAsync(projectId);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(flags, okResult.Value);
    }

    [Fact]
    public async Task GetRaisedFlagsCountAsync_ReturnsOk_WhenSuccess()
    {
        var oidcSub = "oidc";
        SetUserOidcSub(oidcSub);

        var projectId = Guid.NewGuid();
        int count = 5;
        var operationResult = new OperationResult { Success = true, Data = count };

        _flagServiceMock.Setup(s => s.GetRaisedFlagsCountAsync(oidcSub, projectId))
            .ReturnsAsync(operationResult);

        var result = await _controller.GetRaisedFlagsCountAsync(projectId);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(count, okResult.Value);
    }

    [Fact]
    public async Task GetSolvedFlagsCountAsync_ReturnsOk_WhenSuccess()
    {
        var oidcSub = "oidc";
        SetUserOidcSub(oidcSub);

        var projectId = Guid.NewGuid();
        int count = 3;
        var operationResult = new OperationResult { Success = true, Data = count };

        _flagServiceMock.Setup(s => s.GetSolvedFlagsCountAsync(oidcSub, projectId))
            .ReturnsAsync(operationResult);

        var result = await _controller.GetSolvedFlagsCountAsync(projectId);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(count, okResult.Value);
    }

    [Fact]
    public async Task GetUnsolvedFlagsCountAsync_ReturnsOk_WhenSuccess()
    {
        var oidcSub = "oidc";
        SetUserOidcSub(oidcSub);

        var projectId = Guid.NewGuid();
        int count = 7;
        var operationResult = new OperationResult { Success = true, Data = count };

        _flagServiceMock.Setup(s => s.GetUnsolvedFlagsCountAsync(oidcSub, projectId))
            .ReturnsAsync(operationResult);

        var result = await _controller.GetUnsolvedFlagsCountAsync(projectId);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(count, okResult.Value);
    }

    [Fact]
    public async Task GetFlagsByProjectAsync_ReturnsForbid_WhenFailed()
    {
        var oidcSub = "oidc";
        SetUserOidcSub(oidcSub);

        var projectId = Guid.NewGuid();
        var operationResult = new OperationResult { Success = false, Message = "Denied" };

        _flagServiceMock.Setup(s => s.GetFlagsByProjectAsync(oidcSub, projectId))
            .ReturnsAsync(operationResult);

        var result = await _controller.GetFlagsByProjectAsync(projectId);

        var forbidResult = Assert.IsType<ForbidResult>(result);
    }
}
