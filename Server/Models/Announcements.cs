namespace EventsManager.Server.Models;

public class Announcements
{
    public Guid Id { get; set; }
    public Player Player { get; set; }
    public Guid PlayerId { get; set; }
    public bool DevelopmentAnnouncementReadIt { get; set; }
    public bool InitialLevelFormDone { get; set; }
}