using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Sitiowebb.Data;
using Sitiowebb.Data.Hubs;
using Sitiowebb.Models;
using Sitiowebb.Services;
using Microsoft.AspNetCore.Identity;

namespace Sitiowebb.Pages.ManagerOnly
{
    [Authorize(Roles = "Manager")]
    public class RequestModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly IHubContext<NotificationsHub> _hub;
        private readonly IAppEmailSender _email;

        public RequestModel(
            ApplicationDbContext db,
            IHubContext<NotificationsHub> hub,
            IAppEmailSender email)
        {
            _db = db;
            _hub = hub;
            _email = email;
        }

        public VacationRequest Item { get; private set; } = null!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var req = await _db.VacationRequests.FirstOrDefaultAsync(v => v.Id == id);
            if (req == null)
                return NotFound();

            Item = req;
            return Page();
        }

        // --------- APPROVE ---------
        public async Task<IActionResult> OnPostApproveAsync(int id, string? comment)
        {
            var req = await _db.VacationRequests
                            .FirstOrDefaultAsync(v => v.Id == id);

            if (req is null)
                return NotFound();

            // 1) Actualizar la solicitud
            req.Status         = RequestStatus.Approved;
            req.ManagerComment = comment ?? string.Empty;
            req.DecidedUtc     = DateTime.UtcNow;

            // 2) Crear Unavailability con el tipo REAL
            if (!string.IsNullOrWhiteSpace(req.UserEmail))
            {
                var k = (req.Kind ?? "vacation").Trim().ToLowerInvariant();
                var kind = k switch
                {
                    "sick" or "ill"                     => "sick",
                    "halfday" or "half-day" or "half day" => "halfday",
                    "holiday"                           => "holiday",
                    _                                   => "vacation"
                };

                var unav = new Unavailability
                {
                    UserEmail     = req.UserEmail,
                    Kind          = kind,
                    StartDate     = req.From,
                    EndDate       = req.To,
                    IsHalfDay     = false,          // si luego quieres medias jornadas, aquí se ajusta
                    HalfSegment   = null,
                    Justification = string.IsNullOrWhiteSpace(req.ManagerComment)
                    ? (req.UserComment ?? string.Empty)
                    : req.ManagerComment
                };

                _db.Unavailabilities.Add(unav);
            }

            await _db.SaveChangesAsync();

            // 3) Notificación en tiempo real al usuario
            if (!string.IsNullOrWhiteSpace(req.UserEmail))
            {
                var target = req.UserEmail.Trim().ToLowerInvariant();

                await _hub.Clients.User(target).SendAsync(
                    "requestDecision",
                    new
                    {
                        id      = req.Id,
                        status  = "Approved",
                        comment = req.ManagerComment
                    });
            }

            // 4) Actualizar contador de pendientes
            var pending = await _db.VacationRequests
                                .CountAsync(v => v.Status == RequestStatus.Pending);

            await _hub.Clients.Group("managers")
                .SendAsync("pendingCountUpdated", new { count = pending });

            TempData["SuccessMessage"] = "Request approved.";
            // 📧 Email al usuario
            if (!string.IsNullOrWhiteSpace(req.UserEmail))
            {
                var subject = "[Arkose] Your request was approved";
                var body = $@"
                    <p>Hello,</p>
                    <p>Your <strong>{req.Kind}</strong> request from {req.From:d} to {req.To:d} 
                    has been <strong>approved</strong>.</p>
                    <p><strong>Manager comment:</strong> {req.ManagerComment}</p>";

                await _email.SendAsync(req.UserEmail, subject, body);
            }
            return RedirectToPage("./Requests");
        }

        // ---------------- DENY ----------------
        public async Task<IActionResult> OnPostDenyAsync(int id, string? comment)
        {
            var req = await _db.VacationRequests.FirstOrDefaultAsync(v => v.Id == id);
            if (req == null)
                return NotFound();

            req.Status = RequestStatus.Denied;
            req.ManagerComment = comment ?? "";
            req.DecidedUtc = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            // Notify user
            await _hub.Clients.User(req.UserEmail.ToLower()).SendAsync("requestDecision", new
            {
                id = req.Id,
                status = "denied",
                comment = req.ManagerComment
            });

            // Update pending count
            var pending = await _db.VacationRequests.CountAsync(v => v.Status == RequestStatus.Pending);
            await _hub.Clients.Group("managers").SendAsync("pendingCountUpdated", new { count = pending });

            TempData["SuccessMessage"] = "Request denied.";
            // 📧 Email al usuario
            if (!string.IsNullOrWhiteSpace(req.UserEmail))
            {
                var subject = "[Arkose] Your request was denied";
                var body = $@"
                    <p>Hello,</p>
                    <p>Your <strong>{req.Kind}</strong> request from {req.From:d} to {req.To:d} 
                    has been <strong>denied</strong>.</p>
                    <p><strong>Manager comment:</strong> {req.ManagerComment}</p>";

                await _email.SendAsync(req.UserEmail, subject, body);
            }
            return RedirectToPage("./Requests");
        }
    }
}