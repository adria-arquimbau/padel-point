namespace EventsManager.Shared.Requests;

public class CreateMatchRequest
{
    public DateTime Date { get; set; }
    public string Location { get; set; }
    public bool IsPrivate { get; set; }
}