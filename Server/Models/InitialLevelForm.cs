using EventsManager.Shared.Requests;

namespace EventsManager.Server.Models;

public class InitialLevelForm   
{
    public Guid Id { get; set; }
    public Player Player { get; set; }
    public Guid PlayerId { get; set; }
    public int OtherRacketSportsYearsPlaying { get; set; }
    public bool PlayedOtherRacketSportsBefore { get; set; }
    public OtherRacketSportsLevel OtherRacketSportsLevel { get; set; }
    public SelfAssessedPadelSkillLevel SelfAssessedPadelSkillLevel { get; set; }
    public int YearsPlayingPadel { get; set; }
}
