using System.Security.Claims;
using EventsManager.Server.Data;
using EventsManager.Shared.Dtos;
using EventsManager.Shared.Enums;
using EventsManager.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventsManager.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class PlayerController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    public PlayerController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("search")]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> SearchPlayer([FromQuery] string term, CancellationToken cancellationToken)
    {
        var lowerTerm = term.ToLower();

        var players = await _dbContext.Player
            .Where(p => p.NickName.ToLower().Contains(lowerTerm))
            .Select(x => new PlayerDto
            {
                NickName = x.NickName,
                ImageUrl = x.ImageUrl
            })
            .ToListAsync(cancellationToken: cancellationToken);

        return Ok(players);
    }
    
    [HttpGet("{matchId:guid}/search-invite")]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> SearchToInvitePlayer([FromQuery] string term, [FromRoute] Guid matchId, CancellationToken cancellationToken)
    {
        var lowerTerm = term.ToLower();

        var players = await _dbContext.Player
            .Where(p => p.NickName.ToLower().Contains(lowerTerm) )
            .ToListAsync(cancellationToken: cancellationToken);

        var playersToDiscard = await _dbContext.MatchPlayer
            .Where(mp => mp.MatchId == matchId)
            .Select(mp => mp.Player)
            .ToListAsync(cancellationToken: cancellationToken);

        foreach (var player in playersToDiscard)
        {
            players.Remove(player);
        }

        var response = players.Select(x => new PlayerToInviteResponse
        {
            Id = x.Id,
            Elo = x.Elo,
            NickName = x.NickName,
            ImageUrl = x.ImageUrl
        }).ToList();
        
        return Ok(response);
    }
    
    [HttpGet]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var player = await _dbContext.Player
            .Where(x => x.UserId == userId)
            .Select(x => new PlayerDto
            {
                NickName = x.NickName,
                ImageUrl = x.ImageUrl,
                DevelopmentAnnouncementReadIt = x.Announcements.DevelopmentAnnouncementReadIt,
                InitialLevelFormDone = x.Announcements.InitialLevelFormDone
            })
            .SingleAsync(cancellationToken: cancellationToken);

        return Ok(player);
    }
    
    [HttpGet("{playerId:guid}/detail")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPlayerDetail([FromRoute] Guid playerId, CancellationToken cancellationToken)
    {
        var response = await _dbContext.Player
            .Where(x => x.Id == playerId)
            .Select(x => new PlayerDetailResponse
            {
                Id = x.Id,
                NickName = x.NickName,
                ImageUrl = x.ImageUrl,
                Elo = x.Elo,
                Country = x.Country,
                MatchesPlayed = x.EloHistories.Count(eh => eh.ChangeReason == ChangeEloHistoryReason.MatchPlayed),
                EloHistory = x.EloHistories
                    .OrderByDescending(eh => eh.ChangeDate)
                    .Take(5)
                    .Select(eh => new EloHistoryResponse
                    {
                        CurrentElo = eh.NewElo,
                        ChangeDate = eh.ChangeDate
                    })
                    .OrderBy(eh => eh.ChangeDate)
                    .ToList(),
                LastEloGained = !x.EloHistories.Any() ? 0 : x.EloHistories.OrderByDescending(eh => eh.ChangeDate).First().EloChange,
                Rank = 0
            })  
            .SingleAsync(cancellationToken: cancellationToken);

        var allPlayers = await _dbContext.EloHistories
            .Where(x => x.ChangeReason == ChangeEloHistoryReason.MatchPlayed)
            .Select(x => x.Player)
            .Distinct()
            .ToListAsync(cancellationToken: cancellationToken);

        if (response.MatchesPlayed > 0)
        {
            response.Rank = allPlayers.Count(p => p.Elo > response.Elo) + 1;
        }
    
        return Ok(response);
    }

    
    [HttpGet("my-detail")]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> GetMyPlayerDetail(CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var response = await _dbContext.Player
            .Where(x => x.UserId == userId)
            .Select(x => new PlayerDetailResponse
            {
                Id = x.Id,
                NickName = x.NickName,
                ImageUrl = x.ImageUrl,
                Elo = x.Elo,
                Country = x.Country,
                MatchesPlayed = x.EloHistories.Count(eh => eh.ChangeReason == ChangeEloHistoryReason.MatchPlayed),
                EloHistory = x.EloHistories
                    .OrderByDescending(eh => eh.ChangeDate)
                    .Take(5)
                    .Select(eh => new EloHistoryResponse
                    {
                        CurrentElo = eh.NewElo,
                        ChangeDate = eh.ChangeDate
                    })
                    .OrderBy(eh => eh.ChangeDate)
                    .ToList(),
                LastEloGained = !x.EloHistories.Any() ? 0 : x.EloHistories.OrderByDescending(eh => eh.ChangeDate).First().EloChange,
                Rank = 0
            })  
            .SingleAsync(cancellationToken: cancellationToken);

        var allPlayers = await _dbContext.EloHistories
            .Where(x => x.ChangeReason == ChangeEloHistoryReason.MatchPlayed)
            .Select(x => x.Player)
            .Distinct()
            .ToListAsync(cancellationToken: cancellationToken);

        if (response.MatchesPlayed > 0)
        {
            response.Rank = allPlayers.Count(p => p.Elo > response.Elo) + 1;
        }
    
        return Ok(response);
    }
    
    [HttpGet("ranking")]
    [AllowAnonymous]
    public async Task<IActionResult> GetRanking(CancellationToken cancellationToken)
    {
        var rankedPlayers = (await _dbContext.EloHistories
            .Where(x => x.ChangeReason == ChangeEloHistoryReason.MatchPlayed)
            .Select(x => x.Player)
            .Distinct()
            .OrderByDescending(x => x.Elo)
            .Select(x => new PlayerDetailResponse
            {
                NickName = x.NickName,
                ImageUrl = x.ImageUrl,
                Elo = x.Elo,
                MatchesPlayed = x.EloHistories.Count(eh => eh.ChangeReason == ChangeEloHistoryReason.MatchPlayed),
                LastEloGained = x.EloHistories
                    .Where(eh => eh.ChangeReason == ChangeEloHistoryReason.MatchPlayed)
                    .OrderByDescending(eh => eh.ChangeDate).First().EloChange,
                Rank = 0, // Rank is set to 0 for now
                Country = x.Country,
                Id = x.Id
            })
            .ToListAsync(cancellationToken: cancellationToken))
        .Select((player, index) => { player.Rank = index + 1; return player; }) // Assign ranks
        .ToList();
        
        return Ok(rankedPlayers);
    }
    
    [HttpPost("development-announcement-read-it")]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> DevelopmentAnnouncementReadIt(CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var player = await _dbContext.Player
            .Where(x => x.UserId == userId)
            .Include(x => x.Announcements)
            .SingleAsync(cancellationToken: cancellationToken);
        player.Announcements.DevelopmentAnnouncementReadIt = true;
        
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Ok();
    }
    
    [HttpGet("{playerId:guid}/last-matches")]
    [AllowAnonymous]
    public async Task<IActionResult> LastMatches([FromRoute] Guid playerId, CancellationToken cancellationToken)
    {
        var matches = await _dbContext.MatchPlayer
            .Where(x => x.PlayerId == playerId && (x.Match.ScoreConfirmedTeamTwo && x.Match.ScoreConfirmedTeamOne))
            .Select(x => x.Match)
            .OrderByDescending(x => x.StartDateTime)
            .Take(5)
            .Select(x => new LastMatchesResponse
            {
                Id = x.Id,
                StartDateTime = x.StartDateTime,
                Duration = x.Duration,
                PlayerWon = x.MatchPlayers.Single(mp => mp.PlayerId == playerId).Team == x.Winner,
                EloChange = x.EloHistories.Single(eh => eh.PlayerId == playerId).EloChange,
                AverageElo = (int)Math.Round(x.EloHistories.Average(eh => eh.OldElo)),
            })
            .ToListAsync(cancellationToken: cancellationToken);
        
        return Ok(matches);
    }
    
    [HttpDelete("delete-initial-player-calibration/{playerId:guid}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> DeleteInitialPlayerCalibration([FromRoute] Guid playerId, CancellationToken cancellationToken)
    {
        var player = await _dbContext.Player
            .Where(x => x.Id == playerId)
            .Where(x => x.EloHistories.Any(eh => eh.ChangeReason == ChangeEloHistoryReason.InitialSkillCalibration))
            .Include(x => x.EloHistories)
            .Include(x => x.InitialLevelForm)
            .Include(x => x.Announcements)
            .SingleAsync(cancellationToken: cancellationToken);
        
            var initialCalibrationHistory = player.EloHistories.Single(x => x.ChangeReason == ChangeEloHistoryReason.InitialSkillCalibration);
            
            player.Elo = initialCalibrationHistory.OldElo;
            player.InitialLevelForm = null;
            player.Announcements.InitialLevelFormDone = false;
            
            _dbContext.EloHistories.Remove(initialCalibrationHistory);
            
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return Ok();
    }
}