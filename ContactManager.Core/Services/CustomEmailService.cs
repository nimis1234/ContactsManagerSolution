using ContactManager.Core.Interfaces;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;


namespace ContactManager.Core.Services
{
    public class CustomEmailService : ICustomEmailService
    {
        private readonly string? _smtpServer;
        private readonly string? _userName;
        private string? _password;
        private readonly int _smtpPort;
        private readonly bool _enableSsl;

        public CustomEmailService(IConfiguration configuration, ILogger<CustomEmailService> logger)
        {
            // get email settings
            var emailSettings = configuration.GetSection("EmailSettings");
            if (emailSettings == null)
            {
                throw new ArgumentNullException("EmailSettings");
            }
            _smtpServer = emailSettings["SmtpServer"];
            _userName = emailSettings["UserName"];
            _password = emailSettings["Password"];
            _smtpPort = int.Parse(emailSettings["Port"]);
            _enableSsl = bool.Parse(emailSettings["EnableSSL"]);

        }
        public async Task SendEmailAsync(string email, string subject, string message)
        {
            //im using  mail kit pakage to send email
            // send email
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("Contact Manager App", _userName));
            emailMessage.To.Add(new MailboxAddress("", email));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart("plain") { Text = message };

            using var client = new MailKit.Net.Smtp.SmtpClient();

            // Use the appropriate SecureSocketOptions for Mailtrap
            // If your config EnableSSL is false and using port 2525, use SecureSocketOptions.None:
            await client.ConnectAsync(_smtpServer, _smtpPort, SecureSocketOptions.None);
            await client.AuthenticateAsync(_userName, _password);
            await client.SendAsync(emailMessage);
            await client.DisconnectAsync(true);
        }
    }
}
