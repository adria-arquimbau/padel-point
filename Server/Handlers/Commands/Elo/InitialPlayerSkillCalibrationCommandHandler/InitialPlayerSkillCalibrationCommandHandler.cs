using EventsManager.Server.Data;
using EventsManager.Server.Models;
using EventsManager.Shared.Enums;
using EventsManager.Shared.Exceptions;
using EventsManager.Shared.Requests;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventsManager.Server.Handlers.Commands.Elo.InitialPlayerSkillCalibrationCommandHandler;

public class InitialPlayerSkillCalibrationCommandHandler : IRequestHandler<InitialPlayerSkillCalibrationCommandRequest>
{
    private readonly ApplicationDbContext _context;

    public InitialPlayerSkillCalibrationCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task Handle(InitialPlayerSkillCalibrationCommandRequest request, CancellationToken cancellationToken)
    {
        var player = await _context.Player
            .Include(x => x.Announcements)
            .Include(player => player.EloHistories)
            .SingleAsync(x => x.UserId == request.UserId, cancellationToken: cancellationToken);
        
        var didItBefore = await _context.EloHistories
            .Where(x => x.PlayerId == player.Id && x.ChangeReason == ChangeEloHistoryReason.InitialSkillCalibration)
            .AnyAsync(cancellationToken: cancellationToken);

        if (didItBefore)
        {
            throw new InitialSkillCalibrationAlreadyDoneException("Initial skill calibration already done");
        }

        var newElo = CalculateInitialElo(request.Request, player.Elo);
        player.EloHistories.Add(new EloHistory
        {
            NewElo = newElo,    
            OldElo = player.EloHistories.Any() ? player.Elo : 0,
            ChangeDate = DateTime.Now,
            ChangeReason = ChangeEloHistoryReason.InitialSkillCalibration,
            EloChange = newElo - (player.EloHistories.Any() ? player.Elo : 0)
        });
        
        player.Elo = newElo;
        
        player.Announcements.InitialLevelFormDone = true;
        player.InitialLevelForm = new InitialLevelForm
        {
            OtherRacketSportsYearsPlaying = request.Request.OtherRacketSportsYearsPlaying,
            OtherRacketSportsLevel = request.Request.PlayedOtherRacketSportsBefore == false ? OtherRacketSportsLevel.None : request.Request.OtherRacketSportsLevel,
            PlayedOtherRacketSportsBefore = request.Request.PlayedOtherRacketSportsBefore,
            SelfAssessedPadelSkillLevel = request.Request.SelfAssessedPadelSkillLevel,
            YearsPlayingPadel = request.Request.YearsPlayingPadel,
        };
        
        await _context.SaveChangesAsync(cancellationToken);
    }
    
    private static int CalculateInitialElo(InitialLevelFormRequest request, int eloPoints)
    {
        // For "Other Racket Sports"
        if(request.PlayedOtherRacketSportsBefore)
        {
            eloPoints += Math.Min(request.OtherRacketSportsYearsPlaying, 5) * 10;  // Add 8 points per year of experience, up to 5 years

            // Add points based on the other racket sports level
            switch(request.OtherRacketSportsLevel)
            {
                case OtherRacketSportsLevel.Amateur:
                    eloPoints += 30;
                    break;
                case OtherRacketSportsLevel.Competitive:
                    eloPoints += 50;  
                    break;
                case OtherRacketSportsLevel.SemiPro:
                    eloPoints += 75;  
                    break;
                case OtherRacketSportsLevel.Pro:
                    eloPoints += 150;  
                    break;
            }
        }

        switch (request.SelfAssessedPadelSkillLevel)
        {
            // Add points based on Padel skill level
            case SelfAssessedPadelSkillLevel.Beginner:
                break;
            case SelfAssessedPadelSkillLevel.BeginnerIntermediate:
                eloPoints += 90; 
                break;
            case SelfAssessedPadelSkillLevel.Intermediate:
                eloPoints += 135; // 90 + (350 - 90) / 2^5
                break;
            case SelfAssessedPadelSkillLevel.IntermediateAdvanced:
                eloPoints += 168; // 90 + (350 - 90) / 2^4
                break;
            case SelfAssessedPadelSkillLevel.Advanced:
                eloPoints += 203; // 90 + (350 - 90) / 2^3 
                break;
            case SelfAssessedPadelSkillLevel.SemiPro:
                eloPoints += 244; // 90 + (350 - 90) / 2^2
                break;
            case SelfAssessedPadelSkillLevel.Pro:
                eloPoints += 350; // Pro level
                break;
        }


        // Add points based on years playing Padel
        eloPoints += Math.Min(request.YearsPlayingPadel, 5) * 15;  // Add 10 points per year, up to 5 years
        
        return eloPoints;
    }
}