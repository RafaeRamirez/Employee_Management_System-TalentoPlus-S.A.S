namespace TalentoPlus.Application.Contracts.Requests;

public class LoginRequest
{
    public string Document { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
