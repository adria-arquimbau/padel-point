using System.Security.Claims;
using EventsManager.Server.Data;
using EventsManager.Server.Models;
using EventsManager.Shared;
using EventsManager.Shared.Dtos;
using EventsManager.Shared.Requests;
using EventsManager.Shared.Responses;
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

    public MatchController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
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
            StartDateTime = request.StartDateTime.ToUniversalTime(),
            EndDateTime = request.StartDateTime.ToUniversalTime(),
            MatchPlayers = new List<MatchPlayer>
            {
                new()
                {
                    Player = playerCreator,
                    Team = Team.Team1
                }
            },
            Location = request.Location,
            IsPrivate = request.IsPrivate
        };

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
            .SingleAsync(cancellationToken: cancellationToken);

        if (match.Creator.UserId != userId)
        {
            return Conflict("Only creator can edit the match");
        }

        match.StartDateTime = request.StartDateTime.ToUniversalTime();
        match.EndDateTime = request.EndDateTime.ToUniversalTime();
        match.Location = request.Location;
        match.IsPrivate = request.IsPrivate;

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
                StartDateTime = x.StartDateTime,
                EndDateTime = x.EndDateTime,
                Location = x.Location,
                IsPrivate = x.IsPrivate,
                ScoreConfirmedTeamOne = x.ScoreConfirmedTeamOne,
                ScoreConfirmedTeamTwo = x.ScoreConfirmedTeamTwo,
                PlayersTeamOne = x.MatchPlayers.Where(p => p.Team == Team.Team1)
                    .Select(p => new PlayerDto
                {
                    Id = p.Player.Id,
                    NickName = p.Player.NickName,
                    ImageUrl = p.Player.ImageUrl,
                    Elo = p.Player.Elo,
                    CanIDeleteIt = userId != null && p.Player.UserId == userId
                }).ToList(),
                PlayersTeamTwo = x.MatchPlayers.Where(p => p.Team == Team.Team2)
                    .Select(p => new PlayerDto
                {
                    Id = p.Player.Id,
                    NickName = p.Player.NickName,
                    ImageUrl = p.Player.ImageUrl,
                    Elo = p.Player.Elo,
                    CanIDeleteIt = userId != null && p.Player.UserId == userId
                }).ToList()
            })
            .SingleOrDefaultAsync(cancellationToken: cancellationToken);
        
        if (match == null)
        {
            return NotFound("Match not found");
        }
        
        match.AverageEloTeamOne = match.PlayersTeamOne.Any()
            ? Math.Round(match.PlayersTeamOne.Average(mp => mp.Elo), 2) : 0;

        match.AverageEloTeamTwo = match.PlayersTeamTwo.Any()
            ? Math.Round(match.PlayersTeamTwo.Average(mp => mp.Elo), 2) : 0;
        
        return Ok(match);
    }
    
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var matches = await _dbContext.Match
            .Include(x => x.MatchPlayers)
            .ThenInclude(x => x.Player)
            .ToListAsync(cancellationToken: cancellationToken);

        var response = matches.Select(x => new MatchResponse
        {
            Id = x.Id,
            StartDateTime = x.StartDateTime,
            EndDateTime = x.EndDateTime,
            Location = x.Location,
            PlayersCount = x.MatchPlayers.Count,
            PlayersNames = x.MatchPlayers.Select(p => p.Player.NickName).ToList(),
            AverageElo = x.MatchPlayers.Any() ? Math.Round(x.MatchPlayers.Average(mp => mp.Player.Elo), 2) : 0
        }).OrderByDescending(x => x.StartDateTime).ToList();
        
        return Ok(response);
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
            .SingleAsync(cancellationToken: cancellationToken);

        if (match.Creator.UserId != userId)
        {
            return Conflict("Only creator can set the score");
        }

        var score = request.Sets.Select(x => new Set
        {
            SetNumber = x.SetNumber,
            Team1Score = x.Team1Score,
            Team2Score = x.Team2Score
        }).ToList();

        match.Sets.AddRange(score);

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
            .SingleAsync(cancellationToken: cancellationToken);

        if (match.Creator.UserId != userId)
        {
            return Conflict("Only creator can remove the score");
        }
        
        match.Sets = new List<Set>();
        
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return Ok();
    }
}