using Azure.Storage.Blobs;
using EventsManager.Server.Data;
using EventsManager.Server.Settings;
using EventsManager.Shared.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EventsManager.Server.Handlers.Commands.User.DeleteUserImage;

public class DeleteUserImageCommandHandler : IRequestHandler<DeleteUserImageCommandRequest>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly BlobStorageSettings _blobStorageOptions;

    public DeleteUserImageCommandHandler(ApplicationDbContext dbContext, IOptions<BlobStorageSettings> blobStorageOptions)
    {
        _dbContext = dbContext;
        _blobStorageOptions = blobStorageOptions.Value;
    }
    
    public async Task Handle(DeleteUserImageCommandRequest request, CancellationToken cancellationToken)
    {
        var player = await _dbContext.Player
            .SingleAsync(x => x.User.Id == request.UserId, cancellationToken: cancellationToken);

        if (player.ImageUrl == null)
        {
            throw new UserDoesNotHavePictureException();
        }
        
        var blobClient = new BlobClient(_blobStorageOptions.ConnectionString, _blobStorageOptions.ProfileImageContainerName, player.Id + "-user-picture");
        
        await blobClient.DeleteAsync(cancellationToken: cancellationToken);
            
        player.ImageUrl = null;
        
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}