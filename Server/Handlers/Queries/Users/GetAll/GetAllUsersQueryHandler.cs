using EventsManager.Server.Data;
using EventsManager.Shared.Dtos;
using EventsManager.Shared.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventsManager.Server.Handlers.Queries.Users.GetAll;

public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQueryRequest, List<UserDto>>
{
    private readonly ApplicationDbContext _dbContext;

    public GetAllUsersQueryHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<List<UserDto>> Handle(GetAllUsersQueryRequest request, CancellationToken cancellationToken)
    {
        var users = await _dbContext.Users.Select(x => new UserDto
        {
            Id = x.Id,
            UserName = x.UserName,
            Email = x.Email,
            ImageUrl = x.Player.ImageUrl,
            EmailConfirmed = x.EmailConfirmed,
            Country = x.Player.Country,
            Elo = x.Player.Elo,
            EloHistory = x.Player.EloHistories.Select(eh => new EloHistoryResponse
            {
                CurrentElo = eh.CurrentElo,
                PreviousElo = eh.PreviousElo,
                EloChange = eh.EloChange,
                ChangeReason = eh.ChangeReason,
                ChangeDate = eh.ChangeDate
            }).OrderByDescending(x => x.ChangeDate).ToList()
        })
        .ToListAsync(cancellationToken: cancellationToken);

        return users;
    }
}
