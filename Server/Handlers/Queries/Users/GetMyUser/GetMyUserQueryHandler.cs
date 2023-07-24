using EventsManager.Server.Data;
using EventsManager.Shared.Dtos;
using EventsManager.Shared.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventsManager.Server.Handlers.Queries.Users.GetMyUser;

public class GetMyUserQueryHandler : IRequestHandler<GetMyUserQueryRequest, UserDto>
{
    private readonly ApplicationDbContext _dbContext;

    public GetMyUserQueryHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }


    public async Task<UserDto> Handle(GetMyUserQueryRequest request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .Include(x => x.Player)
            .SingleAsync(x => x.Id == request.UserId, cancellationToken);

        if(user == null)
        {
            throw new UserNotFoundException("User not found with Id: " + request.UserId);
        }
        
        return new UserDto
        {
            Id = user.Id,
            UserName = user.UserName!,
            Email = user.Email!,
            ImageUrl = user.Player.ImageUrl,
            Country = user.Player.Country,
            PlayerId = user.Player.Id,
        };
    }
}
