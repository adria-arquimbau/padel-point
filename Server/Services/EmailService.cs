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
            
    public async Task<EmailSendResult?> Execute(string toEmail, string body, string subject)
    {
        var emailClient = new EmailClient(_emailSettings.ConnectionString);
        const string sender = "DoNotReply@arquimbau.dev";

        EmailSendResult? statusMonitor;
        try
        {
            var emailSendOperation = await emailClient.SendAsync(
                wait: WaitUntil.Started,  
                senderAddress: sender,  
                recipientAddress: toEmail,
                subject: subject,
                htmlContent: body);
            statusMonitor = emailSendOperation.Value;
            
            var operationId = emailSendOperation.Id;
        }
        catch (RequestFailedException ex)
        {
            throw new Exception($"Error sending email: {ex.Message}");
        }
        
        return statusMonitor;
    }
}
    
public class EmailOptions
{
    public required string ConnectionString { get; set; }
}

public interface IEmailService
{
    Task<EmailSendResult?> Execute(string toEmail, string body, string subject);
}