using EventsManager.Shared.Dtos;

namespace EventsManager.Shared.Responses;

public class MatchResponse
{
    public Guid Id { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public string Location { get; set; }
    public bool IsPrivate { get; set; }
    public bool ScoreConfirmedTeamOne { get; set; }
    public bool ScoreConfirmedTeamTwo { get; set; }
    public List<PlayerDto> PlayersTeamOne { get; set; } 
    public List<PlayerDto> PlayersTeamTwo { get; set; }
    public decimal AverageElo { get; set; }
    public decimal AverageEloTeamOne { get; set; }
    public decimal AverageEloTeamTwo { get; set; }
    public bool RequesterIsTheCreator { get; set; }
    public bool IAmAlreadyRegistered { get; set; }
    public int PlayersCount { get; set; }
    public List<string> PlayersNames { get; set; }
    public List<SetDto>? Sets { get; set; }
}
                    