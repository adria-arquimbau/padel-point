namespace EventsManager.Shared.Responses;

public class MatchResponse
{
    public Guid Id { get; set; }
    public DateTime DateTime { get; set; }
    public string Location { get; set; }
    public bool IsPrivate { get; set; }
}
