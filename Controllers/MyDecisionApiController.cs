using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;      // <- necesario para [Authorize]
using Microsoft.AspNetCore.Mvc;               // <- necesario para ControllerBase
using Microsoft.EntityFrameworkCore;
using Sitiowebb.Data;                         // ApplicationDbContext
using Sitiowebb.Models;                       // RequestStatus

namespace Sitiowebb.Controllers
{
    // Todas las rutas de este controlador empiezan por /api/manager
    [ApiController]
    [Route("api/manager")]
    [Authorize(Roles = "Manager")]
    public class MyDecisionApiController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public MyDecisionApiController(ApplicationDbContext db) => _db = db;

        // GET /api/manager/pending-count
        [HttpGet("pending-count")]
        public async Task<ActionResult<int>> GetPendingCount()
        {
            var n = await _db.VacationRequests
                .CountAsync(v => v.Status == RequestStatus.Pending);
            return Ok(n);
        }

        // GET /api/manager/resolve-user?q=texto
        // Devuelve { email: "..."} del usuario más probable según email o nombre
        [HttpGet("resolve-user")]
        public async Task<IActionResult> ResolveUser([FromQuery] string q)
        {
            if (string.IsNullOrWhiteSpace(q)) return NotFound();
            var qq = q.Trim().ToLower();

            var u = await _db.Users
                .Select(u => new { u.Email, u.UserName })
                .FirstOrDefaultAsync(u =>
                    ((u.Email ?? "").ToLower().Contains(qq)) ||
                    ((u.UserName ?? "").ToLower().Contains(qq)));

            if (u == null || string.IsNullOrWhiteSpace(u.Email))
                return NotFound();

            return Ok(new { email = u.Email });
        }
    }
}
