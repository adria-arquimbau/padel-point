namespace EventsManager.Shared.Dtos;

public class UserDto
{
    public string Id { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public Uri? ImageUrl { get; set; }
    public bool EmailConfirmed { get; set; }    
    public bool RequestingUpdate { get; set; }
    public string Country { get; set; }
}