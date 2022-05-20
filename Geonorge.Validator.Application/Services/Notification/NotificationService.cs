using Geonorge.Validator.Application.Hubs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using System.Linq;
using System.Threading.Tasks;

namespace Geonorge.Validator.Application.Services.Notification
{
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hub;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string HeaderName = "signalr-connectionid";

        public NotificationService(
            IHubContext<NotificationHub> hub,
            IHttpContextAccessor httpContextAccessor)
        {
            _hub = hub;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task SendAsync(string message)
        {
            var connectionId = GetConnectionId();

            if (connectionId != null)
                await _hub.Clients.Client(connectionId).SendAsync("ReceiveMessage", message);
        }

        private string GetConnectionId() => _httpContextAccessor.HttpContext.Request.Headers[HeaderName].FirstOrDefault();
    }
}
