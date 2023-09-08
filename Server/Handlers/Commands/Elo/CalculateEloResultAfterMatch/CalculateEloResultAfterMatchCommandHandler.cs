using EventsManager.Server.Data;
using EventsManager.Server.Models;
using EventsManager.Shared.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Team = EventsManager.Shared.Enums.Team;

namespace EventsManager.Server.Handlers.Commands.Elo.CalculateEloResultAfterMatch;

public class CalculateEloResultAfterMatchCommandHandler : IRequestHandler<CalculateEloResultAfterMatchCommandRequest>
{
    private readonly ApplicationDbContext _context;

    public CalculateEloResultAfterMatchCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task Handle(CalculateEloResultAfterMatchCommandRequest request, CancellationToken cancellationToken)
    {
         var match = await _context.Match
            .Where(x => x.Id == request.MatchId)
            .Include(x => x.MatchPlayers)
                .ThenInclude(x => x.Player)
            .Include(x => x.Sets)
            .SingleAsync(cancellationToken: cancellationToken);

        // Assume team1 is the first two players and team2 is the second two
        var playersTeamOne = match.MatchPlayers.Where(x => x.Team == Team.Team1).ToList();
        var playersTeamTwo = match.MatchPlayers.Where(x => x.Team == Team.Team2).ToList();
        var team1 = new [] { playersTeamOne[0].Player, playersTeamOne[1].Player };
        var team2 = new [] { playersTeamTwo[0].Player, playersTeamTwo[1].Player };

        // Assume team1 won if they have a higher total score than team2
        var team1TotalScore = match.Sets.Sum(set => set.Team1Score);
        var team2TotalScore = match.Sets.Sum(set => set.Team2Score);
        var team1Won = team1TotalScore > team2TotalScore;
        var team2Won = team2TotalScore > team1TotalScore;
        
        foreach (var player in team1)
        {
            if (!match.IsCompetitive)
            {
                player.EloHistories.Add(new EloHistory
                {
                    OldElo = player.Elo,
                    NewElo = player.Elo,
                    EloChange = 0,
                    ChangeDate = DateTime.Now,
                    MatchId = match.Id,
                    PlayerId = player.Id,
                    ChangeReason = ChangeEloHistoryReason.NonCompetitiveMatchPlayed
                });
                
                continue;
            }
            
            var kFactor = GetKFactor(player.Elo);
            var otherTeamElo = team2.Average(p => p.Elo);
            var expectedScore = 1.0 / (1.0 + Math.Pow(10, (otherTeamElo - player.Elo) / 400.0));
            var actualScore = team1Won ? 1.0 + (team1TotalScore - team2TotalScore) / 10.0 : 0.0;
            var eloChange = kFactor * (actualScore - expectedScore);
            if (!team1Won)
            {
                eloChange *= 1.8; // Double the amount of Elo lost if the team lost the match
            }
            var newElo = player.Elo + (int)eloChange;
            
            CreateEloHistory(player, newElo, match);
            player.Elo = newElo;
        }
        
        foreach (var player in team2)
        {
            if (!match.IsCompetitive)
            {
                player.EloHistories.Add(new EloHistory
                {
                    OldElo = player.Elo,
                    NewElo = player.Elo,
                    EloChange = 0,
                    ChangeDate = DateTime.Now,
                    MatchId = match.Id,
                    PlayerId = player.Id,
                    ChangeReason = ChangeEloHistoryReason.NonCompetitiveMatchPlayed
                });
                
                continue;;
            }
            
            var kFactor = GetKFactor(player.Elo);
            var otherTeamElo = team1.Average(p => p.Elo);
            var expectedScore = 1.0 / (1.0 + Math.Pow(10, (otherTeamElo - player.Elo) / 400.0));
            var actualScore = team1Won ? 0.0 : 1.0 + (team2TotalScore - team1TotalScore) / 10.0;
            var eloChange = kFactor * (actualScore - expectedScore);
            if (!team2Won)
            {
                eloChange *= 1.8; // Double the amount of Elo lost if the team lost the match
            }

            var newElo = player.Elo + (int)eloChange;
            
            CreateEloHistory(player, newElo, match);
            player.Elo = newElo;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
    
    private static int GetKFactor(int elo)
    {
        if (elo < 1650)
        {
            return 40;
        }
        if (elo is >= 1650 and < 1899)
        {
            return 32;
        }

        return 24;
    }
    
    private static void CreateEloHistory(Player player, int newElo, Match match)
    {
        
            player.EloHistories.Add(new EloHistory
            {
                OldElo = player.Elo,
                NewElo = newElo,
                EloChange = newElo - player.Elo,
                ChangeDate = DateTime.Now,
                MatchId = match.Id,
                PlayerId = player.Id,
                ChangeReason = ChangeEloHistoryReason.MatchPlayed
            });

            player.Notifications.Add(new Notification
            {
                CreationDate = DateTime.Now,
                Title = "Elo Change",
                Description = "Your elo has changed, your new elo points are " + newElo,
            });
        
    }
}
