namespace EventsManager.Server.Models;

public class Player
{
    public Guid Id { get; set; }
    public string NickName { get; set; }
    public decimal Elo { get; set; }
    public Uri? ImageUrl { get; set; }
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }
}

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
}   

public class MatchPlayer
{
    public Guid Id { get; set; }
    public Guid MatchId { get; set; }
    public Match Match { get; set; }
    public Guid PlayerId { get; set; }
    public Player Player { get; set; }
    public Team Team { get; set; }
}

public enum Team
{
    Team1,
    Team2
}

public class Set
{
    public Guid Id { get; set; }
    public Guid MatchId { get; set; }
    public Match Match { get; set; }
    public int SetNumber { get; set; }
    public int Team1Score { get; set; }
    public int Team2Score { get; set; }
}
