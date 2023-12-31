﻿using EventsManager.Shared.Enums;

namespace EventsManager.Shared.Responses;

public class TournamentDetailResponse   
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required DateTime StartDate { get; set; }
    public required MatchLocation Location { get; set; }
    public int MaxTeams { get; set; }   
    public bool IsPlayerTheCreator { get; set; }
    public required Guid Id { get; set; }
    public List<TeamResponse> Couples { get; set; } = new();
    public bool IsPlayerAlreadySignedIn { get; set; }   
    public decimal Price { get; set; }
    public bool RegistrationsOpen { get; set; } 
    public bool ShowBrackets { get; set; }
    public bool GeneratedRoundRobinPhase { get; set; }
    public int RoundRobinPhaseGroups { get; set; }
    public Uri? ImageUri { get; set; }      
    public RoundRobinType RoundRobinType { get; set; }
    public CompetitionStyle CompetitionStyle { get; set; }
}           