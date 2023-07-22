using System.Security.Claims;
using EventsManager.Server.Data;
using EventsManager.Shared.Dtos;
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
                DevelopmentAnnouncementReadIt = x.DevelopmentAnnouncementReadIt
            })
            .SingleAsync(cancellationToken: cancellationToken);

        return Ok(player);
    }
    
    [HttpGet("ranking")]
    [AllowAnonymous]
    public async Task<IActionResult> GetRanking(CancellationToken cancellationToken)
    {
        var players = await _dbContext.Player
            .Select(x => new PlayerRankingResponse
            {
                Id = x.Id,
                NickName = x.NickName,
                ImageUrl = x.ImageUrl,
                Elo = x.Elo
            })
            .OrderBy(x => x.Elo)
            .ToListAsync(cancellationToken: cancellationToken);
        
        return Ok(players);
    }
    
    [HttpPost("development-announcement-read-it")]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> DevelopmentAnnouncementReadIt(CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var player = await _dbContext.Player
            .Where(x => x.UserId == userId)
            .SingleAsync(cancellationToken: cancellationToken);
        player.DevelopmentAnnouncementReadIt = true;
        
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Ok();
    }
}