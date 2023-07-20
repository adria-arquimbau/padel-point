namespace EventsManager.Server.Models;

public class Player
{
    public Guid Id { get; set; }
    public string NickName { get; set; }
    public int Elo { get; set; }
    public Uri? ImageUrl { get; set; }
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }
    public ICollection<Match> CreatedMatches { get; set; } = new List<Match>();
    public ICollection<EloHistory> EloHistories { get; set; } = new List<EloHistory>();
}

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
