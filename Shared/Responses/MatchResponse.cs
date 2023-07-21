using EventsManager.Shared.Dtos;

namespace EventsManager.Shared.Responses;

public class MatchResponse
{
    public Guid Id { get; set; }
    public DateTime StartDateTime { get; set; }
    public string Location { get; set; }
    public bool IsPrivate { get; set; }
    public bool ScoreConfirmedTeamOne { get; set; }
    public bool ScoreConfirmedTeamTwo { get; set; }
    public List<PlayerDto> PlayersTeamOne { get; set; } 
    public List<PlayerDto> PlayersTeamTwo { get; set; }
    public int AverageElo { get; set; }
    public int AverageEloTeamOne { get; set; }
    public int AverageEloTeamTwo { get; set; }
    public bool RequesterIsTheCreator { get; set; }
    public bool IAmAlreadyRegistered { get; set; }
    public int PlayersCount { get; set; }   
    public List<string> PlayersNames { get; set; }
    public List<SetDto>? Sets { get; set; }
    public Team? MyTeam { get; set; }
    public bool Finished { get; set; }  
    public double Duration { get; set; }
    public double PricePerHour { get; set; }
}
                            