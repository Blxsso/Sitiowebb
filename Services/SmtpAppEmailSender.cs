// Ruta: Services/SmtpAppEmailSender.cs
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Sitiowebb.Services
{
    public class SmtpAppEmailSender : IAppEmailSender
    {
        private readonly EmailSettings _settings;

        public SmtpAppEmailSender(IOptions<EmailSettings> options)
        {
            _settings = options.Value;
        }

        public async Task SendAsync(string toEmail, string subject, string htmlMessage)
        {
            var from = new MailAddress(_settings.From, _settings.FromName);
            var to   = new MailAddress(toEmail);

            // Aquí envolvemos el cuerpo en el template bonito
            var niceHtml = EmailTemplate.Build(
                title: subject,
                introText: "Hello,",
                mainText: htmlMessage,
                buttonText: "Open Arkose dashboard",
                buttonUrl: "https://localhost:5232/ManagerOnly/Requests", // o localhost mientras desarrollas
                footerText: "You received this email because your user is registered in the Arkose Labs availability tool."
            );

            using var msg = new MailMessage(from, to)
            {
                Subject = subject,
                Body = niceHtml,
                IsBodyHtml = true
            };

            using var client = new SmtpClient(_settings.Host, _settings.Port)
            {
                Credentials = new NetworkCredential(_settings.UserName, _settings.Password),
                EnableSsl = _settings.EnableSsl
            };

            await client.SendMailAsync(msg);
        }
    }
}