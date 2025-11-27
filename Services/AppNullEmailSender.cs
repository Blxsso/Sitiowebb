using System.Threading.Tasks;

namespace Sitiowebb.Services
{
    // Este es SOLO para las notificaciones de la app (IAppEmailSender),
    // no toca el login ni el reset de contraseña.
    public class AppNullEmailSender : IAppEmailSender
    {
        public Task SendAsync(string toEmail, string subject, string htmlMessage)
        {
            // No hace nada. Así, aunque falle el proveedor real,
            // la web nunca explota.
            return Task.CompletedTask;
        }
    }
}