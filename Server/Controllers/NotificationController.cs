﻿using System.Security.Claims;
using EventsManager.Server.Data;
using EventsManager.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventsManager.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class NotificationController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    
    public NotificationController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }   
    
    [HttpGet]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var notifications = await _dbContext.Notifications
            .Where(n => n.Player.UserId == userId && !n.IsDeleted)
            .Select(x => new NotificationResponse
            {
                Id = x.Id,
                CreationDate = x.CreationDate,
                Title = x.Title,
                Description = x.Description,
                IsRead = x.IsRead
            })
            .OrderByDescending(x => x.CreationDate)
            .ToListAsync(cancellationToken);
        
        return Ok(notifications);
    }
    
    [HttpDelete("{notificationId:guid}")]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> Delete([FromRoute] Guid notificationId, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var notification = await _dbContext.Notifications
            .Where(n => n.Id == notificationId)
            .Include(x => x.Player)
            .SingleAsync(cancellationToken);

        if (notification.Player.UserId != userId)
        {
            return Conflict("You are not allowed to delete this notification");
        }
        
        notification.IsDeleted = true;
        
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return Ok();
    }
    
    [HttpPut("mark-as-read")]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> MarkAsRead(CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var notifications = await _dbContext.Notifications
            .Where(n => n.Player.UserId == userId)
            .ToListAsync(cancellationToken);

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
        }
        
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return Ok();
    }
}