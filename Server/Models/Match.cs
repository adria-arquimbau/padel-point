namespace EventsManager.Server.Models;

public class Match
{   
    public Guid Id { get; set; }
    public DateTime CreationDate { get; set; }
    public ICollection<MatchPlayer> MatchPlayers { get; set; } = new List<MatchPlayer>();
    public ICollection<Set> Sets { get; set; } = new List<Set>();
    public bool IsPrivate { get; set; }
    public double PricePerHour { get; set; }
    public DateTime StartDateTime { get; set; }
    public double Duration { get; set; }
    public Guid CreatorId { get; set; } 
    public Player Creator { get; set; }
    public bool ScoreConfirmedTeamOne { get; set; }
    public bool ScoreConfirmedTeamTwo { get; set; }
    public ICollection<EloHistory> EloHistories { get; set; } = new List<EloHistory>();
}
