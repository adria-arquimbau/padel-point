using EventsManager.Shared.Enums;

namespace EventsManager.Shared.Requests;

public class TournamentRequest
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required DateTime StartDate { get; set; }
    public required MatchLocation Location { get; set; }
    public required MaxTeams MaxTeams { get; set; }
    public decimal Price { get; set; }
    public bool OpenRegistrations { get; set; }
    public bool ShowBrackets { get; set; }
    public int RoundRobinPhaseGroups { get; set; }
}   
    