using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Sitiowebb.Pages
{
    [AllowAnonymous]   
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            // No necesitas redirección manual aquí.
            // Si el usuario no está autenticado, ASP.NET Identity lo mandará al login automáticamente.
        }
    }
}
