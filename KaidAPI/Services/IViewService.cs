using Microsoft.AspNetCore.Mvc;

namespace KaidAPI.Services;

public interface IViewService
{
    Task<IActionResult> GetOverview();
    Task<IActionResult> GetTeams();
    Task<IActionResult> GetUsers();
}
