namespace EventsManager.Server.Models;

public class Match
{   
    public Guid Id { get; set; }
    public DateTime CreationDate { get; set; }
    public ICollection<MatchPlayer> MatchPlayers { get; set; } = new List<MatchPlayer>();
    public ICollection<Set> Sets { get; set; } = new List<Set>();
    public string Location { get; set; }
    public bool IsPrivate { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public Guid CreatorId { get; set; }
    public Player Creator { get; set; }
    public bool ScoreConfirmedTeamOne { get; set; }
    public bool ScoreConfirmedTeamTwo { get; set; }
    public ICollection<EloHistory> EloHistories { get; set; } = new List<EloHistory>();
}
