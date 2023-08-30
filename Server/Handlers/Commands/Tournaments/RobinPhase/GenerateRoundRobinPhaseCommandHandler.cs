using EventsManager.Server.Data;
using EventsManager.Server.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Team = EventsManager.Shared.Enums.Team;

namespace EventsManager.Server.Handlers.Commands.Tournaments.RobinPhase;

public class GenerateRoundRobinPhaseCommandHandler : IRequestHandler<GenerateRoundRobinPhaseCommandRequest>
{
    private readonly ApplicationDbContext _context;

    public GenerateRoundRobinPhaseCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task Handle(GenerateRoundRobinPhaseCommandRequest request, CancellationToken cancellationToken)
    {
        var tournament = await _context.Tournament
            .Where(x => x.Id == request.TournamentId)
            .Include(x => x.Creator)
            .Include(x => x.Teams)
            .ThenInclude(x => x.Player1)
            .Include(x => x.Teams)
            .ThenInclude(x => x.Player2)
            .SingleAsync(cancellationToken: cancellationToken);
        
        if (tournament.Creator.UserId != request.UserId)
        {
            throw new UnauthorizedAccessException("You are not the creator of this tournament");
        }

        if (tournament.MaxTeams != tournament.Teams.Count)
        {
            throw new InvalidOperationException("The tournament is not full");
        }
        
        var teamPairs = tournament.Teams.SelectMany((team1, index1) =>
                tournament.Teams.Where((team2, index2) => index2 > index1)
                    .Select(team2 => new { Team1 = team1, Team2 = team2 }))
            .ToList();

        var matchStartTime = tournament.StartDate; // Set the desired start time
        const int matchDuration = 1; // Set the desired match duration

        var matchGenerator = new RoundRobinMatchGenerator();
    
        foreach (var pair in teamPairs)
        {
            var playersTeam1 = new List<Player> { pair.Team1.Player1, pair.Team1.Player2 };
            var playersTeam2 = new List<Player> { pair.Team2.Player1, pair.Team2.Player2 };

            var match = matchGenerator.CreateMatch(playersTeam1, playersTeam2, matchStartTime, matchDuration, tournament);
        
           _context.Match.Add(match);
        }
        
        await _context.SaveChangesAsync(cancellationToken); 
    }
}

public class RoundRobinMatchGenerator
{
    public Match CreateMatch(List<Player> team1Players, List<Player> team2Players, DateTime matchStartTime, double matchDuration, Tournament tournament)
    {
        Match match = new Match
        {
            Id = Guid.NewGuid(),
            CreationDate = DateTime.Now,
            Creator = tournament.Creator,
            MatchPlayers = new List<MatchPlayer>
            {
                new MatchPlayer { Player = team1Players[0], PlayerId = team1Players[0].Id, Team = Team.Team1},
                new MatchPlayer { Player = team1Players[1], PlayerId = team1Players[1].Id, Team = Team.Team1},
                new MatchPlayer { Player = team2Players[0], PlayerId = team2Players[0].Id, Team = Team.Team2},
                new MatchPlayer { Player = team2Players[1], PlayerId = team2Players[1].Id, Team = Team.Team2}
            },
            StartDateTime = matchStartTime,
            Duration = matchDuration,
            TournamentId = tournament.Id,
            Location = tournament.Location
            // Set other properties as needed
        };
        
        return match;
    }
}