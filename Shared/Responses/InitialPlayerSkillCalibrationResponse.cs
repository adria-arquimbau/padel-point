using EventsManager.Shared.Requests;

namespace EventsManager.Shared.Responses;

public class InitialPlayerSkillCalibrationResponse
{
    public int OtherRacketSportsYearsPlaying { get; set; }
    public bool PlayedOtherRacketSportsBefore { get; set; }
    public OtherRacketSportsLevel OtherRacketSportsLevel { get; set; }
    public SelfAssessedPadelSkillLevel SelfAssessedPadelSkillLevel { get; set; }
    public int YearsPlayingPadel { get; set; }
}