namespace EventsManager.Shared.Responses;

public class PlayerToInviteResponse 
{
    public required Guid Id { get; set; }    
    public required int Elo { get; set; }
    public required string NickName { get; set; }
    public required Uri? ImageUrl { get; set; }
}