using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Sitiowebb.Data;
using Sitiowebb.Data.Hubs;
using Sitiowebb.Models;

namespace Sitiowebb.Pages.ManagerOnly.Requests
{
    [Authorize(Roles = "Manager")]
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly IHubContext<NotificationsHub> _hub;

        public DetailsModel(ApplicationDbContext db, IHubContext<NotificationsHub> hub)
        {
            _db  = db;
            _hub = hub;
        }

        // La vista usa esta propiedad
        public VacationRequest Item { get; private set; } = default!;

        // GET /ManagerOnly/Requests/Details?id=123
        public async Task<IActionResult> OnGetAsync(int id)
        {
            var req = await _db.VacationRequests.FirstOrDefaultAsync(v => v.Id == id);
            if (req is null) return NotFound();

            Item = req;
            return Page();
        }

        // POST: Approve
        public async Task<IActionResult> OnPostApproveAsync(int id, string? comment)
        {
            var req = await _db.VacationRequests.FirstOrDefaultAsync(v => v.Id == id);
            if (req is null) return NotFound();

            req.Status         = RequestStatus.Approved;
            req.ManagerComment = comment ?? string.Empty;
            req.DecidedUtc     = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            // 🔔 Aviso en tiempo real al dueño de la solicitud
            if (!string.IsNullOrWhiteSpace(req.UserEmail))
            {
                await _hub.Clients.User(req.UserEmail).SendAsync("requestDecision", new
                {
                    id      = req.Id,
                    status  = "Approved",              // o req.Status.ToString()
                    comment = req.ManagerComment
                });
            }

            // 🔴 Recalcular y publicar el conteo para el badge de managers
            var pending = await _db.VacationRequests.CountAsync(v => v.Status == RequestStatus.Pending);
            await _hub.Clients.Group("managers").SendAsync("pendingCountUpdated", new { count = pending });

            TempData["SuccessMessage"] = "Request approved.";
            return RedirectToPage("./Requests");
        }

        // POST: Deny
        public async Task<IActionResult> OnPostDenyAsync(int id, string? comment)
        {
            var req = await _db.VacationRequests.FirstOrDefaultAsync(v => v.Id == id);
            if (req is null) return NotFound();

            req.Status         = RequestStatus.Denied;
            req.ManagerComment = comment ?? string.Empty;
            req.DecidedUtc     = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            // 🔔 Aviso en tiempo real al dueño de la solicitud
            if (!string.IsNullOrWhiteSpace(req.UserEmail))
            {
                await _hub.Clients.User(req.UserEmail).SendAsync("requestDecision", new
                {
                    id      = req.Id,
                    status  = "Denied",                // o req.Status.ToString()
                    comment = req.ManagerComment
                });
            }

            // 🔴 Recalcular y publicar el conteo para el badge de managers
            var pending = await _db.VacationRequests.CountAsync(v => v.Status == RequestStatus.Pending);
            await _hub.Clients.Group("managers").SendAsync("pendingCountUpdated", new { count = pending });

            TempData["SuccessMessage"] = "Request denied.";
            return RedirectToPage("./Requests");
        }
    }
}
