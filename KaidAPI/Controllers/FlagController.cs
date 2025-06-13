using System.Security.Claims;
using KaidAPI.Models;
using KaidAPI.Services;
using KaidAPI.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KaidAPI.Controllers;

[Authorize]
[Route("api/v1/[controller]")]
[ApiController]
public class FlagController : ControllerBase
{
    private readonly IFlagService _flagService;

    public FlagController(IFlagService flagService)
    {
        _flagService = flagService ?? throw new ArgumentNullException(nameof(flagService));
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateFlagAsync([FromBody] FlagRequest flag)
    {
        var oidcSub = User.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        if (string.IsNullOrEmpty(oidcSub))
        {
            return Unauthorized("User does not have an access token.");
        }
        var result = await _flagService.CreateFlagAsync(oidcSub, flag);
        return Ok(result);
    }

    [HttpDelete("delete")]
    public async Task<IActionResult> DeleteFlagAsync([FromQuery] Guid flagId)
    {
        var oidcSub = User.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        if (string.IsNullOrEmpty(oidcSub))
        {
            return Unauthorized("User does not have an access token.");
        }
        var result = await _flagService.DeleteFlagAsync(oidcSub, flagId);
        return Ok(result);
    }

    [HttpPost("update")]
    public async Task<IActionResult> UpdateFlagAsync([FromQuery] Guid flagId, [FromBody] FlagRequest flagRequest)
    {
        var oidcSub = User.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        if (string.IsNullOrEmpty(oidcSub))
        {
            return Unauthorized("User does not have an access token.");
        }
        var result = await _flagService.UpdateFlagAsync(oidcSub, flagId, flagRequest);
        return Ok(result);
    }

    [HttpGet("get-by-id")]
    public async Task<IActionResult> GetFlagByIdAsync([FromQuery] Guid flagId)
    {
        var oidcSub = User.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        if (string.IsNullOrEmpty(oidcSub))
            return Unauthorized("User does not have an access token.");

        var result = await _flagService.GetFlagByIdAsync(oidcSub, flagId);
        if (!result.Success) return Forbid(result.Message);
        return Ok(result.Data);
    }

    [HttpGet("get-by-project")]
    public async Task<IActionResult> GetFlagsByProjectAsync([FromQuery] Guid projectId)
    {
        var oidcSub = User.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        if (string.IsNullOrEmpty(oidcSub))
            return Unauthorized("User does not have an access token.");

        var result = await _flagService.GetFlagsByProjectAsync(oidcSub, projectId);
        if (!result.Success) return Forbid(result.Message);
        return Ok(result.Data);
    }

    [HttpGet("get-raised-flags-count")]
    public async Task<IActionResult> GetRaisedFlagsCountAsync([FromQuery] Guid projectId)
    {
        var oidcSub = User.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        if (string.IsNullOrEmpty(oidcSub))
            return Unauthorized("User does not have an access token.");

        var result = await _flagService.GetRaisedFlagsCountAsync(oidcSub, projectId);
        if (!result.Success) return Forbid(result.Message);
        return Ok(result.Data);
    }

    [HttpGet("get-solved-flags-count")]
    public async Task<IActionResult> GetSolvedFlagsCountAsync([FromQuery] Guid projectId)
    {
        var oidcSub = User.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        if (string.IsNullOrEmpty(oidcSub))
            return Unauthorized("User does not have an access token.");

        var result = await _flagService.GetSolvedFlagsCountAsync(oidcSub, projectId);
        if (!result.Success) return Forbid(result.Message);
        return Ok(result.Data);
    }

    [HttpGet("get-unsolved-flags-count")]
    public async Task<IActionResult> GetUnsolvedFlagsCountAsync([FromQuery] Guid projectId)
    {
        var oidcSub = User.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        if (string.IsNullOrEmpty(oidcSub))
            return Unauthorized("User does not have an access token.");

        var result = await _flagService.GetUnsolvedFlagsCountAsync(oidcSub, projectId);
        if (!result.Success) return Forbid(result.Message);
        return Ok(result.Data);
    }

}