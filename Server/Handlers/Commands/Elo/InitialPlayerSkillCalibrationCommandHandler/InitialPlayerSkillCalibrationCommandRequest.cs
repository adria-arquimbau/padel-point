using EventsManager.Shared.Requests;
using MediatR;

namespace EventsManager.Server.Handlers.Commands.Elo.InitialPlayerSkillCalibrationCommandHandler;

public class InitialPlayerSkillCalibrationCommandRequest : IRequest
{
    public readonly InitialLevelFormRequest Request;
    public readonly string UserId;

    public InitialPlayerSkillCalibrationCommandRequest(InitialLevelFormRequest request, string userId)
    {
        Request = request;
        UserId = userId;
    }
}       
