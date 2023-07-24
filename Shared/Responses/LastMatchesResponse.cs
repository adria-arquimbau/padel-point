namespace EventsManager.Shared.Responses;

public class LastMatchesResponse
{
    public Guid Id { get; set; }
    public DateTime StartDateTime { get; set; }
    public double Duration { get; set; }
    public int AverageElo { get; set; }
    public int EloChange { get; set; }
}