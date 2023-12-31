using EventsManager.Shared.Enums;

namespace EventsManager.Server.Models;

public class Match
{   
    public Guid Id { get; set; }
    public DateTime CreationDate { get; set; }
    public ICollection<MatchPlayer> MatchPlayers { get; set; } = new List<MatchPlayer>();
    public ICollection<Set> Sets { get; set; } = new List<Set>();
    public ICollection<Promotion> Promotions { get; set; } = new List<Promotion>();
    public bool IsPrivate { get; set; }
    public double PricePerHour { get; set; }
    public DateTime StartDateTime { get; set; }
    public double Duration { get; set; }
    public Guid CreatorId { get; set; } 
    public Player Creator { get; set; }
    public bool ScoreConfirmedTeamOne { get; set; }
    public bool ScoreConfirmedTeamTwo { get; set; } 
    public Shared.Enums.Team? Winner { get; set; }
    public ICollection<EloHistory> EloHistories { get; set; } = new List<EloHistory>();
    public MatchLocation Location { get; set; }
    public bool IsBlocked { get; set; }
    public int? MinimumLevel { get; set; }
    public bool IsCompetitive { get; set; }
    public int? CourtNumber { get; set; }
    public Guid? TournamentId { get; set; } 
    public Tournament? Tournament { get; set; }
    public int? RobinPhaseGroup { get; set; }
    public int? RobinPhaseRound { get; set; }
}
            