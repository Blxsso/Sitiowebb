using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace Sitiowebb.Data.Hubs
{
    // Este provider le dice a SignalR qué usar como "UserId"
    // para Clients.User(...)
    public class EmailUserIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            var user = connection.User;
            if (user?.Identity?.IsAuthenticated != true)
                return null;

            // Usamos el mismo valor que tú usas en New.cshtml.cs:
            // User.Identity.Name  (que en tu app es el email)
            var name = user.Identity?.Name;

            if (string.IsNullOrWhiteSpace(name))
            {
                // Plan B: intentar claim de email
                name = user.FindFirst(ClaimTypes.Email)?.Value;
            }

            return name?.Trim().ToLowerInvariant();
        }
    }
}