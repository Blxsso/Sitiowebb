// Controllers/DebugNotificationsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Sitiowebb.Data.Hubs;

namespace Sitiowebb.Controllers;

[ApiController]
[Route("api/debug")]
public class DebugNotificationsController : ControllerBase
{
    private readonly IHubContext<NotificationsHub> _hub;
    public DebugNotificationsController(IHubContext<NotificationsHub> hub) => _hub = hub;

    // 1) envía un toast al usuario actual
    [HttpPost("me")]
    [Authorize]
    public async Task<IActionResult> Me()
    {
        var uid = User?.Identity?.Name ?? "(no-name)";
        await _hub.Clients.User(uid).SendAsync("requestDecision", new {
            id = 999, status = "Approved", comment = "debug"
        });
        return Ok(new { sentTo = uid });
    }

    // 2) envía badge + tarjeta a los managers
    [HttpPost("managers")]
    [Authorize(Roles = "Manager")]
    public async Task<IActionResult> Managers()
    {
        await _hub.Clients.Group("managers").SendAsync("pendingCountUpdated", new { count = 7 });
        await _hub.Clients.Group("managers").SendAsync("vacationRequestCreated", new {
            id = 123, user = "debug@local", from = DateTime.UtcNow, to = DateTime.UtcNow
        });
        return Ok(new { sent = true });
    }
}
