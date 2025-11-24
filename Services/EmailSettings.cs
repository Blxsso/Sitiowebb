// Ruta: Services/EmailSettings.cs
using System;

namespace Sitiowebb.Services
{
    public class EmailSettings
    {
        public string Host { get; set; } = "";
        public int Port { get; set; } = 587;
        public bool EnableSsl { get; set; } = true;

        public string UserName { get; set; } = "";
        public string Password { get; set; } = "";

        public string From { get; set; } = "";
        public string FromName { get; set; } = "Arkose Labs Notifications";
    }
}