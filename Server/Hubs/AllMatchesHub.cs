using Microsoft.AspNetCore.SignalR;

namespace EventsManager.Server.Hubs;

public class AllMatchesHub : Hub
{
    public async Task SendMessage()
    {
        await Clients.All.SendAsync("ReceiveMessage");
    }
}
