namespace EventsManager.Server.Models;

public class Notification
{
    public Guid Id { get; set; }
    public Player Player { get; set; }
    public DateTime CreationDate { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public bool IsRead { get; set; }
    public bool IsDeleted { get; set; }
}
