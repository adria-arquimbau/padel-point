using EventsManager.Shared.Enums;

namespace EventsManager.Shared.Requests;

public class CreateMatchRequest
{
    public DateTime StartDate { get; set; }
    public double Duration { get; set; }
    public bool IsPrivate { get; set; }
    public double PricePerHour { get; set; }    
    public MatchLocation Location { get; set; }
    public int? MinimumLevel { get; set; }
}       
