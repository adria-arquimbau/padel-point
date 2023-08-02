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

        EmailSendResult? statusMonitor = null;
        try
        {
            Console.WriteLine("Sending email...");
            var emailSendOperation = await emailClient.SendAsync(
                wait: WaitUntil.Completed,  
                senderAddress: sender,  
                recipientAddress: toEmail,
                subject: subject,
                htmlContent: body);
            statusMonitor = emailSendOperation.Value;
    
            Console.WriteLine($"Email Sent. Status = {emailSendOperation.Value.Status}");

            /// Get the OperationId so that it can be used for tracking the message for troubleshooting
            var operationId = emailSendOperation.Id;
            Console.WriteLine($"Email operation id = {operationId}");
        }
        catch (RequestFailedException ex)
        {
            /// OperationID is contained in the exception message and can be used for troubleshooting purposes
            Console.WriteLine($"Email send operation failed with error code: {ex.ErrorCode}, message: {ex.Message}");
        }
        
        return statusMonitor;
    }
}
    
public class EmailOptions
{
    public string ConnectionString { get; set; }
}

public interface IEmailService
{
    Task<EmailSendResult?> Execute(string toEmail, string body, string subject);
}