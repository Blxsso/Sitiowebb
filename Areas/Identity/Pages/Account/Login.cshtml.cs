using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;   // <-- necesario para [AllowAnonymous]
using Sitiowebb.Models;


namespace Sitiowebb.Areas.Identity.Pages.Account
{
    [AllowAnonymous] 
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;

        private readonly UserManager<ApplicationUser> _userManager;
        public LoginModel(SignInManager<ApplicationUser> signInManager,
                        UserManager<ApplicationUser> userManager,
                        ILogger<LoginModel> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? ReturnUrl { get; set; }

        public class InputModel
        {
            [Required, EmailAddress]
            public string Email { get; set; } = string.Empty;

            [Required, DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;

            [Display(Name = "Recordarme")]
            public bool RememberMe { get; set; }
        }


        public void OnGet(string? returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");
        }

                public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");

            if (!ModelState.IsValid)
                return Page();

            // 1) Buscar por email
            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user is null)
            {
                ModelState.AddModelError(string.Empty, "Intento de inicio de sesión no válido.");
                return Page();
            }

            // 2) Iniciar sesión usando el UserName (no el email)
            var result = await _signInManager.PasswordSignInAsync(
                user.UserName!, 
                Input.Password, 
                Input.RememberMe, 
                lockoutOnFailure: true);

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, result.IsLockedOut
                    ? "Cuenta bloqueada temporalmente."
                    : "Inicio de sesión no válido.");
                return Page();
            }

            _logger.LogInformation("Usuario inició sesión correctamente.");

            // 3) Redirigir SIEMPRE a tu home de usuario
            return LocalRedirect(Url.Content("~/UsuarioHome"));
            // Si prefieres respetar ReturnUrl cuando sea local:
            // return LocalRedirect(Url.IsLocalUrl(ReturnUrl) ? ReturnUrl : Url.Content("~/UsuarioHome"));
        }

    }
}
