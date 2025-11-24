using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sitiowebb.Services;

namespace Sitiowebb.Pages
{
    // Solo un usuario logueado puede entrar (para no dejar esto público)
    [Authorize]
    public class TestEmailModel : PageModel
    {
        private readonly IAppEmailSender _email;

        public TestEmailModel(IAppEmailSender email)
        {
            _email = email;
        }

        public string? Result { get; private set; }

        public void OnGet()
        {
            // solo mostrar la página
        }

        public async Task OnPostAsync()
        {
            var to = User?.Identity?.Name ?? "destino@ejemplo.com";

            await _email.SendAsync(
                to,
                "Prueba de correo desde Arkose",
                "<p>Si ves este mensaje, el SMTP está funcionando ✅</p>"
            );

            Result = $"Correo de prueba enviado a: {to}";
        }
    }
}