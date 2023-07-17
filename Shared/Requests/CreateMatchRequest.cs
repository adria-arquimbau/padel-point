namespace EventsManager.Shared.Requests;

public class CreateMatchRequest
{
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public string Location { get; set; }
    public bool IsPrivate { get; set; }
}