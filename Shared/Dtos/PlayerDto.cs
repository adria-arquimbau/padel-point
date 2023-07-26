

namespace EventsManager.Shared.Dtos;

public class PlayerDto
{
    public Guid Id { get; set; }
    public string NickName { get; set; }
    public string? Country { get; set; }
    public Uri? ImageUrl { get; set; }
    public int EloBeforeFinish { get; set; }    
    public bool CanIDeleteIt { get; set; }
    public bool DevelopmentAnnouncementReadIt { get; set; }
    public int GainedElo { get; set; }
    public int? EloStateBeforeMatch { get; set; }
}