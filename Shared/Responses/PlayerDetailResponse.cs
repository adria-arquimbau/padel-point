namespace EventsManager.Shared.Responses;

public class PlayerDetailResponse
{
    public required string NickName { get; set; }
    public Uri? ImageUrl { get; set; }
    public int Elo { get; set; }
    public int MatchesPlayed { get; set; }
    public int? LastEloGained { get; set; }
    public int Rank { get; set; }
    public string? Country { get; set; }    
    public Guid Id { get; set; }
    public List<EloHistoryResponse> EloHistory { get; set; } = new List<EloHistoryResponse>();
}   
