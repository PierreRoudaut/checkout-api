
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Checkout.Api.Hubs
{
    public interface INotifyHub
    {
        Task SendMessage(string user, string message);
    }

    public class NotifyHub : Hub
    {
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}
