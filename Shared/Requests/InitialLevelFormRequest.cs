namespace EventsManager.Shared.Requests;

public class InitialLevelFormRequest
{
    public int OtherRacketSportsYearsPlaying { get; set; }
    public bool PlayedOtherRacketSportsBefore { get; set; }
    public OtherRacketSportsLevel OtherRacketSportsLevel { get; set; }
    public SelfAssessedPadelSkillLevel SelfAssessedPadelSkillLevel { get; set; }
    public int YearsPlayingPadel { get; set; }
}

public enum OtherRacketSportsLevel
{
    Amateur,
    SemiPro,
    Pro 
}   

public enum SelfAssessedPadelSkillLevel 
{
    Beginner,
    BeginnerIntermediate,
    Intermediate,
    IntermediateAdvanced,
    Advanced,
    SemiPro
} 