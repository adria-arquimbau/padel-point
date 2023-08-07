using Azure;
using Azure.Communication.Email;
using Microsoft.Extensions.Options;

namespace EventsManager.Server.Services;

public class EmailService : IEmailService
{
    private readonly EmailOptions _emailSettings;

    public EmailService(IOptions<EmailOptions> emailSettings)
    {
        _emailSettings = emailSettings.Value; 
    }   
            
    public async Task Execute(string recipient, string subject, string body)
    {
        var emailClient = new EmailClient(_emailSettings.ConnectionString);
        
        try
        {
            var emailContent = new EmailContent(subject)
            {
                Html = body
            };
            var emailMessage = new EmailMessage(_emailSettings.EmailFrom, recipient, emailContent);
            var emailSendOperation = await emailClient.SendAsync(
                wait: WaitUntil.Completed,
                message: emailMessage);
            
            var operationId = emailSendOperation.Id;
        }
        catch (RequestFailedException ex)
        {
            throw new Exception($"Error sending email: {ex.Message}");
        }
    }
}
    
public class EmailOptions
{
    public required string ConnectionString { get; set; }
    public required string EmailFrom { get; set; }
}

public interface IEmailService
{
    Task Execute(string recipient, string subject, string body);
}