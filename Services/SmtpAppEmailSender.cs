using System.Net;
using System.Net.Mail;
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
            // Si hay algo crítico que falta, no intentamos enviar
            if (string.IsNullOrWhiteSpace(_settings.Host) ||
                string.IsNullOrWhiteSpace(_settings.UserName) ||
                string.IsNullOrWhiteSpace(_settings.Password) ||
                string.IsNullOrWhiteSpace(_settings.From) ||
                string.IsNullOrWhiteSpace(toEmail))
            {
                return; // No rompemos la web
            }

            using var client = new SmtpClient(_settings.Host, _settings.Port)
            {
                EnableSsl = _settings.EnableSsl,
            };

            // Evitamos el lío del SecureString usando las propiedades
            var creds = new NetworkCredential();
            creds.UserName = _settings.UserName;
            creds.Password = _settings.Password;
            client.Credentials = creds;

            var msg = new MailMessage
            {
                From = new MailAddress(_settings.From, _settings.FromName),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };

            msg.To.Add(toEmail);

            try
            {
                await client.SendMailAsync(msg);
            }
            catch
            {
                // Aquí podrías hacer logging, pero NO relanzamos la excepción.
                // Así, si falla el correo, la web sigue funcionando.
            }
        }
    }
}