using EventsManager.Server.Data;
using EventsManager.Server.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

        var matchStartTime = tournament.StartDate;
        const int matchDuration = 1;

        var matchGenerator = new RoundRobinMatchGenerator();

        // Check if there's a need to divide teams into groups
        if (tournament.RoundRobinPhaseGroups > 1)
        {
            var teamsPerGroup = tournament.Teams.Count / tournament.RoundRobinPhaseGroups;
            var roundsPerGroup = 3; // Number of rounds is fixed to 3 in multi-group scenario

            for (var group = 0; group < tournament.RoundRobinPhaseGroups; group++)
            {
                var groupTeams = tournament.Teams.Skip(group * teamsPerGroup).Take(teamsPerGroup).ToList();

                // Loop through each round
                for (var round = 1; round <= roundsPerGroup; round++)
                {
                    foreach (var team1 in groupTeams)
                    {
                        foreach (var team2 in groupTeams.Where(t => t != team1))
                        {
                            var playersTeam1 = new List<Player> { team1.Player1, team1.Player2 };
                            var playersTeam2 = new List<Player> { team2.Player1, team2.Player2 };

                            var match = RoundRobinMatchGenerator.CreateMatch(playersTeam1, playersTeam2, matchStartTime, matchDuration, tournament, group + 1, round);

                            _context.Match.Add(match);
                        }
                    }
                }
            }
        }
        else
        {
            foreach (var pair in teamPairs)
            {
                var playersTeam1 = new List<Player> { pair.Team1.Player1, pair.Team1.Player2 };
                var playersTeam2 = new List<Player> { pair.Team2.Player1, pair.Team2.Player2 };

                var match = RoundRobinMatchGenerator.CreateMatch(playersTeam1, playersTeam2, matchStartTime, matchDuration, tournament, 1,1);

                _context.Match.Add(match);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

    }
}

public class RoundRobinMatchGenerator
{
    public static Match CreateMatch(List<Player> team1Players, List<Player> team2Players, DateTime matchStartTime, double matchDuration, Tournament tournament, int group, int round)
    {
        var match = new Match
        {
            Id = Guid.NewGuid(),
            CreationDate = DateTime.Now,
            Creator = tournament.Creator,
            MatchPlayers = new List<MatchPlayer>
            {
                new() { Player = team1Players[0], PlayerId = team1Players[0].Id, Team = Team.Team1, Confirmed = true},
                new() { Player = team1Players[1], PlayerId = team1Players[1].Id, Team = Team.Team1, Confirmed = true},
                new() { Player = team2Players[0], PlayerId = team2Players[0].Id, Team = Team.Team2, Confirmed = true},
                new() { Player = team2Players[1], PlayerId = team2Players[1].Id, Team = Team.Team2, Confirmed = true}
            },
            StartDateTime = matchStartTime,
            Duration = matchDuration,
            TournamentId = tournament.Id,
            Location = tournament.Location,
            RobinPhaseGroup = group,
            RobinPhaseRound = round
            // Set other properties as needed
        };

        return match;
    }
}