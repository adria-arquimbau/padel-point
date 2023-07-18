namespace EventsManager.Server.Models;

public class Set
{
    public Guid Id { get; set; }
    public Guid MatchId { get; set; }
    public Match Match { get; set; }
    public int SetNumber { get; set; }
    public int Team1Score { get; set; }
    public int Team2Score { get; set; }
}