using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using Sitiowebb.Models;

namespace Sitiowebb.Security
{
    public class CommonPasswordValidator : IPasswordValidator<ApplicationUser>
    {
        private static readonly HashSet<string> Common = new(StringComparer.OrdinalIgnoreCase)
        {
            "password","123456","123456789","qwerty","abc123","111111","123123",
            "admin","letmein","passw0rd","contraseña","clave123","000000"
        };

        public Task<IdentityResult> ValidateAsync(
            UserManager<ApplicationUser> manager,
            ApplicationUser user,
            string? password)
        {
            if (string.IsNullOrEmpty(password))
            {
                return Task.FromResult(IdentityResult.Failed(new IdentityError
                {
                    Code = "NullPassword",
                    Description = "La contraseña no puede ser nula o vacía."
                }));
            }

            if (Common.Contains(password))
            {
                return Task.FromResult(IdentityResult.Failed(new IdentityError
                {
                    Code = "CommonPassword",
                    Description = "La contraseña es demasiado común. Elige una más fuerte."
                }));
            }

            if (!string.IsNullOrWhiteSpace(user.Email) &&
                password.Contains(user.Email.Split('@')[0], StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(IdentityResult.Failed(new IdentityError
                {
                    Code = "ContainsUserInfo",
                    Description = "La contraseña no debe contener tu usuario o email."
                }));
            }

            return Task.FromResult(IdentityResult.Success);
        }
    }
}
