using EventsManager.Shared.Dtos;

namespace EventsManager.Shared.Requests;

public class SetMatchScoreRequest
{
    public List<SetDto> Sets { get; set; }
}