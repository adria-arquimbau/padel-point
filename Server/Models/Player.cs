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
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public string? Country { get; set; }
    public DateTime CreationDate { get; set; }  
    public Announcements Announcements { get; set; } = new();
    public InitialLevelForm? InitialLevelForm { get; set; }
    public decimal TrustFactor { get; set; }
}   