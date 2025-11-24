//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using Sitiowebb.Data;
//using Sitiowebb.Models;

//namespace Sitiowebb.Controllers
//{
    //[Route("api")]
   // [ApiController]
   // [Authorize(Roles = "Manager")]
    //public class PendingApiController : ControllerBase
    //{
      //  private readonly ApplicationDbContext _db;
        //public PendingApiController(ApplicationDbContext db) => _db = db;

      //  [HttpGet("pending-count")]
        //public async Task<int> GetCount() =>
          //  await _db.VacationRequests.CountAsync(v => v.Status == RequestStatus.Pending);

        //[HttpGet("pending-latest")]
        //public async Task<object?> GetLatest()
        //{
          //  var v = await _db.VacationRequests
           //     .Where(x => x.Status == RequestStatus.Pending)
             //   .OrderByDescending(x => x.CreatedUtc)
               // .FirstOrDefaultAsync();

//            return v == null ? null : new {
  //              id = v.Id,
    //            userEmail = v.UserEmail,
      //          from = v.From.ToString("dd/MM/yyyy"),
        //        to = v.To.ToString("dd/MM/yyyy")
          //  };
        //}
    //}
//}
