using EventsManager.Server.Data;
using EventsManager.Shared.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventsManager.Server.Handlers.Queries.Tournaments.GetPositions.CalculatePositions;

public class CalculateTournamentPositionsQueryHandler : IRequestHandler<CalculateTournamentPositionsQueryRequest, List<TournamentTeamPositionResponse>>
{
    private readonly ApplicationDbContext _context;

    public CalculateTournamentPositionsQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<List<TournamentTeamPositionResponse>> Handle(CalculateTournamentPositionsQueryRequest request, CancellationToken cancellationToken)
    {
        var tournament = await _context.Tournament
            .Where(x => x.Id == request.TournamentId)
            .Include(x => x.Teams)
                .ThenInclude(x => x.Player1)
            .Include(x => x.Teams)
                .ThenInclude(x => x.Player2)
            .Include(x => x.RoundRobinMatches)
                .ThenInclude(x => x.Sets)
            .SingleAsync(cancellationToken: cancellationToken);
        
        if (tournament.RoundRobinMatches.All(x => !x.Sets.Any()))
        {
            var orderedByPositionTeams = tournament.Teams
                .OrderByDescending(x => x.Player1.Elo + x.Player2.Elo / 2)
                .Select(x => new TournamentTeamPositionResponse
                {
                    Position = 0,
                    Team = new TeamResponse
                    {   
                        Id = x.Id,
                        AverageElo = x.Player1.Elo + x.Player2.Elo / 2,
                        Player1 = new PlayerDetailResponse
                        {
                            Id = x.Player1Id,
                            NickName = x.Player1.NickName,
                            Country = x.Player1.Country,
                            ImageUrl = x.Player1.ImageUrl,
                            Elo = x.Player1.Elo
                        },
                        Player2 = new PlayerDetailResponse
                        {
                            Id = x.Player2Id,
                            NickName = x.Player2.NickName,
                            Country = x.Player2.Country,
                            ImageUrl = x.Player2.ImageUrl,
                            Elo = x.Player2.Elo
                        }
                    }
                })
                .ToList();

            for (var i = 0; i < orderedByPositionTeams.Count; i++)
            {
                var team = orderedByPositionTeams[i];
                team.Position = i + 1;
            }

            return orderedByPositionTeams;
        }

        return new List<TournamentTeamPositionResponse>();
    }
}
