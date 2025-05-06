using System.Security.Claims;
using KaidAPI.Services;
using KaidAPI.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KaidAPI.Controllers;

[Authorize]
[Route("api/v1/[controller]")]
[ApiController]
public class ProjectController : ControllerBase
{
    private readonly IProjectService _projectService;
    public ProjectController(IProjectService projectService)
    {
        _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateProject([FromBody] ProjectRequest projectRequest)
    {

        var oidcSub = User.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        
        if (string.IsNullOrEmpty(oidcSub))
        {
            return Unauthorized("User does not have an access token.");
        }

        var result = await _projectService.CreateNewProjectAsync(projectRequest, oidcSub);

        if (result.Success)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }
    
    [HttpGet("test")]
    public IActionResult TestAuth()
    {
        var isAuth = HttpContext.User.Identity?.IsAuthenticated;
        var name = HttpContext.User.Identity?.Name;
        return Ok(new { isAuth, name });
    }
}