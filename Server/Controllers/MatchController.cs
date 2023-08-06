using System.Security.Claims;
using EventsManager.Server.Data;
using EventsManager.Server.Handlers.Commands.Elo;
using EventsManager.Server.Handlers.Commands.Elo.CalculateEloResultAfterMatch;
using EventsManager.Server.Models;
using EventsManager.Shared.Dtos;
using EventsManager.Shared.Enums;
using EventsManager.Shared.Requests;
using EventsManager.Shared.Responses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging;

namespace EventsManager.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class MatchController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMediator _mediator;

    public MatchController(ApplicationDbContext dbContext, IMediator mediator)
    {
        _dbContext = dbContext;
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> Create([FromBody] CreateMatchRequest request, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var playerCreator = await _dbContext.Player.Where(x => x.UserId == userId).SingleAsync(cancellationToken: cancellationToken);
    
        var newMatch = new Match
        {
            Creator = playerCreator,
            CreationDate = DateTime.UtcNow,
            StartDateTime = request.StartDate,
            Duration = request.Duration,
            PricePerHour = request.PricePerHour,
            MatchPlayers = new List<MatchPlayer>
            {
                new()
                {
                    Player = playerCreator,
                    Team = Team.Team1
                }
            },
            IsPrivate = request.IsPrivate,
        };
        
        var random = new Random();
        var randomNumber = random.Next(1, 21);
        
        if (randomNumber == 1)
        {
            newMatch.Promotions.Add(new Promotion
            {
                Title = "Akira Bar",
                Description = "After the match, players get x2 in drinks.",
            });
        }

        _dbContext.Match.Add(newMatch);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new CreateMatchResponse { Id = newMatch.Id });
    }

    
    [HttpPut("{matchId:guid}")]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> Edit([FromRoute] Guid matchId, [FromBody] CreateMatchRequest request, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var match = await _dbContext.Match
            .Where(x => x.Id == matchId)
            .Include(x => x.Creator)
            .Include(x => x.MatchPlayers)
            .ThenInclude(x => x.Player)
            .SingleAsync(cancellationToken: cancellationToken);

        if (match.Creator.UserId != userId)
        {
            return Conflict("Only creator can edit the match");
        }
        
        if (match is { ScoreConfirmedTeamOne: true, ScoreConfirmedTeamTwo: true })
        {
            return Conflict("You can't edit a match with confirmed score.");
        }

        match.StartDateTime = request.StartDate;
        match.Duration = request.Duration;
        match.IsPrivate = request.IsPrivate;

        foreach (var matchPlayer in match.MatchPlayers)
        {
            var notification = new Notification
            {
                CreationDate = DateTime.Now,
                Title = "Match edited",
                Description = "Match you've registered for has been edited.",
            };

            matchPlayer.Player.Notifications.Add(notification);
        }
        
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok();
    }
    
    [HttpDelete("{matchId:guid}")]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> Delete([FromRoute] Guid matchId, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var match = await _dbContext.Match
            .Where(x => x.Id == matchId)
            .Include(x => x.Creator)
            .SingleAsync(cancellationToken: cancellationToken);

        if (match.Creator.UserId != userId)
        {
            return Conflict("Only creator can delete the match.");
        }
        
        if (match is { ScoreConfirmedTeamOne: true, ScoreConfirmedTeamTwo: true })
        {
            return Conflict("You can't delete a match with confirmed score.");
        }

        _dbContext.Match.Remove(match);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok();
    }
    
    [HttpPost("{matchId:guid}/add-me/{team}")]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> AddMeAsAPlayer([FromRoute] Guid matchId, [FromRoute] Team team, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var player = await _dbContext.Player
            .Where(x => x.UserId == userId)
            .SingleAsync(cancellationToken: cancellationToken);
        
        var match = await _dbContext.Match
            .Where(x => x.Id == matchId)
            .Include(x => x.MatchPlayers)
            .ThenInclude(x => x.Player)
            .SingleAsync(cancellationToken: cancellationToken);

        if (match.MatchPlayers.Any(x => x.Player.Id == player.Id))
        {
            return Conflict("You're already registered for this match.");
        }

        var requestedTeamRegistered = match.MatchPlayers.Count(x => x.Team == team);
        if (requestedTeamRegistered >= 2)
        {
            return Conflict("This team is already full.");
        }
        
        match.MatchPlayers.Add(new MatchPlayer
        {
            Player = player,
            Team = team
        });
        
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok();
    }
    
    [HttpPost("{matchId:guid}/confirm-result/{confirmation:bool}")]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> ConfirmMyTeamResult([FromRoute] Guid matchId, [FromRoute] bool confirmation, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        var player = await _dbContext.Player
            .Where(x => x.UserId == userId)
            .SingleAsync(cancellationToken: cancellationToken);
        
        var match = await _dbContext.Match
            .Where(x => x.Id == matchId)
            .Include(x => x.MatchPlayers)
            .ThenInclude(x => x.Player)
            .Include(x => x.Sets)
            .SingleAsync(cancellationToken: cancellationToken);

        if (match is { ScoreConfirmedTeamOne: true, ScoreConfirmedTeamTwo: true })
        {
            return Conflict("Score is already confirmed.");
        }
        
        var matchPlayer = match.MatchPlayers.Single(x => x.PlayerId == player.Id && x.MatchId == matchId);
        
        var myTeam = matchPlayer.Team;

        if (myTeam == Team.Team1)
        {
            match.ScoreConfirmedTeamOne = confirmation;
        }
        if (myTeam == Team.Team2)
        {
            match.ScoreConfirmedTeamTwo = confirmation;
        }

        match.Winner = CalculateMatchWinner(match.Sets.ToList());
        
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        if (match is { ScoreConfirmedTeamOne: true, ScoreConfirmedTeamTwo: true })
        {
            await _mediator.Send(new CalculateEloResultAfterMatchCommandRequest(matchId), cancellationToken);
        }
        
        return Ok();
    }
    
    public Team? CalculateMatchWinner(List<Set> sets)
    {
        var team1Wins = sets.Count(set => set.Team1Score > set.Team2Score);
        var team2Wins = sets.Count(set => set.Team2Score > set.Team1Score);

        return team1Wins != team2Wins ? (team1Wins > team2Wins ? Team.Team1 : Team.Team2) : null;
    }

    [HttpGet("{matchId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> Get([FromRoute] Guid matchId, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        var match = await _dbContext.Match
            .Where(x => x.Id == matchId)
            .Select(x => new MatchResponse
            {
                Id = x.Id,
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
                }).ToList(),
                PlayersTeamOne = x.MatchPlayers.Where(p => p.Team == Team.Team1)
                    .Select(p => new PlayerDto
                {
                    Id = p.Player.Id,
                    NickName = p.Player.NickName,
                    Country = p.Player.Country,
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
            return NotFound("Match not found");
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
        
        return Ok(match);
    }
    
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var matches = await _dbContext.Match
            .Where(x => x.IsPrivate == false || x.ScoreConfirmedTeamOne && x.ScoreConfirmedTeamTwo)
            .Include(x => x.MatchPlayers)
            .ThenInclude(matchPlayer => matchPlayer.Player)
            .Include(x => x.EloHistories)
            .Include(match => match.Promotions)
            .ToListAsync(cancellationToken: cancellationToken);

        var matchResponses = matches.Select(x => new MatchResponse
        {
            Id = x.Id,
            StartDateTime = x.StartDateTime,
            ScoreConfirmedTeamOne = x.ScoreConfirmedTeamOne,
            ScoreConfirmedTeamTwo = x.ScoreConfirmedTeamTwo,
            Duration = x.Duration,
            PlayersCount = x.MatchPlayers.Count,
            PlayersNames = x.MatchPlayers.Select(p => p.Player.NickName).ToList(),
            AverageElo = x.MatchPlayers.Any() ? 
                (x.ScoreConfirmedTeamOne && x.ScoreConfirmedTeamTwo) ? (int)Math.Round(x.EloHistories.Average(eh => eh.OldElo)) : (int)Math.Round(x.MatchPlayers.Average(mp => mp.Player.Elo))
                : 0,
            Promotions = x.Promotions.Select(p => new PromotionResponse
            {
                Title = p.Title,
                Description = p.Description
            }).ToList()
        }).OrderByDescending(x => x.StartDateTime).ToList();
        
        return Ok(matchResponses);
    }
    
    [HttpDelete("{matchId:guid}/remove/{playerId:guid}")]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> DeletePlayer([FromRoute] Guid matchId, [FromRoute] Guid playerId, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        var player = await _dbContext.Player
            .Where(x => x.Id == playerId)
            .SingleAsync(cancellationToken: cancellationToken);
        var match = await _dbContext.Match
            .Where(x => x.Id == matchId)
            .Include(x => x.MatchPlayers)
            .ThenInclude(x => x.Player)
            .SingleAsync(cancellationToken: cancellationToken);
        
        if (userId != player.UserId && userId != match.Creator.UserId)
        {
            return Conflict("You can only remove yourself.");
        }

        if (match.ScoreConfirmedTeamOne || match.ScoreConfirmedTeamTwo)
        {
            return Conflict("You can't remove players from a match with confirmed score.");
        }
        
        var matchPlayer = match.MatchPlayers.Single(x => x.PlayerId == player.Id && x.MatchId == matchId);

        match.MatchPlayers.Remove(matchPlayer);
        
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok();
    }
    
    [HttpPost("{matchId:guid}/set-score")]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> SetScore([FromRoute] Guid matchId, [FromBody] SetMatchScoreRequest request, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var match = await _dbContext.Match
            .Where(x => x.Id == matchId)
            .Include(x => x.Creator)
            .Include(x => x.Sets)
            .Include(x => x.MatchPlayers)
            .SingleAsync(cancellationToken: cancellationToken);

        if (match.MatchPlayers.Count != 4)
        {
            return Conflict("Match is not full");
        }
        
        if (match is { ScoreConfirmedTeamOne: true, ScoreConfirmedTeamTwo: true })
        {
            return Conflict("Score is already confirmed");
        }

        var score = request.Sets.Select(x => new Set
        {
            SetNumber = x.SetNumber,
            Team1Score = x.Team1Score,
            Team2Score = x.Team2Score
        }).ToList();

        match.Sets = new List<Set>();
        match.Sets.AddRange(score);
        match.ScoreConfirmedTeamOne = false;
        match.ScoreConfirmedTeamTwo = false;

        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return Ok();
    }
    
    [HttpDelete("{matchId:guid}/remove-score")]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> RemoveScore([FromRoute] Guid matchId, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var match = await _dbContext.Match
            .Where(x => x.Id == matchId)
            .Include(x => x.Creator)
            .Include(x => x.Sets)
            .SingleAsync(cancellationToken: cancellationToken);

        if (match is { ScoreConfirmedTeamOne: true, ScoreConfirmedTeamTwo: true })
        {
            return Conflict("Score is already confirmed");
        }
        
        match.Sets = new List<Set>();
        match.ScoreConfirmedTeamOne = false;
        match.ScoreConfirmedTeamTwo = false;
        
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return Ok();
    }
    
    [HttpGet("my")]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> GetMyMatches(CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        var matches = await _dbContext.Match
            .Where(x => x.Creator.UserId == userId || x.MatchPlayers.Any(mp => mp.Player.UserId == userId))
            .Select(x => new MyMatchesResponse
            {
                Id = x.Id,
                StartDateTime = x.StartDateTime,
                Duration = x.Duration,
                AverageElo = x.EloHistories.Any() ? (int)Math.Round(x.EloHistories.Average(eh => eh.OldElo)) : 0,
                IsPrivate = x.IsPrivate,
                RequesterIsTheCreator = userId != null && x.Creator.UserId == userId,
                PlayersCount = x.MatchPlayers.Count,
                Finished = x.ScoreConfirmedTeamOne && x.ScoreConfirmedTeamTwo
            })
            .OrderByDescending(x => x.StartDateTime)
            .ToListAsync(cancellationToken);
        
        return Ok(matches);
    }
    
    [HttpGet("administration/all")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> GetAllAdministrator(CancellationToken cancellationToken)
    {
        var matches = await _dbContext.Match
            .Include(x => x.MatchPlayers)
            .ThenInclude(matchPlayer => matchPlayer.Player)
            .Include(x => x.EloHistories)
            .Include(match => match.Promotions)
            .Include(match => match.Creator)
            .ToListAsync(cancellationToken: cancellationToken);

        var matchResponses = matches.Select(x => new MatchAdministratorResponse
        {
            Id = x.Id,
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
        }).OrderByDescending(x => x.StartDateTime).ToList();    
        
        return Ok(matchResponses);
    }
    
    [HttpDelete("administration/{matchId:guid}")]
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
    
    [HttpPost("administration/simulate")]
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
