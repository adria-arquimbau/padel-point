using EventsManager.Server.Data;
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
            .ToListAsync(cancellationToken);
        
        return Ok(tournaments);
    }   
}
