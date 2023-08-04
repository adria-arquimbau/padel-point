namespace EventsManager.Shared.Responses;

public class NotificationResponse
{
    public Guid Id { get; set; }
    public DateTime CreationDate { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public bool IsRead { get; set; }
}