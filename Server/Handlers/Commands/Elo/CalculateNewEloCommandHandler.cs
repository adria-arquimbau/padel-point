using EventsManager.Server.Data;
using EventsManager.Server.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventsManager.Server.Handlers.Commands.Elo;

public class CalculateNewEloCommandHandler : IRequestHandler<CalculateNewEloCommandRequest>
{
    private readonly ApplicationDbContext _context;

    public CalculateNewEloCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task Handle(CalculateNewEloCommandRequest request, CancellationToken cancellationToken)
    {
         var match = await _context.Match
        .Where(x => x.Id == request.MatchId)
        .Include(x => x.MatchPlayers)
            .ThenInclude(x => x.Player)
        .Include(x => x.Sets)
        .SingleAsync(cancellationToken: cancellationToken);

    // Assume team1 is the first two players and team2 is the second two
    var players = match.MatchPlayers.ToList();
    var team1 = new [] { players[0].Player, players[1].Player };
    var team2 = new [] { players[2].Player, players[3].Player };

    // Assume team1 won if they have a higher total score than team2
    var team1TotalScore = match.Sets.Sum(set => set.Team1Score);
    var team2TotalScore = match.Sets.Sum(set => set.Team2Score);
    var team1Won = team1TotalScore > team2TotalScore;

    const int k = 32;

    foreach (var player in team1)
    {
        var otherTeamElo = team2.Average(p => p.Elo);

        var expectedScore = 1.0 / (1.0 + Math.Pow(10, (otherTeamElo - player.Elo) / 400.0));
        var actualScore = team1Won ? 1.0 : 0.0;

        var newElo = player.Elo + (int)(k * (actualScore - expectedScore));
        
        player.EloHistories.Add(new EloHistory
        {
            PreviousElo = player.Elo,
            CurrentElo = newElo,
            ChangeDate = DateTime.UtcNow,
            MatchId = match.Id,
            PlayerId = player.Id
        });

        player.Elo = newElo;
    }

    foreach (var player in team2)
    {
        var otherTeamElo = team1.Average(p => p.Elo);

        var expectedScore = 1.0 / (1.0 + Math.Pow(10, (otherTeamElo - player.Elo) / 400.0));
        var actualScore = team1Won ? 0.0 : 1.0;

        var newElo = player.Elo + (int)(k * (actualScore - expectedScore));

        player.EloHistories.Add(new EloHistory
        {
            PreviousElo = player.Elo,
            CurrentElo = newElo,
            ChangeDate = DateTime.UtcNow,
            MatchId = match.Id,
            PlayerId = player.Id
        });

        player.Elo = newElo;
    }

    await _context.SaveChangesAsync(cancellationToken);
    }
}