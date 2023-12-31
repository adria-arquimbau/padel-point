namespace EventsManager.Server.Models;

public class MatchPlayer
{
    public Guid Id { get; set; }
    public Guid MatchId { get; set; }
    public Match Match { get; set; }
    public Guid PlayerId { get; set; }
    public Player Player { get; set; }
    public Shared.Enums.Team Team { get; set; }
    public bool Confirmed { get; set; }
}
