using System.Security.Claims;
using EventsManager.Server.Data;
using EventsManager.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventsManager.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class EloHistoryController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    public EloHistoryController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    [HttpGet]   
    [Authorize(Roles = "User")]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var eloHistory = await _dbContext.EloHistories
            .Where(x => x.Player.Id == Guid.Parse(userId))
            .OrderBy(x => x.ChangeDate)
            .Select(x => new EloHistoryResponse
            {
                Elo = x.CurrentElo,
                ChangeDate = x.ChangeDate
            })
            .ToListAsync(cancellationToken);
        
        
        return Ok(eloHistory);
    }
}