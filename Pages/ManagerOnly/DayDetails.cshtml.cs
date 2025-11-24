using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Sitiowebb.Data;
using Sitiowebb.Models;

namespace Sitiowebb.Pages.ManagerOnly
{
    [Authorize(Roles = "Manager")]
    public class DayDetailsModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public DayDetailsModel(ApplicationDbContext db) => _db = db;

        // ?date=2025-12-24
        [BindProperty(SupportsGet = true)]
        public DateTime? Date { get; set; }

        public DateTime TargetDate { get; private set; }

        public record EmpRow(string Email, string Name, string Status,
                             string? Half, DateTime? From, DateTime? To);

        public List<EmpRow> Employes { get; private set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            TargetDate = (Date ?? DateTime.Today).Date;

            // Lista de usuarios (con email) ordenados por nombre
            var users = await _db.Users.AsNoTracking()
                .Where(u => u.Email != null)
                .Select(u => new { u.Email, u.UserName })
                .OrderBy(u => u.UserName)
                .ToListAsync();

            // Vacaciones ese día
            var vacToday = await _db.VacationRequests.AsNoTracking()
                .Where(v => v.Status == RequestStatus.Approved &&
                            v.From.Date <= TargetDate &&
                            v.To.Date >= TargetDate)
                .Select(v => new { v.UserEmail, v.From, v.To })
                .ToListAsync();

            var vacMap = vacToday
                .GroupBy(v => v.UserEmail ?? "")
                .ToDictionary(g => g.Key, g => g.First());

            // Otras indisponibilidades ese día
            var unavToday = await _db.Unavailabilities.AsNoTracking()
                .Where(u => u.StartDate.Date <= TargetDate &&
                            u.EndDate.Date >= TargetDate)
                .Select(u => new
                {
                    u.UserEmail,
                    u.Kind,
                    u.IsHalfDay,
                    u.HalfSegment,
                    u.StartDate,
                    u.EndDate
                })
                .ToListAsync();

            var unavMap = unavToday
                .GroupBy(u => u.UserEmail ?? "")
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var u in users)
            {
                var email = u.Email ?? "";
                var name = string.IsNullOrWhiteSpace(u.UserName)
                    ? PrettyFromEmail(email)
                    : u.UserName!;

                var status = "available";
                string? half = null;
                DateTime? from = null, to = null;

                // Vacaciones ganan por defecto
                if (vacMap.TryGetValue(email, out var vrec))
                {
                    status = "vacation";
                    from = vrec.From.Date;
                    to = vrec.To.Date;
                }

                // Otras indisponibilidades pueden sobrescribir
                if (unavMap.TryGetValue(email, out var list))
                {
                    string winner = status;
                    foreach (var r in list)
                    {
                        var s = StatusFromUnav(r.Kind, r.IsHalfDay);
                        bool beats =
                            (s == "sick") ||
                            (s == "meeting" && winner != "sick") ||
                            (s == "trip" && winner is not ("sick" or "meeting")) ||
                            (s == "halfday" && winner is "available" or "unavailability") ||
                            ((s is "training" or "overtime" or "personal") && winner == "available");

                        if (beats)
                        {
                            winner = s;
                            from = r.StartDate.Date;
                            to = r.EndDate.Date;
                            half = r.IsHalfDay ? (r.HalfSegment ?? "") : null;
                        }
                    }

                    status = winner;
                }

                Employes.Add(new EmpRow(email, name, status, half, from, to));
            }

            Employes = Employes
                .OrderBy(r => r.Status == "available" ? 1 : 0)
                .ThenBy(r => r.Name)
                .ToList();

            return Page();
        }

        // ===== helpers iguales a CalendarModel =====
        private static string Normalize(string? k) => (k ?? "").Trim().ToLowerInvariant();

        private static string StatusFromUnav(string? kind, bool isHalf)
        {
            var k = Normalize(kind);
            if (k is "sick" or "ill" or "sickness") return "sick";
            if (k is "meeting" or "meet") return "meeting";
            if (k is "trip" or "jobtrip" or "job trip") return "trip";
            if (k == "training") return "training";
            if (k == "overtime") return "overtime";
            if (k == "personal") return "personal";
            if (isHalf) return "halfday";
            return "unavailability";
        }

        private static string PrettyFromEmail(string email)
        {
            try
            {
                var local = (email ?? "").Split('@')[0]
                    .Replace('.', ' ')
                    .Replace('_', ' ')
                    .Replace('-', ' ')
                    .Trim();

                return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(local);
            }
            catch
            {
                return string.IsNullOrWhiteSpace(email) ? "(Unknown)" : email;
            }
        }
    }
}
