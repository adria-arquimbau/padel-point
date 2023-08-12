namespace EventsManager.Shared.Responses;

public class InvitedMatchesResponse
{
    public required Guid MatchId { get; set; }
    public required string CreatorNickname { get; set; }
    public required DateTime MatchDateTime { get; set; }
}