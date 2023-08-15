using EventsManager.Shared.Dtos;

namespace EventsManager.Shared.Responses;

public class CoupleResponse
{
    public string Name { get; set; }
    public PlayerDto Player1 { get; set; }
    public PlayerDto Player2 { get; set; }
}