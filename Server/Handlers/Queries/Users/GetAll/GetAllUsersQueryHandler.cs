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
            RegistrationDate = x.RegistrationDate,
            ImageUrl = x.Player != null ? x.Player.ImageUrl : null,
            EmailConfirmed = x.Player != null ? x.EmailConfirmed : false,
            Country = x.Player != null ? x.Player.Country : null,
            Elo = x.Player != null ? x.Player.Elo : 0,
            PlayerId = x.Player != null ? x.Player.Id : null,
            InitialPlayerSkillCalibration = x.Player.InitialLevelForm != null ? new InitialPlayerSkillCalibrationResponse
            {
                OtherRacketSportsYearsPlaying = x.Player.InitialLevelForm.OtherRacketSportsYearsPlaying,
                PlayedOtherRacketSportsBefore = x.Player.InitialLevelForm.PlayedOtherRacketSportsBefore,
                OtherRacketSportsLevel = x.Player.InitialLevelForm.OtherRacketSportsLevel,
                SelfAssessedPadelSkillLevel = x.Player.InitialLevelForm.SelfAssessedPadelSkillLevel,
                YearsPlayingPadel = x.Player.InitialLevelForm.YearsPlayingPadel
            } : null,
            EloHistory = x.Player.EloHistories.Select(eh => new EloHistoryResponse
            {
                CurrentElo = eh.NewElo, 
                PreviousElo = eh.OldElo,
                EloChange = eh.EloChange,
                ChangeReason = eh.ChangeReason,
                ChangeDate = eh.ChangeDate
            }).OrderByDescending(eh => eh.ChangeDate).ToList()
        }).OrderByDescending(x => x.RegistrationDate)
        .ToListAsync(cancellationToken: cancellationToken);

        return users;
    }
}