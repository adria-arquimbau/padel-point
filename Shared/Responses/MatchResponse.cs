using EventsManager.Shared.Dtos;

namespace EventsManager.Shared.Responses;

public class MatchResponse
{
    public Guid Id { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public string Location { get; set; }
    public bool IsPrivate { get; set; }
    public IEnumerable<PlayerDto> PlayersTeamOne { get; set; }
    public IEnumerable<PlayerDto> PlayersTeamTwo { get; set; }
}
        