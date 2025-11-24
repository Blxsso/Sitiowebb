using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Sitiowebb.Data.Hubs
{
    [Authorize]
    public class NotificationsHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var user = Context.User;
            if (user == null)
            {
                await base.OnConnectedAsync();
                return;
            }

            // Grupo "managers"
            if (user.IsInRole("Manager"))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "managers");
            }

            // Grupo por usuario (para toasts al dueño de la request)
            var email = user.FindFirstValue(ClaimTypes.Email) ??
                        user.Identity?.Name;
            if (!string.IsNullOrEmpty(email))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user:{email}");
            }

            await base.OnConnectedAsync();
        }
    }
}
