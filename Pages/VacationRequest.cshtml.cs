using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Sitiowebb.Data;
using Sitiowebb.Data.Hubs;
using Sitiowebb.Models;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Security.Claims;

namespace Sitiowebb.Pages
{
    [Authorize]
    [ValidateAntiForgeryToken]
    public class VacationRequestModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly IHubContext<NotificationsHub> _hub;

        public VacationRequestModel(ApplicationDbContext db, IHubContext<NotificationsHub> hub)
        {
            _db  = db;
            _hub = hub;
        }

        [BindProperty, Required]
        public string From { get; set; } = "";

        [BindProperty, Required]
        public string To { get; set; } = "";

        public async Task<IActionResult> OnPostAsync()
        {
            // 1) Validar fechas
            var formats = new[] { "dd/MM/yyyy", "dd-MM-yyyy", "dd.MM.yyyy", "yyyy-MM-dd" };
            if (!DateTime.TryParseExact(From, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var fromDate))
            {
                ModelState.AddModelError(nameof(From), "Invalid date. Use dd/MM/yyyy.");
                return Page();
            }
            if (!DateTime.TryParseExact(To, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var toDate))
            {
                ModelState.AddModelError(nameof(To), "Invalid date. Use dd/MM/yyyy.");
                return Page();
            }
            if (fromDate > toDate)
            {
                ModelState.AddModelError(string.Empty, "The start date must be before the end date.");
                return Page();
            }

            // 2) Usuario actual
            var userEmail = User.FindFirstValue(ClaimTypes.Email) ?? User.Identity?.Name ?? "";

            // 3) Crear solicitud
            var req = new VacationRequest
            {
                From       = fromDate,
                To         = toDate,
                CreatedUtc = DateTime.UtcNow,
                Status     = RequestStatus.Pending,
                UserEmail  = userEmail
            };

            _db.VacationRequests.Add(req);
            await _db.SaveChangesAsync();

            // 4) Notificaciones a MANAGERS
        // tras SaveChangesAsync():
            var pending = await _db.VacationRequests.CountAsync(v => v.Status == RequestStatus.Pending);

            // badge managers
            await _hub.Clients.Group("managers").SendAsync("pendingCountUpdated", new { count = pending });

            // tarjetita managers
            await _hub.Clients.Group("managers").SendAsync("vacationRequestCreated", new {
                id   = req.Id,
                user = (req.UserEmail ?? string.Empty).ToLowerInvariant(),
                from = req.From,
                to   = req.To
            });


            // 5) Mensaje UI y redirect
            TempData["SuccessMessage"] = "Request sent.";
            return RedirectToPage("/UsuarioHome");
        }
    }
}
