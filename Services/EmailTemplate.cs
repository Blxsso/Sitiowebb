using System.Text;

namespace Sitiowebb.Services
{
    public static class EmailTemplate
    {
        /// <summary>
        /// Crea un HTML de correo bonito con los colores de Arkose.
        /// </summary>
        public static string Build(
            string title,          // Ej: "New unavailability request"
            string introText,      // Ej: "Hi Sara, you have a new request:"
            string mainText,       // Ej: "User prueba4 requested vacation from 21/11/2025 to 25/11/2025."
            string? buttonText = null,
            string? buttonUrl = null,
            string? footerText = null
        )
        {
            footerText ??= "This is an automatic notification from Arkose Labs availability system.";

            var sb = new StringBuilder();

            sb.Append($@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
    <title>{title}</title>
</head>
<body style=""margin:0; padding:0; background-color:#f5f5f5; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif;"">

    <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""background-color:#f5f5f5; padding:24px 0;"">
        <tr>
            <td align=""center"">
                <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""max-width:600px; background-color:#ffffff; border-radius:12px; overflow:hidden; box-shadow:0 4px 16px rgba(0,0,0,0.06);"">
                    <!-- Header -->
                    <tr>
                        <td style=""background:#1b5e20; padding:18px 24px; text-align:left;"">
                            <span style=""color:#ffffff; font-size:20px; font-weight:600;"">Arkose Labs</span>
                        </td>
                    </tr>

                    <!-- Title -->
                    <tr>
                        <td style=""padding:24px 24px 8px 24px; text-align:left;"">
                            <h1 style=""margin:0; font-size:22px; font-weight:600; color:#111111;"">
                                {title}
                            </h1>
                        </td>
                    </tr>

                    <!-- Intro -->
                    <tr>
                        <td style=""padding:4px 24px 8px 24px; text-align:left;"">
                            <p style=""margin:0; font-size:15px; line-height:1.5; color:#444444;"">
                                {introText}
                            </p>
                        </td>
                    </tr>

                    <!-- Main text -->
                    <tr>
                        <td style=""padding:8px 24px 16px 24px; text-align:left;"">
                            <p style=""margin:0; font-size:15px; line-height:1.6; color:#222222;"">
                                {mainText}
                            </p>
                        </td>
                    </tr>");

            // Botón opcional
            if (!string.IsNullOrWhiteSpace(buttonText) && !string.IsNullOrWhiteSpace(buttonUrl))
            {
                sb.Append($@"
                    <!-- Button -->
                    <tr>
                        <td style=""padding:8px 24px 24px 24px; text-align:left;"">
                            <a href=""{buttonUrl}"" 
                               style=""display:inline-block; padding:12px 24px; background:#1b5e20; border-radius:999px; 
                                      color:#ffffff; text-decoration:none; font-size:15px; font-weight:600;"">
                                {buttonText}
                            </a>
                        </td>
                    </tr>");
            }

            sb.Append($@"
                    <!-- Divider -->
                    <tr>
                        <td style=""padding:0 24px;"">
                            <hr style=""border:none; border-top:1px solid #eeeeee; margin:0;"" />
                        </td>
                    </tr>

                    <!-- Footer -->
                    <tr>
                        <td style=""padding:16px 24px 20px 24px; text-align:left;"">
                            <p style=""margin:0; font-size:12px; line-height:1.4; color:#777777;"">
                                {footerText}
                            </p>
                            <p style=""margin:8px 0 0 0; font-size:11px; color:#aaaaaa;"">
                                &copy; @(DateTime.UtcNow.Year) Arkose Labs. All rights reserved.
                            </p>
                        </td>
                    </tr>

                </table>
            </td>
        </tr>
    </table>

</body>
</html>");

            return sb.ToString();
        }
    }
}