using EventsManager.Server.Handlers.Queries.Matches.Get;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace EventsManager.Server.Hubs;

public class MatchHub : Hub
{
    private readonly IMediator _mediator;

    public MatchHub(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    public async Task JoinGroup(string matchId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, matchId);
    }
    
    public async Task SendMessage(string matchId, string? userId)
    {
        var match = await _mediator.Send(new GetMatchQueryRequest(userId, Guid.Parse(matchId)));
        await Clients.Group(matchId).SendAsync("ReceiveMessage", match);
    }
}   
