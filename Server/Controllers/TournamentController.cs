using System.Security.Claims;
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
                RegistrationsOpen = x.RegistrationOpen,
                Description = x.Description,
                Price = x.Price,
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
            RegistrationOpen = request.OpenRegistrations,
            Description = request.Description,
            StartDate = request.StartDate,
            CreationDate = DateTime.UtcNow,
            Location = request.Location,
            MaxTeams = maxTeams,
            Creator = player,
            Price = request.Price
        };
    
        _context.Tournament.Add(tournament);
        
        await _context.SaveChangesAsync(cancellationToken);
           
        return Ok(new CreateTournamentResponse
        {
            Id = tournament.Id
        });
    }
    
    [HttpPut("{tournamentId:guid}")]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> Edit([FromRoute] Guid tournamentId, [FromBody] TournamentRequest request, CancellationToken cancellationToken)
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
        
        tournament.Name = request.Name;
        tournament.RegistrationOpen = request.OpenRegistrations;
        tournament.Description = request.Description;
        tournament.StartDate = request.StartDate;
        tournament.Location = request.Location;
        tournament.MaxTeams = request.MaxTeams switch
        {
            MaxTeams.Eight => 8,
            MaxTeams.Sixteen => 16,
            _ => throw new ArgumentOutOfRangeException()
        };
        tournament.Price = request.Price;
        
        
        // var maxTeams = request.MaxTeams switch
        // {
        //     MaxTeams.Eight => 8,
        //     MaxTeams.Sixteen => 16,
        //     _ => throw new ArgumentOutOfRangeException()
        // };
        
        await _context.SaveChangesAsync(cancellationToken);
           
        return Ok();
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
                RegistrationsOpen = x.RegistrationOpen,
                StartDate = x.StartDate,
                Location = x.Location,
                MaxTeams = x.MaxTeams,
                Price = x.Price,
                IsPlayerTheCreator = userId != null && x.Creator.UserId == userId,
                IsPlayerAlreadySignedIn = userId != null && x.Teams.Any(t => t.Player1.UserId == userId || t.Player2.UserId == userId),
                Couples = x.Teams.Select(c => new CoupleResponse
                {
                    Id = c.Id,
                    AverageElo = (int)Math.Round(x.Teams.Average(t => (t.Player1.Elo + t.Player2.Elo) / 2)),
                    Player1 = new PlayerDetailResponse
                    {
                        Id = c.Player1.Id,
                        NickName = c.Player1.NickName,
                        Elo = c.Player1.Elo,
                        ImageUrl = c.Player1.ImageUrl,
                        IsConfirmed = c.Player1Confirmed,
                        Country = c.Player1.Country,
                        MatchesPlayed = c.Player1.EloHistories.Count(eh => eh.ChangeReason == ChangeEloHistoryReason.MatchPlayed),
                        LastEloGained = !c.Player1.EloHistories.Any() ? 0 : c.Player1.EloHistories.OrderByDescending(eh => eh.ChangeDate).First().EloChange,
                    },
                    Player2 = new PlayerDetailResponse
                    {
                        Id = c.Player2.Id,
                        NickName = c.Player2.NickName,
                        Elo = c.Player2.Elo,
                        ImageUrl = c.Player2.ImageUrl,
                        IsConfirmed = c.Player2Confirmed,
                        Country = c.Player2.Country,
                        MatchesPlayed = c.Player2.EloHistories.Count(eh => eh.ChangeReason == ChangeEloHistoryReason.MatchPlayed),
                        LastEloGained = !c.Player2.EloHistories.Any() ? 0 : c.Player2.EloHistories.OrderByDescending(eh => eh.ChangeDate).First().EloChange,
                    }
                }).ToList()
            }).SingleAsync(cancellationToken: cancellationToken);
           
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
    
    [HttpDelete("registration")]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> RemoveRegistration(CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        var team = await _context.Couple
            .Where(x => x.Player1.UserId == userId || x.Player2.UserId == userId)
            .SingleAsync(cancellationToken: cancellationToken);
        
        _context.Couple.Remove(team);
        
        await _context.SaveChangesAsync(cancellationToken);
        
        return Ok();
    }
    
    [HttpDelete("{tournamentId:guid}/couple/{coupleId:guid}")]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> RemoveCouple([FromRoute] Guid coupleId, [FromRoute] Guid tournamentId, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        var tournament = await _context.Tournament
            .Where(x => x.Id == tournamentId)
            .Include(x => x.Teams)
            .Include(x => x.Creator)
            .SingleAsync(cancellationToken: cancellationToken);

        if (tournament.Creator.UserId != userId)
        {
            return Conflict("You are not the creator of this tournament");
        }
        
        var team = tournament.Teams.Single(x => x.Id == coupleId);
        _context.Couple.Remove(team);
        
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
    
    [HttpPost("{tournamentId:guid}/sign-in")]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> SignIn([FromRoute] Guid tournamentId, [FromBody] TournamentSignInRequest request, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var tournament = await _context.Tournament
            .Where(x => x.Id == tournamentId)
            .Include(x => x.Teams)
                .ThenInclude(x => x.Player1)
            .Include(x => x.Teams)
                .ThenInclude(x => x.Player2)
            .SingleAsync(cancellationToken: cancellationToken);

        if (tournament.Teams.Any(x => x.Player1Id == request.CoupleId || x.Player2Id == request.CoupleId))
        {
            return Conflict("Your couple is already signed in this tournament");
        }
        
        if (tournament.Teams.Any(x => x.Player1.UserId == userId || x.Player2.UserId == userId))
        {
            return Conflict("You are already signed in this tournament");
        }

        if (tournament.MaxTeams == tournament.Teams.Count)
        {
            return Conflict("This tournament is full");
        }
       
        var player1 = await _context.Player
            .Where(x => x.UserId == userId)
            .SingleAsync(cancellationToken: cancellationToken);
        
        var player2 = await _context.Player
            .Where(x => x.Id == request.CoupleId)
            .SingleAsync(cancellationToken: cancellationToken);
        
        var newTeam = new Couple
        {
            Player1 = player1,
            Player1Confirmed = true,
            Player2 = player2,
            CreationDate = DateTime.UtcNow
        };
        
        tournament.Teams.Add(newTeam);
        
        await _context.SaveChangesAsync(cancellationToken);
        
        return Ok();
    }
}