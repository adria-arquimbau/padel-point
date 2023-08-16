namespace EventsManager.Shared.Responses;

public class InvitedTournamentResponse
{
    public required Guid TournamentId { get; set; }
    public required string CoupleName { get; set; }  
}