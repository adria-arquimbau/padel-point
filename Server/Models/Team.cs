namespace EventsManager.Server.Models;

public class Team
{
    public Guid Id { get; set; }
    public required Player Player1 { get; set; }
    public Guid Player1Id { get; set; }
    public bool Player1Confirmed { get; set; }
    public required Player Player2 { get; set; }
    public Guid Player2Id { get; set; }
    public bool Player2Confirmed { get; set; }
    public required DateTime CreationDate { get; set; }
    public Tournament Tournament { get; set; }
}
