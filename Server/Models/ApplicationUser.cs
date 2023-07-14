using Microsoft.AspNetCore.Identity;

namespace EventsManager.Server.Models;

public class ApplicationUser : IdentityUser
{
    public virtual DateTime? LastLoginTime { get; set; }
    public virtual DateTime? RegistrationDate { get; set; }
    public virtual Player Player { get; set; }

    public void UpdateLastLoginTime()   
    {
        LastLoginTime = DateTime.UtcNow;
    }
        
    public void SetRegistrationDate()   
    {
        RegistrationDate = DateTime.UtcNow;
    }
}   
