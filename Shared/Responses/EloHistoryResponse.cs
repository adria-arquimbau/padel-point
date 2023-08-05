using EventsManager.Shared.Enums;

namespace EventsManager.Shared.Responses;

public class EloHistoryResponse
{
    public int CurrentElo { get; set; }
    public DateTime ChangeDate { get; set; }
    public ChangeEloHistoryReason ChangeReason { get; set; }
    public int EloChange { get; set; }
    public int PreviousElo { get; set; }
}   