using System.Security.Claims;
using EventsManager.Server.Data;
using EventsManager.Server.Models;
using EventsManager.Shared.Requests;
using EventsManager.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
            CreationDate = request.Date.ToUniversalTime(),
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

    [HttpGet("{matchId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> Get([FromRoute] Guid matchId, CancellationToken cancellationToken)
    {
        var match = await _dbContext.Match
            .Where(x => x.Id == matchId)
            .Select(x => new MatchResponse
            {
                Id = x.Id,
                CreationDate = x.CreationDate,
                Location = x.Location,
                IsPrivate = x.IsPrivate
            })
            .SingleOrDefaultAsync(cancellationToken: cancellationToken);
        
        if (match == null)
        {
            return NotFound();
        }
        
        return Ok(match);
    }
}