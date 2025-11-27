using System;

namespace Sitiowebb.Services
{
    public class EmailSettings
    {
        // Para SMTP (MailerSend)
        public string Host { get; set; } = "";
        public int Port { get; set; } = 587;
        public bool EnableSsl { get; set; } = true;

        public string UserName { get; set; } = "";   // SMTP username
        public string Password { get; set; } = "";   // SMTP password

        public string From { get; set; } = "";       // Dirección FROM
        public string FromName { get; set; } = "Arkose Labs Notifications";

        // Para compatibilidad con MailerEmailSender (aunque no lo usemos)
        public string ApiKey { get; set; } = "";
    }
}