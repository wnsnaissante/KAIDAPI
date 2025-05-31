using System.Security.Claims;
using KaidAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KaidAPI.Controllers;

[Authorize]
[Route("api/v1/[controller]")]
[ApiController]
public class MembershipController : ControllerBase
{
    private readonly IMembershipService _membershipService;

    public MembershipController(IMembershipService membershipService)
    {
        _membershipService = membershipService ?? throw new ArgumentNullException(nameof(membershipService));
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateMembershipAsync(MemberRequest membershipRequest)
    {
        var oidcSub = User.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        if (string.IsNullOrEmpty(oidcSub))
        {
            return Unauthorized("User does not have an access token.");
        }
        
        var result = await _membershipService.CreateMembershipAsync(oidcSub, membershipRequest);

        if (result.Success)
        {
            return Ok();
        }
        return BadRequest(result.Message);
    }
    
    [HttpPut("delete")]
    public async Task<IActionResult> DeleteMembershipAsync([FromQuery] Guid membershipId)
    {
        var oidcSub = User.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        if (string.IsNullOrEmpty(oidcSub))
        {
            return Unauthorized("User does not have an access token.");
        }
        
        var result = await _membershipService.DeleteMembershipAsync(oidcSub, membershipId);

        if (result.Success)
        {
            return Ok();
        }
        return BadRequest(result.Message);
    }

    [HttpGet("get-members")]
    public async Task<IActionResult> GetMembersAsync([FromQuery] Guid projectId, [FromQuery] Guid teamId)
    {
        var oidcSub = User.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        if (string.IsNullOrEmpty(oidcSub))
        {
            return Unauthorized("User does not have an access token.");
        }

        var result = await _membershipService.GetMembersAsync(projectId, teamId);
        return Ok(result);
    }

    [HttpPut("update")]
    public async Task<IActionResult> UpdateMembershipAsync([FromQuery] Guid projectMembershipId, [FromBody] MemberRequest membershipRequest)
    {
        var oidcSub = User.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        if (string.IsNullOrEmpty(oidcSub))
        {
            return Unauthorized("User does not have an access token.");
        }

        var result = await _membershipService.UpdateMembershipAsync(oidcSub, projectMembershipId, membershipRequest);

        if (result.Success)
        {
            return Ok();
        }
        return BadRequest(result.Message);
    }


}