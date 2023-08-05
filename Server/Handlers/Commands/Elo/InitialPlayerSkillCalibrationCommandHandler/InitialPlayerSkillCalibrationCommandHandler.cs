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
            .SingleAsync(x => x.UserId == request.UserId, cancellationToken: cancellationToken);
        
        var didItBefore = await _context.EloHistories
            .Where(x => x.PlayerId == player.Id && x.ChangeReason == ChangeEloHistoryReason.InitialSkillCalibration)
            .AnyAsync(cancellationToken: cancellationToken);

        if (didItBefore)
        {
            throw new InitialSkillCalibrationAlreadyDoneException("Initial skill calibration already done");
        }
        
        var newElo = CalculateInitialElo(request.Request);
        
        player.EloHistories.Add(new EloHistory
        {
            CurrentElo = newElo,    
            PreviousElo = player.Elo,
            ChangeDate = DateTime.Now,
            ChangeReason = ChangeEloHistoryReason.InitialSkillCalibration,
            EloChange = newElo - player.Elo,
        });
        
        player.Elo = newElo;
        
        player.Announcements.InitialLevelFormDone = true;
        
        await _context.SaveChangesAsync(cancellationToken);
    }
    
    private static int CalculateInitialElo(InitialLevelFormRequest request)
    {
        var eloPoints = 1500;  // base Elo score
        
        // For "Other Racket Sports"
        if(request.PlayedOtherRacketSportsBefore)
        {
            eloPoints += 100;  // Add 100 points if player has other racket sports experience
            eloPoints += Math.Min(request.OtherRacketSportsYearsPlaying, 5) * 8;  // Add 8 points per year of experience, up to 5 years

            // Add points based on the other racket sports level
            switch(request.OtherRacketSportsLevel)
            {
                case OtherRacketSportsLevel.Amateur:
                    eloPoints += 30;
                    break;
                case OtherRacketSportsLevel.SemiPro:
                    eloPoints += 130;  // Increase to 130 points for semi-pro level
                    break;
                case OtherRacketSportsLevel.Pro:
                    eloPoints += 230;  // Increase to 230 points for pro level
                    break;
            }
        }

        switch (request.SelfAssessedPadelSkillLevel)
        {
            // Add points based on Padel skill level
            case SelfAssessedPadelSkillLevel.Beginner:
                break;
            case SelfAssessedPadelSkillLevel.BeginnerIntermediate:
                eloPoints += 80; // Increased from 60
                break;
            case SelfAssessedPadelSkillLevel.Intermediate:
                eloPoints += 180; // Increased from 150
                break;
            case SelfAssessedPadelSkillLevel.IntermediateAdvanced:
                eloPoints += 280; // Increased from 240
                break;
            case SelfAssessedPadelSkillLevel.Advanced:
                eloPoints += 400; // Increased from 330
                break;
            case SelfAssessedPadelSkillLevel.SemiPro:
                eloPoints += 500; // Increased from 420
                break;
            case SelfAssessedPadelSkillLevel.Pro:
                eloPoints += 700; // Increased from 600
                break;
        }

        // Add points based on years playing Padel
        eloPoints += Math.Min(request.YearsPlayingPadel, 5) * 10;  // Add 10 points per year, up to 5 years
        
        return eloPoints;
    }
}