using System.Linq;
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
using MailKitMailMessage = MimeKit.MimeMessage;
using IMailKitMailMessage = MimeKit.IMimeMessage;

namespace TRENZ.Lib.RazorMail;

public class MailKitMailClient(IOptions<SmtpAccount> accountOptions, ILogger<MailKitMailClient> logger) : BaseMailClient(accountOptions)
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
        var nativeMessage = ConvertToNative(message);

        using var client = await CreateClientAsync(cancellationToken);

        var formatOptions = FormatOptions.Default.Clone();

        logger.LogInformation("Sending mail from {From} to {Recipients} (CC: {Cc}, BCC: {Bcc}) with subject {Subject}",
            nativeMessage.From, nativeMessage.To, nativeMessage.Cc, nativeMessage.Bcc, nativeMessage.Subject);

        await client.SendAsync(formatOptions, nativeMessage, cancellationToken);

        await client.DisconnectAsync(true, cancellationToken);
    }

    private static MailKitMailMessage ConvertToNative(RazorMailMessage razorMessage)
    {
        var mailKitMessage = new MailKitMailMessage();

        SetMailHeaders(razorMessage, mailKitMessage);
        SetMailContent(razorMessage, mailKitMessage);

        return mailKitMessage;
    }

    private static void SetMailHeaders(RazorMailMessage razorMessage, IMailKitMailMessage mailMessage)
    {
        mailMessage.From.Add(razorMessage.Headers.From!.ToMailboxAddress());
        mailMessage.To.AddRange(razorMessage.Headers.Recipients.Select(x => x.ToMailboxAddress()));
        mailMessage.Cc.AddRange(razorMessage.Headers.CarbonCopy.Select(x => x.ToMailboxAddress()));
        mailMessage.Bcc.AddRange(razorMessage.Headers.BlindCarbonCopy.Select(x => x.ToMailboxAddress()));
        mailMessage.ReplyTo.AddRange(razorMessage.Headers.ReplyTo.Select(x => x.ToMailboxAddress()));
    }

    private static void SetMailContent(RazorMailMessage razorMessage, IMailKitMailMessage mailMessage)
    {
        mailMessage.Subject = razorMessage.Content.Subject;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = razorMessage.Content.HtmlBody,
        };

        foreach (var item in razorMessage.Content.Attachments.Values)
        {
            var attachment =
                bodyBuilder.Attachments.Add(item.FileName, item.FileStream, ContentType.Parse(item.ContentType));
            attachment.ContentId = item.ContentId;
            attachment.IsAttachment = !item.Inline;
        }

        mailMessage.Body = bodyBuilder.ToMessageBody();
    }
}
