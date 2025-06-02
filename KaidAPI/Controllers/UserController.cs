using KaidAPI.Models;
using KaidAPI.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace KaidAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    public UserController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] User user)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        user.UserId = Guid.NewGuid();
        user.CreatedAt = DateTime.UtcNow;

        var userId = await _userRepository.CreateUserAsync(user);
        return CreatedAtAction(nameof(GetUserById), new { userId = userId }, user);
    }

    [HttpGet("{userId:guid}")]
    public async Task<IActionResult> GetUserById(Guid userId)
    {
        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user == null)
            return NotFound();

        return Ok(user);
    }

    [HttpGet("email/{email}")]
    public async Task<IActionResult> GetUserByEmail(string email)
    {
        var user = await _userRepository.GetUserByEmailAsync(email);
        if (user == null)
            return NotFound();

        return Ok(user);
    }

    [HttpGet("oidc/{oidcSub}")]
    public async Task<IActionResult> GetUserByOidc(string oidcSub)
    {
        var user = await _userRepository.GetUserByOidcAsync(oidcSub);
        if (user == null)
            return NotFound();

        return Ok(user);
    }
}
