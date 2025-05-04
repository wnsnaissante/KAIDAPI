using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KaidAPI.Controllers;

[ApiController]
[Authorize]
public class UserInfoController : ControllerBase
{
    [HttpGet] 
    [Route("api/userinfo")]
    public IActionResult GetUserInfo()
    {
        var user = HttpContext.User;
        var name = user.FindFirst("name")?.Value ?? "'name' claim not found";
        var email = user.FindFirst("email")?.Value ?? "'email' claim not found";
        var sub = user.FindFirst("sub")?.Value ?? "'sub' claim not found";
        return Ok(new
        {
            name,
            email, 
            sub,
        });
    }
}