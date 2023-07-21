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