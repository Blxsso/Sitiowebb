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
            // 1) Si hay ApiKey => usar SendGrid
            if (!string.IsNullOrWhiteSpace(_settings.ApiKey))
            {
                var client = new SendGridClient(_settings.ApiKey);

                var from = new EmailAddress(_settings.From, _settings.FromName);
                var to = new EmailAddress(toEmail);

                // construimos el HTML bonito con tu template
                var niceHtml = EmailTemplate.Build(
                    title: subject,
                    introText: "Hello,",
                    mainText: htmlMessage,
                    buttonText: "Open Arkose dashboard",
                    buttonUrl: "https://sitiowebb-production.up.railway.app/ManagerOnly/Requests",
                    footerText: "You received this email because your user is registered in the Arkose Labs availability tool."
                );

                var msg = MailHelper.CreateSingleEmail(
                    from,
                    to,
                    subject,
                    plainTextContent: null,
                    htmlContent: niceHtml
                );

                var response = await client.SendEmailAsync(msg);

                // Si SendGrid devuelve error, lanza excepción para que lo veamos en logs
                if ((int)response.StatusCode >= 400)
                {
                    var body = await response.Body.ReadAsStringAsync();
                    throw new Exception($"SendGrid error {(int)response.StatusCode}: {body}");
                }

                return;
            }

            // 2) Si NO hay ApiKey => usar SMTP (modo desarrollo local)
            var fromAddr = new MailAddress(_settings.From, _settings.FromName);
            var toAddr   = new MailAddress(toEmail);

            var niceHtmlSmtp = EmailTemplate.Build(
                title: subject,
                introText: "Hello,",
                mainText: htmlMessage,
                buttonText: "Open Arkose dashboard",
                buttonUrl: "https://localhost:5232/ManagerOnly/Requests",
                footerText: "You received this email because your user is registered in the Arkose Labs availability tool."
            );

            using var msg2 = new MailMessage(fromAddr, toAddr)
            {
                Subject = subject,
                Body = niceHtmlSmtp,
                IsBodyHtml = true
            };

            using var clientSmtp = new SmtpClient(_settings.Host, _settings.Port)
            {
                EnableSsl = _settings.EnableSsl,
                Credentials = new System.Net.NetworkCredential(_settings.UserName, _settings.Password)
            };

            await clientSmtp.SendMailAsync(msg2);
        }
    }
}