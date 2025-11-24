// Controllers/NotificationsApiController.cs
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sitiowebb.Data;
using Sitiowebb.Models;

namespace Sitiowebb.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    public class NotificationsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public NotificationsApiController(ApplicationDbContext db) => _db = db;

        /// <summary>
        /// GET /api/notifications/user-last-decision
        /// Devuelve la última solicitud del usuario con estado distinto a Pending (Approved/Denied).
        /// Usado por notifications.js al iniciar para mostrar:
        /// "Your request is accepted / isn't accepted".
        /// </summary>
        [HttpGet("user-last-decision")]
        [Authorize]
        public async Task<IActionResult> GetUserLastDecision()
        {
            var email = User.FindFirstValue(ClaimTypes.Email) ?? User.Identity?.Name ?? "";
            if (string.IsNullOrWhiteSpace(email))
                return Ok(null); // sin usuario autenticado

            var item = await _db.VacationRequests
                .Where(v => v.UserEmail == email
                            && v.Status != RequestStatus.Pending
                            && v.DecidedUtc != null)
                .OrderByDescending(v => v.DecidedUtc)
                .Select(v => new
                {
                    id = v.Id,
                    status = v.Status.ToString(),              // "Approved" | "Denied"
                    comment = v.ManagerComment ?? string.Empty,
                    decidedUtc = v.DecidedUtc
                })
                .FirstOrDefaultAsync();

            return Ok(item); // null si no hay
        }

        /// <summary>
        /// GET /api/notifications/manager-latest-pending
        /// Para managers: última solicitud pendiente (para la tarjetita "Pending request of ...").
        /// </summary>
        [HttpGet("manager-latest-pending")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> GetManagerLatestPending()
        {
            var item = await _db.VacationRequests
                .Where(v => v.Status == RequestStatus.Pending)
                .OrderByDescending(v => v.CreatedUtc)
                .Select(v => new
                {
                    id = v.Id,
                    user = v.UserEmail,
                    from = v.From.ToString("dd/MM/yyyy"),
                    to   = v.To.ToString("dd/MM/yyyy")
                })
                .FirstOrDefaultAsync();

            return Ok(item); // null si no hay
        }
    }
}
