using EventsManager.Shared.Responses;
using MediatR;

namespace EventsManager.Server.Handlers.Queries.Tournaments.GetPositions.CalculatePositions;

public class CalculateTournamentPositionsQueryRequest : IRequest<List<TournamentTeamPositionResponse>>
{
    public readonly Guid TournamentId;
    public readonly string UserId;

    public CalculateTournamentPositionsQueryRequest(Guid tournamentId, string userId)
    {
        TournamentId = tournamentId;
        UserId = userId;
    }
}
