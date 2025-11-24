// Ruta: Services/IAppEmailSender.cs
using System.Threading.Tasks;

namespace Sitiowebb.Services
{
    public interface IAppEmailSender
    {
        Task SendAsync(string toEmail, string subject, string htmlMessage);
    }
}