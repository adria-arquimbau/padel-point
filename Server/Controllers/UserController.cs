﻿/*using System.Security.Claims;
using EventsManager.Server.Data;
using EventsManager.Server.Handlers.Commands.User.ConfirmEmail;
using EventsManager.Server.Handlers.Commands.User.DeleteUser;
using EventsManager.Server.Handlers.Commands.User.DeleteUserImage;
using EventsManager.Server.Handlers.Commands.User.UpdateMyUser;
using EventsManager.Server.Handlers.Commands.User.UploadUserImage;
using EventsManager.Server.Handlers.Queries.Users.GetAll;
using EventsManager.Server.Handlers.Queries.Users.GetMyUser;
using EventsManager.Shared;
using EventsManager.Shared.Dtos;
using EventsManager.Shared.Requests;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventsManager.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ApplicationDbContext _dbContext;

    public UserController(IMediator mediator, ApplicationDbContext dbContext)
    {
        _mediator = mediator;
        _dbContext = dbContext;
    }   
    
    [HttpGet]
    [Authorize(Roles = "User")] 
    public async Task<IActionResult> GetMyUser()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var response = await _mediator.Send(new GetMyUserQueryRequest(userId));
        return Ok(response); 
    }
    
    [HttpPut]   
    [Authorize(Roles = "User")] 
    public async Task<IActionResult> UpdateUser([FromBody] UserDto request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        await _mediator.Send(new UpdateMyUserCommandRequest(userId, request));
        return Ok();
    }
    
    [HttpPost("image")]
    [Authorize(Roles = "User")]
    public async Task<ActionResult<UploadResult>> UploadUserImage(IFormFile file) 
    {   
        var response = await _mediator.Send(new UploadUserImageCommandRequest(file, User.FindFirst(ClaimTypes.NameIdentifier)?.Value));
        return Ok(response);    
    }
    
    [HttpDelete("image")]   
    [Authorize(Roles = "User")] 
    public async Task<IActionResult> DeleteUserImage()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        await _mediator.Send(new DeleteUserImageCommandRequest(userId));
        return Ok();
    }
    
    [HttpGet("all-users")]   
    [Authorize(Roles = "Administrator")] 
    public async Task<IActionResult> GetAllUsers()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var response = await _mediator.Send(new GetAllUsersQueryRequest(userId));
        return Ok(response);
    }

    [HttpPut("confirm-email")]   
    [Authorize(Roles = "Administrator")] 
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequest request)
    {
        await _mediator.Send(new ConfirmEmailCommandRequest(request.IdToConfirm));
        return Ok();
    }

    [HttpDelete("{userId}")]   
    [Authorize(Roles = "Administrator")] 
    public async Task<IActionResult> DeleteUser([FromRoute]string userId)
    {
        await _mediator.Send(new DeleteUserCommandRequest(userId));
        return Ok();
    }
}*/