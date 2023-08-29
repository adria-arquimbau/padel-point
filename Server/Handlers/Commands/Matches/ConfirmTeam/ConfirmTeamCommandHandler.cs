using EventsManager.Server.Data;
using EventsManager.Server.Handlers.Commands.Elo.CalculateEloResultAfterMatch;
using EventsManager.Server.Models;
using EventsManager.Shared.Enums;
using EventsManager.Shared.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Team = EventsManager.Shared.Enums.Team;

namespace EventsManager.Server.Handlers.Commands.Matches.ConfirmTeam;

public class ConfirmTeamCommandHandler : IRequestHandler<ConfirmTeamCommandRequest>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMediator _mediator;

    public ConfirmTeamCommandHandler(ApplicationDbContext context, IMediator mediator)
    {
        _dbContext = context;
        _mediator = mediator;
    }   
    
    public async Task Handle(ConfirmTeamCommandRequest request, CancellationToken cancellationToken)
    {
        var userId = request.UserId;
        var matchId = request.MatchId;
        var confirmation = request.Confirmation;
        var adminRequest = request.AdminRequest;
        
        Player? player = null;
        if (!adminRequest)
        {   
            player = await _dbContext.Player
                .Where(x => x.UserId == userId)
                .SingleAsync(cancellationToken: cancellationToken);
        }
        
        var match = await _dbContext.Match
            .Where(x => x.Id == matchId)
            .Include(x => x.MatchPlayers)
            .ThenInclude(x => x.Player)
            .Include(x => x.Sets)
            .SingleAsync(cancellationToken: cancellationToken);

        if (match is { ScoreConfirmedTeamOne: true, ScoreConfirmedTeamTwo: true } && !adminRequest)
        {
            throw new ScoreIsAlreadyConfirmedException();
        }

        if (match.IsBlocked && !adminRequest)
        {
            throw new MatchIsBlockedException();    
        }

        if (match.MatchPlayers.Any(x => x.Confirmed == false) && !adminRequest)
        {
            throw new NotAllPlayersConfirmedException();
        }
        
        if (match.MatchPlayers.Any(x => x.Confirmed == false))
        {
            throw new OneOrMorePlayersAreNotConfirmedInTheMatchException();
        }

        if (!adminRequest)
        {
            var matchPlayer = match.MatchPlayers.Single(x => x.PlayerId == player.Id && x.MatchId == matchId);
            var myTeam = matchPlayer.Team;

            if (myTeam == Team.Team1)
            {
                match.ScoreConfirmedTeamOne = confirmation;
            }
            if (myTeam == Team.Team2)
            {
                match.ScoreConfirmedTeamTwo = confirmation;
            }
        }
        if (adminRequest)
        {
            match.ScoreConfirmedTeamOne = confirmation;
            match.ScoreConfirmedTeamTwo = confirmation;
        }
        
        match.Winner = CalculateMatchWinner(match.Sets.ToList());
        
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        if (match is { ScoreConfirmedTeamOne: true, ScoreConfirmedTeamTwo: true })
        {
            await _mediator.Send(new CalculateEloResultAfterMatchCommandRequest(matchId), cancellationToken);
        }
    }

    private static Team? CalculateMatchWinner(IReadOnlyCollection<Set> sets)
    {
        var team1Wins = sets.Count(set => set.Team1Score > set.Team2Score);
        var team2Wins = sets.Count(set => set.Team2Score > set.Team1Score);

        return team1Wins != team2Wins ? (team1Wins > team2Wins ? Team.Team1 : Team.Team2) : null;
    }
}