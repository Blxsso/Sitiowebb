using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Sitiowebb.Data;
using Sitiowebb.Data.Hubs;
using Sitiowebb.Models;
using Sitiowebb.Services;
using Sitiowebb; 

namespace Sitiowebb.Pages.Unavailable
{
    [Authorize]
    public class NewModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly IHubContext<NotificationsHub> _hub;
        private readonly IAppEmailSender _email;
        private readonly UserManager<ApplicationUser> _userManager;

        public NewModel(
            ApplicationDbContext db,
            IHubContext<NotificationsHub> hub,
            IAppEmailSender email,
            UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _hub = hub;
            _email = email;
            _userManager = userManager;
        }
        // --------- ViewModel ---------
        [BindProperty]
        public InputData Input { get; set; } = new();

        public class InputData
        {
            public string? Kind { get; set; }           // vacation | sick | meeting | trip | halfday ...
            public DateTime StartDate { get; set; }
            public DateTime? EndDate { get; set; }      // si no llega, se usa StartDate
            public string? Comment { get; set; }        // justificación (obligatoria en sick)
        }

        public string KindTitle { get; private set; } = "Report unavailability";

        // --------- GET ---------
        public void OnGet(string? kind)
        {
            Input.Kind = (kind ?? "unavailability").Trim().ToLowerInvariant();

            KindTitle = Input.Kind switch
            {
                "vacation" => "Report vacation",
                "holiday"  => "Report holiday",
                "sick"     => "Report sickness",
                "meeting"  => "Report meeting",
                "trip"     => "Report job trip",
                "halfday"  => "Report half day off",
                _          => "Report unavailability"
            };

            Input.StartDate = DateTime.Today;
        }

        // --------- POST ---------
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var user = await _userManager.GetUserAsync(User);
            var userEmail = user?.Email ?? "unknown@domain.com";
            var kind = (Input.Kind ?? "unavailability").Trim().ToLowerInvariant();
            var endDate = Input.EndDate ?? Input.StartDate;

            // Regla: sickness requiere comentario
            if (kind == "sick" && string.IsNullOrWhiteSpace(Input.Comment))
            {
                ModelState.AddModelError(string.Empty, "Justification is required for sickness.");
                return Page();
            }

            // 🔸 1) Tipos que VAN a PENDING REQUESTS: vacation, sick, halfday
            if (kind == "vacation" || kind == "holiday" ||
                kind == "sick" || kind == "ill" ||
                kind == "halfday" || kind == "half-day" || kind == "half day")
            {
                var req = new VacationRequest
                {
                    UserEmail   = userEmail,
                    Kind        = kind,               // 👈 IMPORTANTE: guardar el tipo real
                    From        = Input.StartDate,
                    To          = endDate,
                    Status      = RequestStatus.Pending,
                    CreatedUtc  = DateTime.UtcNow,
                    UserComment=Input.Comment
                };

                _db.VacationRequests.Add(req);
                await _db.SaveChangesAsync();

                // 🔔 Notificación a managers
                await _hub.Clients.Group("managers").SendAsync("vacationRequestCreated", new
                {
                    id   = req.Id,
                    user = userEmail
                });

                // 📧 Email a managers
                var managers = await _userManager.GetUsersInRoleAsync("Manager");
                var subject = $"[Arkose] New {req.Kind} request from {userEmail}";
                var body = $@"
                    <p>A new <strong>{req.Kind}</strong> request has been created.</p>
                    <p><strong>User:</strong> {userEmail}</p>
                    <p><strong>From:</strong> {req.From:d}<br/>
                    <strong>To:</strong> {req.To:d}</p>
                    <p>You can review it in the manager panel.</p>";

                foreach (var m in managers)
                {
                    if (!string.IsNullOrWhiteSpace(m.Email))
                        await _email.SendAsync(m.Email, subject, body);
                }

                TempData["flash.success"] = "Request sent successfully!";
                return RedirectToPage("/UnavailableOptions");
            }

            // 🔸 2) Otros tipos → unavailability directa (meeting, trip, etc.)
            var entity = new Unavailability
            {
                UserEmail     = userEmail,
                Kind          = kind,
                StartDate     = Input.StartDate,
                EndDate       = endDate,
                IsHalfDay     = false,
                HalfSegment   = null,
                Justification = Input.Comment
            };

            try
            {
                _db.Unavailabilities.Add(entity);
                await _db.SaveChangesAsync();

                static string PrettyKind(string k)
                {
                    k = (k ?? "").Trim().ToLowerInvariant();
                    return k switch
                    {
                        "sick" or "ill"                     => "sickness",
                        "meeting"                           => "meeting",
                        "trip" or "jobtrip" or "job trip"  => "job trip",
                        "halfday"                           => "half day off",
                        "vacation" or "holiday"             => "vacation",
                        _                                   => "unavailability"
                    };
                }

                var pretty      = PrettyKind(entity.Kind);
                var displayUser = userEmail;

                await _hub.Clients.Group("managers").SendAsync("unavailabilityCreated", new
                {
                    id            = entity.Id,
                    user          = displayUser,
                    kind          = entity.Kind,
                    prettyKind    = pretty,
                    start         = entity.StartDate,
                    end           = entity.EndDate,
                    half          = entity.IsHalfDay ? (entity.HalfSegment ?? "") : null,
                    justification = entity.Justification
                });

                TempData["flash.success"] = "Unavailability reported successfully!";
                // 📧 Email a managers
                var managers2 = await _userManager.GetUsersInRoleAsync("Manager");
                var subject2 = $"[Arkose] New {pretty} from {displayUser}";
                var body2 = $@"
                    <p>User <strong>{displayUser}</strong> reported a new <strong>{pretty}</strong>.</p>
                    <p><strong>From:</strong> {entity.StartDate:d}<br/>
                    <strong>To:</strong> {entity.EndDate:d}</p>
                    <p><strong>Comment:</strong> {entity.Justification}</p>";

                foreach (var m in managers2)
                {
                    if (!string.IsNullOrWhiteSpace(m.Email))
                        await _email.SendAsync(m.Email, subject2, body2);
                }
            }
            catch (Exception ex)
            {
                TempData["flash.error"] = "Something went wrong. Please try again.";
                Console.WriteLine($"[Unavailable/New] Error: {ex}");
            }

            return RedirectToPage("/UnavailableOptions");
        }
    }
}