namespace EventsManager.Server.Models;

public class Couple
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required Player Player1 { get; set; }
    public Guid Player1Id { get; set; }
    public required Player Player2 { get; set; }
    public Guid Player2Id { get; set; }
    public required DateTime CreationDate { get; set; }
}
