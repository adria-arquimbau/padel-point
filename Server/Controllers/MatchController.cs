using System.Security.Claims;
using System.Text;
using EventsManager.Server.Data;
using EventsManager.Server.Handlers.Commands.Elo.CalculateEloResultAfterMatch;
using EventsManager.Server.Handlers.Commands.Matches.ConfirmTeam;
using EventsManager.Server.Handlers.Queries.Matches.Get;
using EventsManager.Server.Models;
using EventsManager.Shared.Dtos;
using EventsManager.Shared.Enums;
using EventsManager.Shared.Exceptions;
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
            Location = request.Location,
            MinimumLevel = request.MinimumLevel,
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
                Title = "Akira Bar Promotion",
                Description = "Winner team gets 2x1 in drinks.",
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
        match.Location = request.Location;
        match.IsPrivate = request.IsPrivate;
        match.MinimumLevel = request.MinimumLevel;

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

        if (match.MinimumLevel > player.Elo)
        {
            return Conflict($"Your level is too low, only players with {match.MinimumLevel} or more can join this match.");
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

        try
        {
            await _mediator.Send(new ConfirmTeamCommandRequest(userId, matchId, confirmation), cancellationToken);
        }
        catch (ScoreIsAlreadyConfirmedException)
        {
            return Conflict("Score is already confirmed.");
        }
        catch (MatchIsBlockedException)
        {
            return Conflict("Match is blocked, you can't confirm score. Try again later.");
        }
        
        return Ok();
    }

    [HttpGet("{matchId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> Get([FromRoute] Guid matchId, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var match = await _mediator.Send(new GetMatchQueryRequest(userId, matchId), cancellationToken);
        
        return Ok(match);
    }
    
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var matches = await _dbContext.Match
            .Where(x => x.IsPrivate == false || x.ScoreConfirmedTeamOne && x.ScoreConfirmedTeamTwo)
            .Include(x => x.MatchPlayers)
            .ThenInclude(matchPlayer => matchPlayer.Player)
            .Include(x => x.EloHistories)
            .Include(match => match.Promotions)
            .ToListAsync(cancellationToken: cancellationToken);

        int? requesterElo = null;
        if (userId != null)
        {
            requesterElo = await _dbContext.Player
                .Where(x => x.UserId == userId)
                .Select(x => x.Elo)
                .SingleOrDefaultAsync(cancellationToken: cancellationToken);
        }
        
        var matchResponses = matches.Select(x => new MatchResponse
        {
            Id = x.Id,
            Location = x.Location,
            MinimumLevel = x.MinimumLevel,
            RequesterElo = userId != null ? requesterElo : null,
            StartDateTime = x.StartDateTime,
            ScoreConfirmedTeamOne = x.ScoreConfirmedTeamOne,
            ScoreConfirmedTeamTwo = x.ScoreConfirmedTeamTwo,
            Duration = x.Duration,
            PlayersTeamOne = x.MatchPlayers.Where(mp => mp.Team == Team.Team1).Select(mp => new PlayerDto
            {
                NickName = mp.Player.NickName
            }).ToList(),
            PlayersTeamTwo = x.MatchPlayers.Where(mp => mp.Team == Team.Team2).Select(mp => new PlayerDto
            {
                NickName = mp.Player.NickName
            }).ToList(),
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
}
