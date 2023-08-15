namespace EventsManager.Shared.Requests;

public class TournamentSignInRequest
{
    public required string TeamName { get; set; }
    public required Guid CoupleId { get; set; }
}