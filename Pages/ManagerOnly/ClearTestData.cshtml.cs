using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sitiowebb.Data;

namespace Sitiowebb.Pages.ManagerOnly
{
    [Authorize(Roles = "Manager")]
    public class ClearTestDataModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public ClearTestDataModel(ApplicationDbContext db) => _db = db;

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            // Borra TODO lo de estas tablas
            _db.Unavailabilities.RemoveRange(_db.Unavailabilities);
            _db.VacationRequests.RemoveRange(_db.VacationRequests);

            await _db.SaveChangesAsync();

            TempData["Status"] = "All vacation requests and unavailabilities were deleted.";
            return RedirectToPage("/ManagerOnly/Calendar");
        }
    }
}
