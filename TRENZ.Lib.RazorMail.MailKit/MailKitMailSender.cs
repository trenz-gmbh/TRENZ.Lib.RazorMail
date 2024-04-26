using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

using MailKit.Net.Smtp;

using Microsoft.Extensions.Logging;

using MimeKit;

using TRENZ.Lib.RazorMail.MailKitExtensions;
using TRENZ.Lib.RazorMail.Models;
using TRENZ.Lib.RazorMail.Services;

namespace TRENZ.Lib.RazorMail;

public class MailKitMailSender(
    MailAddress from,
    IEnumerable<MailAddress> to,
    IEnumerable<MailAddress> cc,
    IEnumerable<MailAddress> bcc,
    IEnumerable<MailAddress> replyTo,
    RenderedMail renderedMail,
    ILogger<MailKitMailSender>? logger = null
)
    : MailSender(from, to, cc, bcc, replyTo, renderedMail, logger)
{
    public override async Task SendAsync(SmtpAccount account)
    {
        using var client = new SmtpClient();
        //           await client.ConnectAsync(account.Host, account.Port, account.TLS);
        await client.ConnectAsync(account.Host, account.Port, MailKit.Security.SecureSocketOptions.Auto);

        await client.AuthenticateAsync(new NetworkCredential(account.Login, account.Password));

        var mail = new MimeMessage();

        mail.Subject = Subject;

        var bodyBuilder = new BodyBuilder();

        bodyBuilder.HtmlBody = HtmlBodies[0];

        foreach (var item in Attachments)
        {
            var attachment =
                bodyBuilder.Attachments.Add(item.Filename, item.FileData, ContentType.Parse(item.ContentType));
            attachment.ContentId = item.ContentId;
            attachment.IsAttachment = !item.Inline;
        }

        mail.Body = bodyBuilder.ToMessageBody();
        mail.From.Add(From.ToMailboxAddress());

        foreach (var item in To)
            mail.To.Add(item.ToMailboxAddress());

        foreach (var item in Cc)
            mail.Cc.Add(item.ToMailboxAddress());

        foreach (var item in Bcc)
            mail.Bcc.Add(item.ToMailboxAddress());

        foreach (var item in ReplyTo)
            mail.ReplyTo.Add(item.ToMailboxAddress());

        Logger.LogInformation("Sending mail from {From} to {Recipients} (CC: {Cc}, BCC: {Bcc})", mail.From, mail.To, mail.Cc, mail.Bcc);
        Logger.LogInformation("Subject: {Subject}", mail.Subject);
        Logger.LogTrace("Body: {Body}", mail.Body);

        var formatOptions = FormatOptions.Default.Clone();

        await client.SendAsync(formatOptions, mail);

        await client.DisconnectAsync(true);
    }
}
