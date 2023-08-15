using System.Security.Claims;
using EventsManager.Client.Pages.Tournament;
using EventsManager.Server.Data;
using EventsManager.Server.Models;
using EventsManager.Shared.Enums;
using EventsManager.Shared.Requests;
using EventsManager.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventsManager.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class TournamentController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public TournamentController(ApplicationDbContext context)
    {
        _context = context;
    }
    
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var tournaments = await _context.Tournament
            .Select(x => new TournamentResponse
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                StartDate = x.StartDate,
                Location = x.Location,
                MaxTeams = x.MaxTeams,
                TeamsCount = x.Teams.Count
            })
            .ToListAsync(cancellationToken);
        
        return Ok(tournaments);
    }   
    
    [HttpPost]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> Create([FromBody] TournamentRequest request, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        var player = await _context.Player
            .Where(x => x.UserId == userId)
            .SingleAsync(cancellationToken: cancellationToken);
        
        var maxTeams = request.MaxTeams switch
        {
            MaxTeams.Eight => 8,
            MaxTeams.Sixteen => 16,
            _ => throw new ArgumentOutOfRangeException()
        };

        var tournament = new Tournament
        {
            Name = request.Name,
            Description = request.Description,
            StartDate = request.StartDate,
            CreationDate = DateTime.UtcNow,
            Location = request.Location,
            MaxTeams = maxTeams,
            Creator = player
        };
    
        _context.Tournament.Add(tournament);
        
        await _context.SaveChangesAsync(cancellationToken);
           
        return Ok(new CreateTournamentResponse
        {
            Id = tournament.Id
        });
    }
    
    [HttpGet("{tournamentId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> Get([FromRoute] Guid tournamentId, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var tournament = await _context.Tournament
            .Where(x => x.Id == tournamentId)
            .Select(x => new TournamentDetailResponse
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                StartDate = x.StartDate,
                Location = x.Location,
                MaxTeams = x.MaxTeams,
                IsPlayerTheCreator = userId != null && x.Creator.UserId == userId
            })
            .SingleAsync(cancellationToken: cancellationToken);
           
        return Ok(tournament);
    }   
    
    [HttpDelete("{tournamentId:guid}")]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> Delete([FromRoute] Guid tournamentId, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        var tournament = await _context.Tournament
            .Where(x => x.Id == tournamentId)
            .Include(x => x.Creator)
            .SingleAsync(cancellationToken: cancellationToken);

        if (tournament.Creator.UserId != userId)
        {
            return Conflict("You are not the creator of this tournament");
        }
        
        _context.Tournament.Remove(tournament);
        
        await _context.SaveChangesAsync(cancellationToken);
        
        return Ok();
    }   
    
    [HttpGet("{tournamentId:guid}/search-invite")]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> SearchToInvitePlayer([FromQuery] string term, [FromRoute] Guid tournamentId, CancellationToken cancellationToken)
    {
        var lowerTerm = term.ToLower();

        var players = await _context.Player
            .Where(p => p.NickName.ToLower().Contains(lowerTerm) )
            .ToListAsync(cancellationToken: cancellationToken);

        var response = players.Select(x => new PlayerToInviteResponse
        {
            Id = x.Id,
            Elo = x.Elo,
            NickName = x.NickName,
            ImageUrl = x.ImageUrl
        }).ToList();
        
        return Ok(response);
    }
}