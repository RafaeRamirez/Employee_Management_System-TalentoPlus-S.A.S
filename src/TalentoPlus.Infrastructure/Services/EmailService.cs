using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using TalentoPlus.Application.Interfaces;

namespace TalentoPlus.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("TalentoPlus", _configuration["Smtp__From"] ?? "no-reply@talentoplus.com"));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;
        message.Body = new TextPart("plain") { Text = body };

        var host = _configuration["Smtp__Host"];
        var port = int.TryParse(_configuration["Smtp__Port"], out var parsedPort) ? parsedPort : 587;
        var user = _configuration["Smtp__User"];
        var pass = _configuration["Smtp__Pass"];

        using var client = new SmtpClient();
        await client.ConnectAsync(host, port, SecureSocketOptions.StartTls, cancellationToken);
        if (!string.IsNullOrWhiteSpace(user))
        {
            await client.AuthenticateAsync(user, pass, cancellationToken);
        }
        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }
}
