using System.Threading.Tasks;

namespace Geonorge.Validator.Application.Services.Notification
{
    public interface INotificationService
    {
        Task SendAsync(object message);
    }
}
