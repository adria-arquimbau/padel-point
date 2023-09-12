using EventsManager.Shared.Enums;
using EventsManager.Shared.Responses;

namespace EventsManager.Server.Models;

public class Tournament
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required DateTime StartDate { get; set; }
    public required DateTime CreationDate { get; set; }
    public required MatchLocation Location { get; set; }
    public ICollection<Team> Teams { get; set; } = new List<Team>();
    public int MaxTeams { get; set; }
    public Player Creator { get; set; }
    public decimal Price { get; set; }
    public bool RegistrationOpen { get; set; }
    public bool ShowBrackets { get; set; }
    public bool Finished { get; set; }
    public ICollection<Match> RoundRobinMatches { get; set; } = new List<Match>();
    public int RoundRobinPhaseGroups { get; set; }
    public Uri? ImageUri { get; set; }
    public RoundRobinType RoundRobinType { get; set; }
    public CompetitionStyle CompetitionStyle { get; set; }
}
            