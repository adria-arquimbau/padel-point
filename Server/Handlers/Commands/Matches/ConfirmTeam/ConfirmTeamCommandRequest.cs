using MediatR;

namespace EventsManager.Server.Handlers.Commands.Matches.ConfirmTeam;

public class ConfirmTeamCommandRequest : IRequest
{
    public bool AdminRequest;
    public readonly string? UserId;
    public readonly Guid MatchId;
    public readonly bool Confirmation;

    public ConfirmTeamCommandRequest(string? userId, Guid matchId, bool confirmation, bool adminRequest = false)
    {
        AdminRequest = adminRequest;
        UserId = userId;
        MatchId = matchId;
        Confirmation = confirmation;
    }
}   