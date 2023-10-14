namespace EventsManager.Shared.Responses;

public class AllDataResponse
{
    public double TotalHours { get; set; }
    public int Matches { get; set; }
    public int Sets { get; set; }
    public int PointsGained { get; set; }
    public int PointsLost { get; set; }
}