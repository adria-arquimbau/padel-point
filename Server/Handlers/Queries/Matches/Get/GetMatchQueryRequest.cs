using EventsManager.Shared.Responses;
using MediatR;

namespace EventsManager.Server.Handlers.Queries.Matches.Get;

public class GetMatchQueryRequest : IRequest<MatchResponse>
{
    public readonly string? UserId;
    public readonly Guid MatchId;

    public GetMatchQueryRequest(string? userId, Guid matchId)
    {
        UserId = userId;
        MatchId = matchId;
    }
}   
