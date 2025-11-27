using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace Sitiowebb.Services
{
    // Implementa IAppEmailSender, pero usando la API HTTP de MailerSend
    public class SmtpAppEmailSender : IAppEmailSender
    {
        private readonly EmailSettings _settings;
        private readonly HttpClient _http;

        public SmtpAppEmailSender(IOptions<EmailSettings> options, HttpClient http)
        {
            _settings = options.Value;
            _http = http;
        }

        public async Task SendAsync(string toEmail, string subject, string htmlMessage)
        {
            // Si falta algo crítico, no intentamos enviar
            if (string.IsNullOrWhiteSpace(_settings.ApiKey) ||
                string.IsNullOrWhiteSpace(_settings.From) ||
                string.IsNullOrWhiteSpace(toEmail))
            {
                return;
            }

            // Si quieres seguir usando tu template bonito:
            var niceHtml = EmailTemplate.Build(
                title: subject,
                introText: "Hello,",
                mainText: htmlMessage,
                buttonText: "Open Arkose dashboard",
                buttonUrl: "https://sitiowebb-production.up.railway.app/ManagerOnly/Requests",
                footerText: "You received this email because your user is registered in the Arkose Labs availability tool."
            );

            var payload = new
            {
                from = new
                {
                    email = _settings.From,
                    name  = _settings.FromName
                },
                to = new[]
                {
                    new { email = toEmail }
                },
                subject = subject,
                html = niceHtml
            };

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                "https://api.mailersend.com/v1/email"
            );

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", _settings.ApiKey);

            var json = JsonSerializer.Serialize(payload);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                // Para que no se quede colgado eternamente si algo va mal
                _http.Timeout = TimeSpan.FromSeconds(10);

                var response = await _http.SendAsync(request);

                // Opcional: podrías loguear si response.IsSuccessStatusCode == false
                // pero no relances la excepción.
            }
            catch
            {
                // No tiramos la web si falla el correo
            }
        }
    }
}