using EventsManager.Server.Data;
using MediatR;

namespace EventsManager.Server.Handlers.Commands.Tournaments.CalculatePositions;

public class CalculateTournamentPositionsCommandHandler : IRequestHandler<CalculateTournamentPositionsCommandRequest>
{
    private readonly ApplicationDbContext _context;

    public CalculateTournamentPositionsCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public Task Handle(CalculateTournamentPositionsCommandRequest request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}