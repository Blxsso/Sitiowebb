using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Sitiowebb.Pages
{
    [Authorize]
    public class AvailableModel : PageModel
    {
        public void OnGet() { }
    }
}
