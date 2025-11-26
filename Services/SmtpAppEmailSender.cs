using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

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
            // Construimos el HTML bonito
            var niceHtml = EmailTemplate.Build(
                title: subject,
                introText: "Hello,",
                mainText: htmlMessage,
                buttonText: "Open Arkose dashboard",
                // 🔹 Usa la URL en producción, no localhost
                buttonUrl: "https://sitiowebb-production.up.railway.app/ManagerOnly/Requests",
                footerText: "You received this email because your user is registered in the Arkose Labs availability tool."
            );

            // 1) INTENTAR SENDGRID SI HAY API KEY
            if (!string.IsNullOrWhiteSpace(_settings.ApiKey))
            {
                var client = new SendGridClient(_settings.ApiKey);

                var fromAddress = new EmailAddress(_settings.From, _settings.FromName);
                var toAddress = new EmailAddress(toEmail);

                var msg = MailHelper.CreateSingleEmail(
                    from: fromAddress,
                    to: toAddress,
                    subject: subject,
                    plainTextContent: null,
                    htmlContent: niceHtml
                );

                try
                {
                    await client.SendEmailAsync(msg);
                }
                catch (Exception ex)
                {
                    // Si falla SendGrid, lo escribimos en logs y dejamos que la app siga
                    Console.WriteLine($"SendGrid error: {ex.Message}");
                }

                // 👈 IMPORTANTE: no sigas al SMTP si ya usaste SendGrid
                return;
            }

            // 2) SI NO HAY API KEY → SMTP (solo funcionará en local)
            var from = new MailAddress(_settings.From, _settings.FromName);
            var to = new MailAddress(toEmail);

            using var msgSmtp = new MailMessage(from, to)
            {
                Subject = subject,
                Body = niceHtml,
                IsBodyHtml = true
            };

            using var clientSmtp = new SmtpClient(_settings.Host, _settings.Port)
            {
                Credentials = new NetworkCredential(_settings.UserName, _settings.Password),
                EnableSsl = _settings.EnableSsl
            };

            try
            {
                await clientSmtp.SendMailAsync(msgSmtp);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SMTP error: {ex.Message}");
            }
        }
    }
}