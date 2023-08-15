using EventsManager.Shared.Dtos;

namespace EventsManager.Shared.Responses;

public class CoupleResponse
{
    public string Name { get; set; }
    public PlayerDetailResponse Player1 { get; set; }
    public PlayerDetailResponse Player2 { get; set; }
    public int AverageElo { get; set; }
}