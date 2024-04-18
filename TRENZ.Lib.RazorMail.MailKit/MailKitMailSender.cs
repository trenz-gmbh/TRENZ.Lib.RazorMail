using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

using MailKit.Net.Smtp;

using MimeKit;

using NLog;

using TRENZ.Lib.RazorMail.MailKitExtensions;
using TRENZ.Lib.RazorMail.Models;
using TRENZ.Lib.RazorMail.Services;

namespace TRENZ.Lib.RazorMail;

public class MailKitMailSender : MailSender
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    public MailKitMailSender(MailAddress from, List<MailAddress> to, RenderedMail renderedMail)
        : base(from, to, renderedMail)
    {
    }

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

        // wird offenbar nicht genutzt
        //if (replyTo != null)
        //    mail.ReplyToList.Add(replyTo);

        mail.From.Add(From.ToMailboxAddress());

        foreach (var item in To)
            mail.To.Add(item.ToMailboxAddress());

        foreach (var item in Cc)
            mail.Cc.Add(item.ToMailboxAddress());

        foreach (var item in Bcc)
            mail.Bcc.Add(item.ToMailboxAddress());

        Log.Info(
            $"Sending mail from {mail.From} to {string.Join(", ", mail.To)}, cc {string.Join(", ", mail.Cc)}, bcc {string.Join(", ", mail.Bcc)}");

        Log.Info($"{nameof(mail.Subject)}: {mail.Subject}");

        Log.Debug(mail.Body);

        var formatOptions = FormatOptions.Default.Clone();

        await client.SendAsync(formatOptions, mail);

        await client.DisconnectAsync(true);
    }
}