using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Sitiowebb.Pages
{
    [Authorize(Roles = "Manager")]
    public class ManagerOnlyModel : PageModel
    {
        public void OnGet() { }
    }
}
