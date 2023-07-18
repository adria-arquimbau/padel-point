namespace EventsManager.Shared.Dtos;

public class PlayerDto
{
    public Guid Id { get; set; }
    public string NickName { get; set; }
    public Uri? ImageUrl { get; set; }
    public decimal Elo { get; set; }
}
