using Microsoft.AspNetCore.Identity;

namespace TalentoPlus.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public string Document { get; set; } = string.Empty;
    public Guid? EmployeeId { get; set; }
}
