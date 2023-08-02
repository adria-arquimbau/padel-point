using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace EventsManager.Server.Services;

public class EmailService : IEmailService
{
    private readonly EmailOptions _emailSettings;

    public EmailService(IOptions<EmailOptions> emailSettings)
    {
        _emailSettings = emailSettings.Value; 
    }   
            
    public async Task<Response> Execute(string toEmail, string toUserName, string body, string subject)
    {
        var client = new SendGridClient(_emailSettings.ApiKey); 
        var from = new EmailAddress("adria.arquimbau@gmail.com", "Padel Point");
        var receiver = new EmailAddress(toEmail, toUserName);
        var message = MailHelper.CreateSingleEmail(from, receiver, subject, body, body);
        var response = await client.SendEmailAsync(message);
        return response;
    }
}
    
public class EmailOptions
{
    public required string ApiKey { get; set; }
}

public interface IEmailService
{
    Task<Response> Execute(string toEmail, string toUserName, string body, string subject);
}