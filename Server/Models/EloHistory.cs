namespace EventsManager.Server.Models;

public class EloHistory
{
    public Guid Id { get; set; }
    public int PreviousElo { get; set; }
    public int CurrentElo { get; set; }
    public int EloChange { get; set; }
    public DateTime ChangeDate { get; set; }
    public Guid MatchId { get; set; }
    public Match Match { get; set; }
    public Guid PlayerId { get; set; }
    public Player Player { get; set; }
}