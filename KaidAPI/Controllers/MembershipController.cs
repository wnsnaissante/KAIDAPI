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

    // [HttpGet("get")]
    // public async Task<IActionResult> GetMembershipsAsync([FromQuery] Guid projectId)
    // {
    //     var oidcSub = User.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
    //     if (string.IsNullOrEmpty(oidcSub))
    //     {
    //         return Unauthorized("User does not have an access token.");
    //     }
    //     
    //     
    // }
    //
    // [HttpPut("update")]
    // public async Task<IActionResult> UpdateMembershipAsync(MemberRequest membershipRequest)
    // {
    //     var oidcSub = User.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
    //     if (string.IsNullOrEmpty(oidcSub))
    //     {
    //         return Unauthorized("User does not have an access token.");
    //     }
    //     
    //     var result = await _membershipService.;
    // }

    
}