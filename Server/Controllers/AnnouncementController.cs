/*
using System.Security.Claims;
using EventsManager.Server.Data;
using EventsManager.Server.Handlers.Commands.Elo.InitialPlayerSkillCalibrationCommandHandler;
using EventsManager.Shared.Exceptions;
using EventsManager.Shared.Requests;
using EventsManager.Shared.Responses;
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
        
        return Ok(new InitialLevelIsDoneResponse
        {
            Done = done
        });
    }
    
    [HttpGet("development-announcement-read-it")]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> DevelopmentAnnouncement(CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var done = await _dbContext.Player
            .Where(x => x.UserId == userId)
            .Select(x => x.Announcements.DevelopmentAnnouncementReadIt)
            .SingleAsync(cancellationToken: cancellationToken);
        
        return Ok(new AnnouncementDevelopmentResponse
        {
            Done = done
        });
    }
    
    [HttpPost("initial-level-done")]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> InitialLevelCalculate([FromBody] InitialLevelFormRequest request ,CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        try
        {
            await _mediator.Send(new InitialPlayerSkillCalibrationCommandRequest(request, userId), cancellationToken);
        }
        catch (InitialSkillCalibrationAlreadyDoneException)
        {
            return Conflict("Initial skill calibration already done");
        }
        catch (Exception)
        {
            return BadRequest("Something went wrong");
        }
        
        return Ok();
    }
}
*/
