using EventsManager.Server.Data;
using MediatR;

namespace EventsManager.Server.Handlers.Commands.Elo;

public class CalculateNewEloCommandHandler : IRequestHandler<CalculateNewEloCommandRequest>
{
    private readonly ApplicationDbContext _context;

    public CalculateNewEloCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public Task Handle(CalculateNewEloCommandRequest request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}