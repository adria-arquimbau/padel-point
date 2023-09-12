using MediatR;

namespace EventsManager.Server.Handlers.Commands.Tournaments.CalculatePositions;

public class CalculateTournamentPositionsCommandRequest : IRequest
{
    public readonly Guid TournamentId;
    public readonly string UserId;

    public CalculateTournamentPositionsCommandRequest(Guid tournamentId, string userId)
    {
        TournamentId = tournamentId;
        UserId = userId;
    }
}