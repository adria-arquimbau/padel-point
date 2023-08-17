using EventsManager.Server.Data;
using EventsManager.Shared.Dtos;
using EventsManager.Shared.Enums;
using EventsManager.Shared.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventsManager.Server.Handlers.Queries.Matches.Get;

public class GetMatchQueryHandler : IRequestHandler<GetMatchQueryRequest, MatchResponse>
{
    private readonly ApplicationDbContext _context;

    public GetMatchQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<MatchResponse> Handle(GetMatchQueryRequest request, CancellationToken cancellationToken)
    {
        var userId = request.UserId;
        var matchId = request.MatchId;

        int? requesterElo = null;
        if (userId != null)
        {
            requesterElo = await _context.Player
                .Where(x => x.UserId == userId)
                .Select(x => x.Elo)
                .SingleOrDefaultAsync(cancellationToken: cancellationToken);
        }
        
        var match = await _context.Match
            .Where(x => x.Id == matchId)
            .Select(x => new MatchResponse
            {
                Id = x.Id,
                Location = x.Location,
                IsCompetitive = x.IsCompetitive,
                IsBlocked = x.IsBlocked,
                IHaveOpenInvitation = x.MatchPlayers.Any(p => p.Player.UserId == userId && !p.Confirmed),
                RequesterElo = userId != null ? requesterElo : null,
                MinimumLevel = x.MinimumLevel,
                IAmAlreadyRegistered = userId != null && x.MatchPlayers.Any(p => p.Player.UserId == userId),
                RequesterIsTheCreator = userId != null && x.Creator.UserId == userId,
                RequesterIsAPlayer = x.MatchPlayers.Any(p => p.Player.UserId == userId),
                StartDateTime = x.StartDateTime,
                Duration = x.Duration,
                CreatorNickName = x.Creator.NickName,
                IsPrivate = x.IsPrivate,
                PricePerHour = x.PricePerHour,
                MyTeam = x.MatchPlayers.Where(p => p.Player.UserId == userId).Select(p => p.Team).SingleOrDefault(),
                PlayersCount = x.MatchPlayers.Count,
                ScoreConfirmedTeamOne = x.ScoreConfirmedTeamOne,
                ScoreConfirmedTeamTwo = x.ScoreConfirmedTeamTwo,
                TeamWinner = x.Winner,
                Sets = x.Sets.Select(s => new SetDto
                {
                    SetNumber = s.SetNumber,
                    Team1Score = s.Team1Score,
                    Team2Score = s.Team2Score
                }).OrderBy(s => s.SetNumber).ToList(),
                PlayersTeamOne = x.MatchPlayers.Where(p => p.Team == Team.Team1)
                    .Select(p => new PlayerDto
                {
                    Id = p.Player.Id,
                    NickName = p.Player.NickName,
                    Country = p.Player.Country,
                    IsConfirmed = p.Confirmed,
                    ImageUrl = p.Player.ImageUrl,
                    EloBeforeFinish = p.Player.EloHistories
                        .Where(e => e.MatchId == matchId)
                        .Select(e => (int?)e.OldElo)
                        .SingleOrDefault() ?? p.Player.Elo,
                    CanIDeleteIt = userId != null && p.Player.UserId == userId,
                    GainedElo = p.Player.EloHistories.Where(e => e.MatchId == matchId).Sum(e => e.EloChange),
                }).ToList(),
                PlayersTeamTwo = x.MatchPlayers.Where(p => p.Team == Team.Team2)
                    .Select(p => new PlayerDto
                {
                    Id = p.Player.Id,
                    NickName = p.Player.NickName,
                    Country = p.Player.Country,
                    IsConfirmed = p.Confirmed,
                    ImageUrl = p.Player.ImageUrl,
                    EloBeforeFinish = p.Player.EloHistories
                        .Where(e => e.MatchId == matchId)
                        .Select(e => (int?)e.OldElo)
                        .SingleOrDefault() ?? p.Player.Elo,
                    CanIDeleteIt = userId != null && p.Player.UserId == userId,
                    GainedElo = p.Player.EloHistories.Where(e => e.MatchId == matchId).Sum(e => e.EloChange),
                }).ToList()
            })
            .SingleOrDefaultAsync(cancellationToken: cancellationToken);
        
        if (match == null)
        {
            throw new NullReferenceException();
        }
        
        match.AverageEloTeamOne = match.PlayersTeamOne.Any()
            ? (int)Math.Round(match.PlayersTeamOne.Average(mp => mp.EloBeforeFinish)) : 0;

        match.AverageEloTeamTwo = match.PlayersTeamTwo.Any()
            ? (int)Math.Round(match.PlayersTeamTwo.Average(mp => mp.EloBeforeFinish)) : 0;
        
        var eloDifference = match.AverageEloTeamTwo - match.AverageEloTeamOne;
        var probabilityTeamOneWins = 1 / (1 + Math.Pow(10, eloDifference / 400.0));
        var probabilityTeamTwoWins = 1 - probabilityTeamOneWins;

        match.ProbabilityTeamOneWins = Math.Round(probabilityTeamOneWins * 100, 2);
        match.ProbabilityTeamTwoWins = Math.Round(probabilityTeamTwoWins * 100, 2);
        
        return match;
    }
}
