using MediatR;

namespace EventsManager.Server.Handlers.Commands.Elo;

public class CalculateNewEloCommandRequest : IRequest
{
    public readonly Guid MatchId;

    public CalculateNewEloCommandRequest(Guid matchId)
    {
        MatchId = matchId;
    }
}