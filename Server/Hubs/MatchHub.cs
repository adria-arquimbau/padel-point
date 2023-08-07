using Microsoft.AspNetCore.SignalR;

namespace EventsManager.Server.Hubs;

public class MatchHub : Hub
{
    public async Task JoinGroup(string matchId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, matchId);
    }
    
    public async Task SendMessage(string matchId)
    {
        await Clients.Group(matchId).SendAsync("ReceiveMessage", matchId);
    }
}   
