using System.Security.Claims;
using EventsManager.Server.Data;
using EventsManager.Server.Handlers.Commands.Tournaments.RobinPhase;
using EventsManager.Server.Models;
using EventsManager.Shared.Dtos;
using EventsManager.Shared.Enums;
using EventsManager.Shared.Requests;
using EventsManager.Shared.Responses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Team = EventsManager.Server.Models.Team;

namespace EventsManager.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class TournamentController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IMediator _mediator;

    public TournamentController(ApplicationDbContext context, IMediator mediator)
    {
        _context = context;
        _mediator = mediator;
    }
    
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var tournaments = await _context.Tournament
            .AsNoTracking()
            .Where(x => !x.Name.Contains("TEST"))
            .Select(x => new TournamentResponse
            {
                Id = x.Id,
                Name = x.Name,
                ImageUri = x.ImageUri,
                EloAverage = x.Teams.Count == 0 ? 0 : (int)Math.Round(x.Teams.Average(t => (t.Player1.Elo + t.Player2.Elo) / 2)),
                RegistrationsOpen = x.RegistrationOpen && x.Teams.Count < x.MaxTeams,
                Description = x.Description,
                Price = x.Price,
                StartDate = x.StartDate,
                Location = x.Location,
                MaxTeams = x.MaxTeams,
                TeamsCount = x.Teams.Count
            }).OrderByDescending(x => x.StartDate)
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
        
        if (request.RoundRobinPhaseGroups != 1 && request.RoundRobinPhaseGroups != 2)
        {
            return Conflict("The number of groups must be 1 or 2");
        }
        
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
            Price = request.Price,
            RoundRobinType = request.RoundRobinType,
            RoundRobinPhaseGroups = request.RoundRobinPhaseGroups
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
        tournament.ShowBrackets = request.ShowBrackets;
        tournament.RoundRobinType = request.RoundRobinType;
        tournament.RoundRobinPhaseGroups = request.RoundRobinPhaseGroups;
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
            .AsNoTracking()
            .Where(x => x.Id == tournamentId)
            .Select(x => new TournamentDetailResponse
            {   
                Id = x.Id,
                Name = x.Name,
                ImageUri = x.ImageUri,
                RoundRobinPhaseGroups = x.RoundRobinPhaseGroups,
                Description = x.Description,
                GeneratedRoundRobinPhase = x.RoundRobinMatches.Any(),
                RegistrationsOpen = x.RegistrationOpen && x.Teams.Count < x.MaxTeams,
                StartDate = x.StartDate,
                Location = x.Location,
                MaxTeams = x.MaxTeams,
                ShowBrackets = x.ShowBrackets,
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
    
    [HttpGet("{tournamentId:guid}/round-robin-phase")]
    [AllowAnonymous]
    public async Task<IActionResult> GetRoundRobinPhase([FromRoute] Guid tournamentId, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var matches = await _context.Match
            .AsNoTracking() 
            .Where(x => x.TournamentId == tournamentId && x.RobinPhaseGroup != null)
            .Select(rrm => new RoundRobinMatchResponse
            {
                Id = rrm.Id,
                IsFinished = rrm.ScoreConfirmedTeamOne && rrm.ScoreConfirmedTeamTwo,
                StartDateTime = rrm.StartDateTime,
                RequesterIsTheCreator = rrm.Creator.UserId == userId,
                Sets = rrm.Sets.Select(s => new SetDto
                {
                    SetNumber = s.SetNumber,
                    Team1Score = s.Team1Score,
                    Team2Score = s.Team2Score,
                }).ToList(),
                RoundRobinPhaseGroup = rrm.RobinPhaseGroup ?? 0,
                RoundRobinPhaseRound = rrm.RobinPhaseRound ?? 0,
                AverageElo = (int)Math.Round(rrm.MatchPlayers.Average(mp => mp.Player.Elo)),
                PlayersTeamOne = rrm.MatchPlayers.Where(mp => mp.Team == Shared.Enums.Team.Team1).Select(mp => new PlayerDto
                {
                    Id = mp.Player.Id,
                    NickName = mp.Player.NickName,
                    ImageUrl = mp.Player.ImageUrl,
                    Elo = mp.Player.Elo
                }).ToList(),
                PlayersTeamTwo = rrm.MatchPlayers.Where(mp => mp.Team == Shared.Enums.Team.Team2).Select(mp => new PlayerDto
                {
                    Id = mp.Player.Id,
                    NickName = mp.Player.NickName,
                    ImageUrl = mp.Player.ImageUrl,
                    Elo = mp.Player.Elo
                }).ToList(),
            }).ToListAsync(cancellationToken: cancellationToken);
           
        return Ok(matches);
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
    
    [HttpDelete("registration/{tournamentId:guid}")]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> RemoveRegistration([FromRoute] Guid tournamentId, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        var team = await _context.Team
            .Where(x => x.Tournament.Id == tournamentId && (x.Player1.UserId == userId || x.Player2.UserId == userId))
            .SingleAsync(cancellationToken: cancellationToken);
        
        _context.Team.Remove(team);
        
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
        _context.Team.Remove(team);
        
        await _context.SaveChangesAsync(cancellationToken);
        
        return Ok();
    }
    
    [HttpGet("{tournamentId:guid}/search-invite")]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> SearchToInvitePlayer([FromQuery] string term, [FromRoute] Guid tournamentId, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var lowerTerm = term.ToLower();

        var players = await _context.Player
            .AsNoTracking()
            .Where(p => p.NickName.ToLower().Contains(lowerTerm) && p.UserId != userId)
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
        
        var newTeam = new Team
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
    
    [HttpPost("{tournamentId:guid}/generate-round-robin-phase")]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> GenerateRoundRobinPhase([FromRoute] Guid tournamentId, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        try
        {
            await _mediator.Send(new GenerateRoundRobinPhaseCommandRequest(tournamentId, userId), cancellationToken);
        }
        catch (Exception e)
        {
            return Conflict(e.Message);
        }
        
        return Ok();
    }
    
    [HttpDelete("{tournamentId:guid}/round-robin-phase")]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> DeleteRoundRobinPhase([FromRoute] Guid tournamentId, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var tournament = await _context.Tournament
            .Where(x => x.Id == tournamentId)
            .Include(x => x.Creator)
            .Include(x => x.RoundRobinMatches)
                .ThenInclude(x => x.Sets)
            .SingleAsync(cancellationToken: cancellationToken);
        
        if (tournament.Creator.UserId != userId)
        {
            return Conflict("You are not the creator of this tournament");
        }

        if (tournament.RoundRobinMatches.Any(x => x.Sets.Any()))
        {
            return Conflict("You can't delete the round robin phase if there are matches with sets");
        }
        
        _context.Match.RemoveRange(tournament.RoundRobinMatches);
        await _context.SaveChangesAsync(cancellationToken);
        
        return Ok();
    }
    
    [HttpPost("{tournamentId:guid}/confirm-round-robin-phase-matches")]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> ConfirmRoundRobinPhaseMatches([FromRoute] Guid tournamentId, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var matches = await _context.Match
            .Where(x => x.TournamentId == tournamentId && x.RobinPhaseGroup != null)
            .Include(x => x.Creator)
            .Include(x => x.Sets)
            .ToListAsync(cancellationToken: cancellationToken);

        if (matches.Any(x => x.Creator.UserId != userId))
        {
            return Conflict("You are not the creator of this tournament");
        }

        if (matches.Any(x => !x.Sets.Any()))
        {
            return Conflict("You can't confirm the round robin phase matches if there are matches without sets");
        }

        foreach (var match in matches)
        {
           match.ScoreConfirmedTeamOne = true;
           match.ScoreConfirmedTeamTwo = true;
           match.Winner = CalculateMatchWinner(match.Sets.ToList());
        }
        
        await _context.SaveChangesAsync(cancellationToken);
        
        return Ok();
    }
        
    private static Shared.Enums.Team? CalculateMatchWinner(IReadOnlyCollection<Set> sets)
    {
        var team1Wins = sets.Count(set => set.Team1Score > set.Team2Score);
        var team2Wins = sets.Count(set => set.Team2Score > set.Team1Score);

        return team1Wins != team2Wins ? (team1Wins > team2Wins ? Shared.Enums.Team.Team1 : Shared.Enums.Team.Team2) : null;
    }
    
    [HttpGet("{tournamentId:guid}/finals")]
    [AllowAnonymous]
    public async Task<IActionResult> GetFinals([FromRoute] Guid tournamentId, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var roundRobinPhaseInProgress = await _context.Match
            .AnyAsync(x => x.TournamentId == tournamentId && 
                           x.RobinPhaseGroup != null && 
                           !x.ScoreConfirmedTeamOne && 
                           !x.ScoreConfirmedTeamTwo, 
                cancellationToken: cancellationToken);
        
        
        return Ok(new List<TournamentFinalsMatchResponse>());
    }
}
