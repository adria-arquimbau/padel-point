namespace EventsManager.Shared.Responses;

public class PlayerRankingResponse
{
    public required Guid Id { get; set; }
    public required string NickName { get; set; }    
    public Uri? ImageUrl { get; set; }
    public int Elo { get; set; }
}