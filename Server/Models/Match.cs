namespace EventsManager.Server.Models;

public class Match
{   
    public Guid Id { get; set; }
    public DateTime CreationDate { get; set; }
    public ICollection<MatchPlayer> MatchPlayers { get; set; } = new List<MatchPlayer>();
    public ICollection<Set> Sets { get; set; }
    public string Location { get; set; }
    public bool IsPrivate { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public Guid CreatorId { get; set; }
    public Player Creator { get; set; }
    public bool ScoreConfirmedTeamOne { get; set; }
    public bool ScoreConfirmedTeamTwo { get; set; }
}
