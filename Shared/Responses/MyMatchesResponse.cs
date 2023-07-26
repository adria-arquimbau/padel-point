namespace EventsManager.Shared.Responses;

public class MyMatchesResponse
{
    public required Guid Id { get; set; }
    public required DateTime StartDateTime { get; set; }
    public required double Duration { get; set; }
    public required int AverageElo { get; set; }
    public required bool IsPrivate { get; set; }
    public required bool RequesterIsTheCreator { get; set; }
    public required int PlayersCount { get; set; }
    public required bool Finished { get; set; }
}           