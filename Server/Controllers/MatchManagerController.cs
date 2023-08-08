using EventsManager.Server.Data;
using EventsManager.Server.Handlers.Commands.Matches.ConfirmTeam;
using EventsManager.Server.Models;
using EventsManager.Shared.Dtos;
using EventsManager.Shared.Enums;
using EventsManager.Shared.Responses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventsManager.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class MatchManagerController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMediator _mediator;

    public MatchManagerController(ApplicationDbContext dbContext, IMediator mediator)
    {
        _dbContext = dbContext;
        _mediator = mediator;
    }
    
    [HttpPost("delete-punctuation/{matchId:guid}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> DeletePunctuation([FromRoute] Guid matchId, CancellationToken cancellationToken)
    {
        var match = await _dbContext.Match
            .Where(x => x.Id == matchId)
            .SingleAsync(cancellationToken: cancellationToken);
        
        var eloHistoriesFromTheMatch = await _dbContext.EloHistories
            .Where(x => x.MatchId == matchId)
            .ToListAsync(cancellationToken: cancellationToken);
        
        var playersFromTheMatch = await _dbContext.MatchPlayer
            .Where(x => x.MatchId == matchId)
            .Select(x => x.Player)
            .ToListAsync(cancellationToken: cancellationToken);

        if (playersFromTheMatch.Any())  
        {
            foreach (var player in playersFromTheMatch)
            {
                var eloHistoryForThePlayer = eloHistoriesFromTheMatch
                    .SingleOrDefault(x => x.PlayerId == player.Id);

                if (eloHistoryForThePlayer is not null)
                {
                    player.Elo = eloHistoryForThePlayer.OldElo;
                }
            }
            _dbContext.EloHistories.RemoveRange(eloHistoriesFromTheMatch);
        }
        
        match.ScoreConfirmedTeamOne = false;
        match.ScoreConfirmedTeamTwo = false;
        match.Winner = null;
        
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return Ok();
    }
    
    [HttpGet("all")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var matches = await _dbContext.Match
            .Include(x => x.MatchPlayers)
            .ThenInclude(matchPlayer => matchPlayer.Player)
            .Include(x => x.EloHistories)
            .Include(match => match.Promotions)
            .Include(match => match.Creator)
            .OrderByDescending(x => x.CreationDate)
            .Select(x => new MatchAdministratorResponse
            {
                Id = x.Id,
                IsBlocked = x.IsBlocked,
                CreationDate = x.CreationDate,
                CreatorNickName = x.Creator.NickName,
                StartDateTime = x.StartDateTime,
                IsPrivate = x.IsPrivate,
                ScoreConfirmedTeamOne = x.ScoreConfirmedTeamOne,
                ScoreConfirmedTeamTwo = x.ScoreConfirmedTeamTwo,
                Duration = x.Duration,
                PlayersCount = x.MatchPlayers.Count,
                PlayersTeamOne = x.MatchPlayers.Where(mp => mp.Team == Team.Team1)
                    .Select(mp => new PlayerDto
                    {
                        NickName = mp.Player.NickName
                    }).ToList(),
                PlayersTeamTwo = x.MatchPlayers.Where(mp => mp.Team == Team.Team2)
                    .Select(mp => new PlayerDto
                    {
                        NickName = mp.Player.NickName,
                    }).ToList(),
                PlayersNames = x.MatchPlayers.Select(p => p.Player.NickName).ToList(),
                AverageElo = x.MatchPlayers.Any() ? 
                    (x.ScoreConfirmedTeamOne && x.ScoreConfirmedTeamTwo) ? (int)Math.Round(x.EloHistories.Average(eh => eh.OldElo)) : (int)Math.Round(x.MatchPlayers.Average(mp => mp.Player.Elo))
                    : 0,
                Promotions = x.Promotions.Select(p => new PromotionResponse
                {
                    Title = p.Title,
                    Description = p.Description
                }).ToList()
            })
            .ToListAsync(cancellationToken: cancellationToken);
        
        return Ok(matches);
    }
    
    [HttpDelete("{matchId:guid}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> DeleteMatch([FromRoute] Guid matchId, CancellationToken cancellationToken)
    {
        var match = await _dbContext.Match
            .Where(x => x.Id == matchId)
            .SingleAsync(cancellationToken: cancellationToken);
        
        var eloHistoriesFromTheMatch = await _dbContext.EloHistories
            .Where(x => x.MatchId == matchId)
            .ToListAsync(cancellationToken: cancellationToken);
        
        var playersFromTheMatch = await _dbContext.MatchPlayer
            .Where(x => x.MatchId == matchId)
            .Select(x => x.Player)
            .ToListAsync(cancellationToken: cancellationToken);

        if (playersFromTheMatch.Any())  
        {
            foreach (var player in playersFromTheMatch)
            {
                var eloHistoryForThePlayer = eloHistoriesFromTheMatch
                    .SingleOrDefault(x => x.PlayerId == player.Id);

                if (eloHistoryForThePlayer is not null)
                {
                    player.Elo = eloHistoryForThePlayer.OldElo;
                }
            }
            _dbContext.EloHistories.RemoveRange(eloHistoriesFromTheMatch);
        }
        
        _dbContext.Match.Remove(match);
        
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return Ok();
    }
    
    [HttpPut("{matchId:guid}/block/{isBlocked:bool}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> BlockMatch([FromRoute] Guid matchId, [FromRoute] bool isBlocked, CancellationToken cancellationToken)
    {
        var match = await _dbContext.Match
            .Where(x => x.Id == matchId)
            .SingleAsync(cancellationToken: cancellationToken);
        
        match.IsBlocked = isBlocked;
        
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return Ok();
    }
    
    [HttpPost("{matchId:guid}/confirm-all-results")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> ConfirmAllResultsMatch([FromRoute] Guid matchId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new ConfirmTeamCommandRequest(null, matchId, true, true), cancellationToken);
        
        return Ok();
    }
    
    [HttpPost("simulate")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Simulate(CancellationToken cancellationToken)
    {
        var player1 = new Player{Elo = 1700};
        var player2 = new Player{Elo = 1700};
        var player3 = new Player{Elo = 1800};
        var player4 = new Player{Elo = 1800};

        var player1PreviousElo = player1.Elo;
        var player2PreviousElo = player2.Elo;
        var player3PreviousElo = player3.Elo;
        var player4PreviousElo = player4.Elo;
        
        var sets = new List<Set>
        {
            new Set{SetNumber = 1, Team1Score = 6, Team2Score = 4},
            new Set{SetNumber = 2, Team1Score = 6, Team2Score = 3},
        };
        
        var team1 = new [] { player1, player2 };
        var team2 = new [] { player3, player4 };

        // Assume team1 won if they have a higher total score than team2
        var team1TotalScore = sets.Sum(set => set.Team1Score);
        var team2TotalScore = sets.Sum(set => set.Team2Score);
        var team1Won = team1TotalScore > team2TotalScore;
        var team2Won = team2TotalScore > team1TotalScore;
        
        foreach (var player in team1)
        {
            var kFactor = GetKFactor(player.Elo);
            var otherTeamElo = team2.Average(p => p.Elo);
            var expectedScore = 1.0 / (1.0 + Math.Pow(10, (otherTeamElo - player.Elo) / 400.0));
            var actualScore = team1Won ? 1.0 + (team1TotalScore - team2TotalScore) / 10.0 : 0.0;
            var eloChange = kFactor * (actualScore - expectedScore);
            if (!team1Won)
            {
                eloChange *= 2; // Double the amount of Elo lost if the team lost the match
            }
            var newElo = player.Elo + (int)eloChange;
            player.Elo = newElo;
        }

        // Repeat the same process for team 2
        foreach (var player in team2)
        {
            var kFactor = GetKFactor(player.Elo);
            var otherTeamElo = team1.Average(p => p.Elo);
            var expectedScore = 1.0 / (1.0 + Math.Pow(10, (otherTeamElo - player.Elo) / 400.0));
            var actualScore = team1Won ? 0.0 : 1.0 + (team2TotalScore - team1TotalScore) / 10.0;
            var eloChange = kFactor * (actualScore - expectedScore);
            if (!team2Won)
            {
                eloChange *= 2; // Double the amount of Elo lost if the team lost the match
            }

            var newElo = player.Elo + (int)eloChange;
            player.Elo = newElo;
        }
        
        Console.WriteLine("Player 1 elo gained: " + (player1.Elo - player1PreviousElo));
        Console.WriteLine("Player 2 elo gained: " + (player2.Elo - player2PreviousElo));
        Console.WriteLine("Player 3 elo gained: " + (player3.Elo - player3PreviousElo));
        Console.WriteLine("Player 4 elo gained: " + (player4.Elo - player4PreviousElo));
        
        return Ok();
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
}