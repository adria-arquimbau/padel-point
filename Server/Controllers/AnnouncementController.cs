using System.Security.Claims;
using EventsManager.Server.Data;
using EventsManager.Shared.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventsManager.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class AnnouncementController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    public AnnouncementController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    [HttpGet("initial-level-done")]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> IsInitialLevelDone(CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var done = await _dbContext.Player
            .Where(x => x.UserId == userId)
            .Select(x => x.Announcements.InitialLevelFormDone)
            .SingleAsync(cancellationToken: cancellationToken);
        
        return Ok(done);
    }
    
    [HttpPost("initial-level-done")]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> InitialLevelDone([FromBody] InitialLevelFormRequest request ,CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var announcements = await _dbContext.Announcements
            .Where(x => x.Player.UserId == userId)
            .SingleAsync(cancellationToken: cancellationToken);
        
        announcements.InitialLevelFormDone = true;  
        
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return Ok();
    }
}
