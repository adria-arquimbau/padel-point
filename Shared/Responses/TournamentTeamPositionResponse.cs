namespace EventsManager.Shared.Responses;

public class TournamentTeamPositionResponse
{
    public required int Position { get; set; }
    public required TeamResponse Team { get; set; }
}   