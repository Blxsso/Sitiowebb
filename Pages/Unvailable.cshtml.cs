using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Sitiowebb.Pages
{
    public class UnavailableModel : PageModel
    {
        public string? RequestType { get; private set; }

        public void OnGet(string? type)
        {
            RequestType = type; // "vacations", "holidays", "meeting", etc.
            // Aquí más adelante haremos la lógica específica por tipo.
        }
    }
}

