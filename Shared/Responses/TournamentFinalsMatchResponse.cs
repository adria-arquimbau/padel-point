using EventsManager.Shared.Dtos;

namespace EventsManager.Shared.Responses;

public class TournamentFinalsMatchResponse
{
    public Guid Id { get; set; }
    public List<PlayerDto> PlayersTeamOne { get; set; } = new();
    public List<PlayerDto> PlayersTeamTwo { get; set; } = new();
    public int AverageElo { get; set; }
    public DateTime StartDateTime { get; set; }
    public bool RequesterIsTheCreator { get; set; }
    public List<SetDto> Sets { get; set; } = new();
    public bool IsFinished { get; set; }
}