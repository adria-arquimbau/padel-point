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
        
        var teams = tournament.Teams.ToList();

        if (tournament.RoundRobinPhaseGroups >= 2)
        {
            var groups = GetGroups(teams, tournament);
    
            var groupNumber = 1;
            foreach (var group in groups)
            {
                var numberOfTeams = group.Count;
                var numberOfRounds = numberOfTeams - 1;

                for (var round = 1; round <= numberOfRounds; round++)
                {
                    for (var match = 0; match < numberOfTeams / 2; match++)
                    {
                        var team1Players = new List<Player> { group[match].Player1, group[match].Player2 };
                        var team2Players = new List<Player> { group[numberOfTeams - 1 - match].Player1, group[numberOfTeams - 1 - match].Player2 };

                        var matchStartTime = tournament.StartDate.AddMinutes((round - 1) * 0.30);
                        var matchDuration = 0.30;

                        var matchToCreate = RoundRobinMatchGenerator.CreateMatch(team1Players, team2Players, matchStartTime, matchDuration, tournament, groupNumber, round);

                        _context.Match.Add(matchToCreate);
                    }

                    // Rotate the array
                    var lastTeam = group[group.Count - 1];
                    group.RemoveAt(group.Count - 1);
                    group.Insert(1, lastTeam);
                }
                
                groupNumber++;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    private static IEnumerable<List<Models.Team>> GetGroups(List<Models.Team> teams, Tournament tournament)
    {
        var random = new Random();

        // Shuffle the teams randomly
        teams = teams.OrderBy(t => random.Next()).ToList();

        // Calculate the number of teams per group
        var teamsPerGroup = teams.Count / tournament.RoundRobinPhaseGroups;

        // Create and distribute teams to groups
        var groups = Enumerable.Range(0, tournament.RoundRobinPhaseGroups)
            .Select(i => teams.Skip(i * teamsPerGroup).Take(teamsPerGroup).ToList())
            .ToList();

        // Handle any remaining teams (if teams.Count is not a multiple of numberOfGroups)
        for (var i = 0; i < teams.Count % tournament.RoundRobinPhaseGroups; i++)
        {
            groups[i].Add(teams[i + (tournament.RoundRobinPhaseGroups * teamsPerGroup)]);
        }

        // Now, 'groups' contains your randomly divided teams in the specified number of groups.
        return groups;
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
            RobinPhaseRound = round,
            // Set other properties as needed
        };

        return match;
    }
}