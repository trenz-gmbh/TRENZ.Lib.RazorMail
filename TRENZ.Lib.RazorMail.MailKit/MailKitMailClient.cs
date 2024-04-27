using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

using MailKit.Net.Smtp;
using MailKit.Security;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using MimeKit;

using TRENZ.Lib.RazorMail.Extensions;
using TRENZ.Lib.RazorMail.Models;
using TRENZ.Lib.RazorMail.Services;

using RazorMailMessage = TRENZ.Lib.RazorMail.Models.MailMessage;

namespace TRENZ.Lib.RazorMail;

public class MailKitMailClient(IOptions<SmtpAccount> accountOptions, ILogger<MailKitMailClient> logger)
    : BaseSmtpMailClient(accountOptions)
{
    [MustDisposeResource]
    private async Task<SmtpClient> CreateClientAsync(CancellationToken cancellationToken)
    {
        var client = new SmtpClient();

        await client.ConnectAsync(Account.Host, Account.Port, SecureSocketOptions.Auto, cancellationToken);

        await client.AuthenticateAsync(Account.Login, Account.Password, cancellationToken);

        return client;
    }

    /// <inheritdoc />
    protected override async Task SendInternalAsync(RazorMailMessage message, CancellationToken cancellationToken)
    {
        var mimeMessage = message.ToMimeMessage();

        using var client = await CreateClientAsync(cancellationToken);

        var formatOptions = FormatOptions.Default.Clone();

        logger.LogInformation("Sending mail from {From} to {Recipients} (CC: {Cc}, BCC: {Bcc}) with subject {Subject}",
            mimeMessage.From, mimeMessage.To, mimeMessage.Cc, mimeMessage.Bcc, mimeMessage.Subject);

        await client.SendAsync(formatOptions, mimeMessage, cancellationToken);

        await client.DisconnectAsync(true, cancellationToken);
    }
}
