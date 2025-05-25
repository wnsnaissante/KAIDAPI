using System.Security.Claims;
using KaidAPI.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KaidAPI.Controllers;

[Authorize]
[Route("api/v1/[controller]")]
[ApiController]
public class TeamController : ControllerBase
{
    private readonly ITeamService _teamService;

    public TeamController(ITeamService teamService)
    {
        _teamService = teamService ?? throw new ArgumentNullException(nameof(teamService));
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateTeam([FromBody] TeamRequest request) {
        var oidcSub = User.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        
        if (string.IsNullOrEmpty(oidcSub))
        {
            return Unauthorized("User does not have an access token.");
        }
        
        var result = await _teamService.CreateTeamAsync(request);
        return Ok(result);
    }

    [HttpPost("delete")]
    public async Task<IActionResult> DeleteTeam([FromQuery] Guid teamId) {
        var oidcSub = User.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        
        if (string.IsNullOrEmpty(oidcSub))
        {
            return Unauthorized("User does not have an access token.");
        }

        var result = await _teamService.DeleteTeamAsync(oidcSub, teamId);
        return Ok(result);
    }

    [HttpPost("update")]
    public async Task<IActionResult> UpdateTeam([FromQuery] Guid teamId, [FromBody] TeamRequest request) {
        var oidcSub = User.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        
        if (string.IsNullOrEmpty(oidcSub))
        {
            return Unauthorized("User does not have an access token.");
        }

        var result = await _teamService.UpdateTeamAsync(oidcSub, teamId, request);
        return Ok(result);
    }

    [HttpGet("get")]
    public async Task<IActionResult> GetTeams([FromQuery] Guid projectId)
    {
        var oidcSub = User.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        
        if (string.IsNullOrEmpty(oidcSub))
        {
            return Unauthorized("User does not have an access token.");
        }

        var result = await _teamService.GetTeamsByProjectId(oidcSub, projectId);
        return Ok(result);
    }
}
