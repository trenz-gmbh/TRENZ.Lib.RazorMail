using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using TRENZ.Lib.RazorMail.Models;
using TRENZ.Lib.RazorMail.Services;
using TRENZ.Lib.RazorMail.SystemNet.Extensions;

using RazorMailMessage = TRENZ.Lib.RazorMail.Models.MailMessage;

namespace TRENZ.Lib.RazorMail.SystemNet;

public class SystemNetMailClient(IOptions<SmtpAccount> accountOptions, ILogger<SystemNetMailClient> logger) : BaseSmtpMailClient(accountOptions)
{
    [MustDisposeResource]
    private SmtpClient CreateClient()
    {
        var client = new SmtpClient(Account.Host);

        client.Port = Account.Port;
        client.EnableSsl = Account.TLS;
        client.Credentials = new NetworkCredential(Account.Login, Account.Password);

        return client;
    }

    /// <inheritdoc />
    protected override async Task SendInternalAsync(RazorMailMessage message, CancellationToken cancellationToken)
    {
        using var nativeMessage = message.ToSystemNetMailMessage();
        using var client = CreateClient();

        cancellationToken.ThrowIfCancellationRequested();

        logger.LogInformation("Sending mail from {From} to {Recipients} (CC: {Cc}, BCC: {Bcc}) with subject {Subject}",
            nativeMessage.From, nativeMessage.To, nativeMessage.CC, nativeMessage.Bcc, nativeMessage.Subject);

        await client.SendAsyncWithCancellation(nativeMessage, cancellationToken);
    }
}
