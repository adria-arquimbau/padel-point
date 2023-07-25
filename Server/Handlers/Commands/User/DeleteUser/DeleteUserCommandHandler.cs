using EventsManager.Server.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventsManager.Server.Handlers.Commands.User.DeleteUser;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommandRequest>
{
    private readonly ApplicationDbContext _dbContext;

    public DeleteUserCommandHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task Handle(DeleteUserCommandRequest request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .Include(x => x.Player)
            .ThenInclude(x => x.EloHistories)
            .SingleAsync(x => x.Id == request.UserId, cancellationToken: cancellationToken);
        
        _dbContext.EloHistories.RemoveRange(user.Player.EloHistories);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await _dbContext.Users
            .Where(x => x.Id == request.UserId)
            .ExecuteDeleteAsync(cancellationToken: cancellationToken);
    }
}