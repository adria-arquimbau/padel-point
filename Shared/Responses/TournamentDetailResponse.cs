using EventsManager.Shared.Enums;

namespace EventsManager.Shared.Responses;

public class TournamentDetailResponse   
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required DateTime StartDate { get; set; }
    public required MatchLocation Location { get; set; }
    public int MaxTeams { get; set; }   
    public bool IsPlayerTheCreator { get; set; }
    public required Guid Id { get; set; }
    public List<CoupleResponse> Couples { get; set; } = new();
    public bool IsPlayerAlreadySignedIn { get; set; }
    public decimal Price { get; set; }
    public bool RegistrationsOpen { get; set; } 
    public bool ShowBrackets { get; set; }
    public List<RoundRobinMatch> RoundRobinPhase { get; set; } = new();
}