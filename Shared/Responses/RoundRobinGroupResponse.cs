namespace EventsManager.Shared.Responses;

public class RoundRobinGroupResponse        
{
    public int GroupNumber { get; set; }
    public List<RoundRobinRoundResponse>? Rounds { get; set; }
    public int AverageElo { get; set; }
}

public class RoundRobinRoundResponse
{
    public int RoundNumber { get; set; }
    public List<RoundRobinMatchResponse>? Matches { get; set; }
}                   