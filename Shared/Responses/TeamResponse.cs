﻿namespace EventsManager.Shared.Responses;

public class TeamResponse
{
    public PlayerDetailResponse Player1 { get; set; }
    public PlayerDetailResponse Player2 { get; set; }
    public int AverageElo { get; set; }
    public required Guid Id { get; set; }
}