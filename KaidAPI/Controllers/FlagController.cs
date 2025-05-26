using System.Security.Claims;
using KaidAPI.Models;
using KaidAPI.Services;
using KaidAPI.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KaidAPI.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class FlagController : ControllerBase
{
    private readonly IFlagService _flagService;

    public FlagController(IFlagService flagService)
    {
        _flagService = flagService ?? throw new ArgumentNullException(nameof(flagService));
    }

    public async Task<IActionResult> CreateFlagAsync([FromBody] FlagRequest flag)
    {
        var oidcSub = User.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        if (string.IsNullOrEmpty(oidcSub))
        {
            return Unauthorized("User does not have an access token.");
        }
        var result = await _flagService.CreateFlagAsync(flag);
        return Ok(result);
    }

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
}