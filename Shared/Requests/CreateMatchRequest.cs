namespace EventsManager.Shared.Requests;

public class CreateMatchRequest
{
    public DateTime StartDate { get; set; }
    public double Duration { get; set; }
    public string Location { get; set; }
    public bool IsPrivate { get; set; }
    public double PricePerHour { get; set; }
}   