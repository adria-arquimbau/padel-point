using System.Security.Claims;
using EventsManager.Server.Data;
using EventsManager.Server.Handlers.Commands.Elo.InitialPlayerSkillCalibrationCommandHandler;
using EventsManager.Shared.Requests;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventsManager.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class AnnouncementController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMediator _mediator;

    public AnnouncementController(ApplicationDbContext dbContext, IMediator mediator)
    {
        _dbContext = dbContext;
        _mediator = mediator;
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
        
        await _mediator.Send(new InitialPlayerSkillCalibrationCommandRequest(request, userId), cancellationToken);
        
        return Ok();
    }
}
