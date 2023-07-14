using EventsManager.Server.Data;
using EventsManager.Shared.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventsManager.Server.Handlers.Commands.User.UpdateMyUser;

public class UpdateMyUserCommandHandler : IRequestHandler<UpdateMyUserCommandRequest>
{
    private readonly ApplicationDbContext _dbContext;

    public UpdateMyUserCommandHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task Handle(UpdateMyUserCommandRequest request, CancellationToken cancellationToken)
    {
        if (request.UserId != request.UserDto.Id)
        {
            throw new NotSameUserForUpdateException();
        }

        var userEntity = await _dbContext.Users.SingleAsync(x => x.Id == request.UserId, cancellationToken: cancellationToken);
        
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}