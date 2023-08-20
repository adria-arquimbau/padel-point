using System.ComponentModel.DataAnnotations;
using EventsManager.Shared.Enums;

namespace EventsManager.Server.Models;

public class Tournament
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required DateTime StartDate { get; set; }
    public required DateTime CreationDate { get; set; }
    public required MatchLocation Location { get; set; }
    public ICollection<Couple> Teams { get; set; } = new List<Couple>();
    public int MaxTeams { get; set; }
    public Player Creator { get; set; }
    public decimal Price { get; set; }
    public bool RegistrationOpen { get; set; }
    public bool ShowBrackets { get; set; }
    public bool RoundRobinEnabled { get; set; }
    public RoundRobinPhase? RoundRobinPhase { get; set; }
}
    
public class RoundRobinPhase    
{
    public Guid Id { get; set; }
    public ICollection<Round> Rounds { get; set; } = new List<Round>();
    public int MaxRounds { get; set; }
}

public class Round
{
    public Guid Id { get; set; }
    public int Number { get; set; } 
    public TournamentMatch Match { get; set; }
}   

public class TournamentMatch
{
    public Guid Id { get; set; }
    public Guid Team1Id { get; set; }
    public Couple Team1 { get; set; }
    public Guid Team2Id { get; set; }
    public Couple Team2 { get; set; }
    public List<Set> Score { get; set; }
}
