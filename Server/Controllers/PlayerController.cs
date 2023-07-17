using EventsManager.Server.Data;
using EventsManager.Shared.Dtos;
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
}