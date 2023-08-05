using EventsManager.Server.Data;
using EventsManager.Shared.Dtos;
using EventsManager.Shared.Requests;
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
            ImageUrl = x.Player.ImageUrl,
            EmailConfirmed = x.EmailConfirmed,
            Country = x.Player.Country,
            Elo = x.Player.Elo,
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
                CurrentElo = eh.CurrentElo,
                PreviousElo = eh.PreviousElo,
                EloChange = eh.EloChange,
                ChangeReason = eh.ChangeReason,
                ChangeDate = eh.ChangeDate
            }).OrderByDescending(x => x.ChangeDate).ToList()
        }).OrderByDescending(x => x.RegistrationDate)
        .ToListAsync(cancellationToken: cancellationToken);

        return users;
    }
}