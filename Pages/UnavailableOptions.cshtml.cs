using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Sitiowebb.Pages
{
    [Authorize]
    public class UnavailableOptionsModel : PageModel
    {
        public void OnGet() { }
    }
}
