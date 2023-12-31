﻿using EventsManager.Shared.Enums;

namespace EventsManager.Server.Models;

public class EloHistory
{
    public Guid Id { get; set; }
    public int OldElo { get; set; }
    public int NewElo { get; set; }
    public int EloChange { get; set; }  
    public DateTime ChangeDate { get; set; }
    public Guid? MatchId { get; set; }
    public Match? Match { get; set; }
    public Guid PlayerId { get; set; }  
    public Player Player { get; set; }
    public ChangeEloHistoryReason ChangeReason { get; set; }
}