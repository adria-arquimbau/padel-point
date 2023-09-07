using EventsManager.Shared.Dtos;
using EventsManager.Shared.Enums;

namespace EventsManager.Shared.Responses;

public class MatchResponse
{
    public bool IsBlocked { get; set; }
    public Guid Id { get; set; }
    public DateTime StartDateTime { get; set; }
    public bool IsPrivate { get; set; }
    public bool ScoreConfirmedTeamOne { get; set; }
    public bool ScoreConfirmedTeamTwo { get; set; }
    public List<PlayerDto> PlayersTeamOne { get; set; } = new();
    public List<PlayerDto> PlayersTeamTwo { get; set; } = new();
    public int AverageElo { get; set; }
    public int AverageEloTeamOne { get; set; }
    public int AverageEloTeamTwo { get; set; }
    public bool RequesterIsTheCreator { get; set; }
    public bool IAmAlreadyRegistered { get; set; }
    public int PlayersCount { get; set; }   
    public List<SetDto>? Sets { get; set; }
    public Team? MyTeam { get; set; }
    public bool Finished => ScoreConfirmedTeamTwo && ScoreConfirmedTeamOne;
    public double Duration { get; set; }
    public double PricePerHour { get; set; }
    public Team? TeamWinner { get; set; }
    public bool RequesterIsAPlayer { get; set; }
    public int? RequesterElo { get; set; }   
    public string CreatorNickName { get; set; }
    public double ProbabilityTeamOneWins { get; set; }  
    public double ProbabilityTeamTwoWins { get; set; }  
    public List<PromotionResponse> Promotions { get; set; }
    public MatchLocation Location { get; set; }
    public int? MinimumLevel { get; set; }
    public bool IHaveOpenInvitation { get; set; }   
    public bool IsCompetitive { get; set; }
    public int? CourtNumber { get; set; }       
    public string? TournamentName { get; set; }
    public Guid? TournamentId { get; set; }
    public string TournamentPhase { get; set; } 
    public string TournamentRound { get; set; }
    public Uri? TournamentImageUri { get; set; }
}       