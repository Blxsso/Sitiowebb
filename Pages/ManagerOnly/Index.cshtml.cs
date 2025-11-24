using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sitiowebb.Data;
using Sitiowebb.Models;

namespace Sitiowebb.Pages.ManagerOnly
{
    [Authorize(Roles = "Manager")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // === Propiedades que usa tu Index.cshtml ===
        public int PendingCount { get; private set; }
        public int ActiveEmployees { get; private set; }
        public int AbsencesThisMonth { get; private set; }

        public void OnGet()
        {
            // Pendientes de decisión
            PendingCount = _db.VacationRequests.Count(v => v.Status == RequestStatus.Pending);

            // Empleados activos (aprox; si tienes otra lógica, mantenla)
            ActiveEmployees = _userManager.Users.Count();

            // Ausencias del mes actual (solicitudes aprobadas con rango en el mes)
            var now = System.DateTime.UtcNow;
            var first = new System.DateTime(now.Year, now.Month, 1);
            var nextMonth = first.AddMonths(1);

            AbsencesThisMonth = _db.VacationRequests
                .Where(v => v.Status == RequestStatus.Approved &&
                            v.To >= first && v.From < nextMonth)
                .Count();
        }
    }
}
