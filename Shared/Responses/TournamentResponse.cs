using EventsManager.Shared.Enums;

namespace EventsManager.Shared.Responses;

public class TournamentResponse
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required DateTime StartDate { get; set; }
    public required string Description { get; set; }
    public required MatchLocation Location { get; set; }
    public int MaxTeams { get; set; }
    public int TeamsCount { get; set; }
    public decimal Price { get; set; }
}