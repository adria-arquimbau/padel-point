using MediatR;

namespace EventsManager.Server.Handlers.Commands.Elo.CalculateEloResultAfterMatch;

public class CalculateEloResultAfterMatchCommandRequest : IRequest
{
    public readonly Guid MatchId;

    public CalculateEloResultAfterMatchCommandRequest(Guid matchId)
    {
        MatchId = matchId;
    }
}