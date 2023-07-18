using EventsManager.Shared.Dtos;

namespace EventsManager.Shared.Responses;

public class MatchResponse
{
    public Guid Id { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public string Location { get; set; }
    public bool IsPrivate { get; set; }
    public List<PlayerDto> PlayersTeamOne { get; set; }
    public List<PlayerDto> PlayersTeamTwo { get; set; }
    public decimal AverageElo { get; set; }
}
        