using MediatR;

namespace EventsManager.Server.Handlers.Commands.Tournaments.RobinPhase;

public class GenerateRoundRobinPhaseCommandRequest : IRequest
{
    public Guid TournamentId;
    public string UserId;

    public GenerateRoundRobinPhaseCommandRequest(Guid tournamentId, string userId)
    {
        TournamentId = tournamentId;
        UserId = userId;
    }
}