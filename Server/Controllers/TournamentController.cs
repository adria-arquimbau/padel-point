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
    [AllowAnonymous]
    public async Task<IActionResult> Create([FromBody] TournamentRequest request, CancellationToken cancellationToken)
    {
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
            MaxTeams = maxTeams
        };
    
        _context.Tournament.Add(tournament);
        
        await _context.SaveChangesAsync(cancellationToken);
           
        return Ok();
    }   
}