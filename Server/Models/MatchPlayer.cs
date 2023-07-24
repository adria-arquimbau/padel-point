using EventsManager.Shared.Enums;

namespace EventsManager.Server.Models;

public class MatchPlayer
{
    public Guid Id { get; set; }
    public Guid MatchId { get; set; }
    public Match Match { get; set; }
    public Guid PlayerId { get; set; }
    public Player Player { get; set; }
    public Team Team { get; set; }
}