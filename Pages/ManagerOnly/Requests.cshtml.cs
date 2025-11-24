using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Sitiowebb.Data;
using Sitiowebb.Models;

namespace Sitiowebb.Pages.ManagerOnly
{
    [Authorize(Roles = "Manager")]
    public class RequestsModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        public List<VacationRequest> Items { get; private set; } = new();

        public RequestsModel(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task OnGetAsync()
        {
    
            Items = await _db.VacationRequests
                .Where(v => v.Status == RequestStatus.Pending)
                .OrderByDescending(v => v.CreatedUtc)
                .ToListAsync();
        }
    }
}
