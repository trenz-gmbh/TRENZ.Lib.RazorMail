using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using MailKit.Net.Smtp;

using MimeKit;

using NLog;

using TRENZ.Lib.RazorMail.Extensions;
using TRENZ.Lib.RazorMail.Models;

namespace TRENZ.Lib.RazorMail.Services;

public class MailSender
{
    private static Logger _Log = LogManager.GetCurrentClassLogger();

    // TODO: this isn't very useful yet; need support for includes/hierachy
    public List<string> HtmlBodies { get; private set; }
    public List<MailAttachment> Attachments { get; private set; }

    private MailSender(MailAddress from)
    {
        HtmlBodies = new List<string>();
        Attachments = new List<MailAttachment>();
        From = from;
    }

    public MailSender(MailAddress from, List<MailAddress> to, RenderedMail renderedMail)
        : this(from)
    {
        To.AddRange(to);

        HtmlBodies.Add(renderedMail.HtmlBody);

        if (!string.IsNullOrWhiteSpace(renderedMail.Subject))
            Subject = renderedMail.Subject;

        if (renderedMail.Attachments != null)
            Attachments.AddRange(renderedMail.Attachments.Values);
    }

    public MailAddress From { get; private set; }
    public List<MailAddress> To { get; } = new List<MailAddress>();
    public List<MailAddress> Cc { get; } = new List<MailAddress>();
    public List<MailAddress> Bcc { get; } = new List<MailAddress>();

    /// <summary>
    /// The e-mail subject.
    /// 
    /// However, if the template contains its own non-empty subject, it
    /// takes precedence.
    /// </summary>
    public string Subject { get; set; } = "";

    public async Task SendViaSystemNetMailAsync(SmtpAccount account)
    {
        using (var client = new System.Net.Mail.SmtpClient(account.Host))
        {
            client.Port = account.Port;
            client.EnableSsl = account.TLS;
            client.Credentials = new NetworkCredential(account.Login, account.Password);

            using (var mail = new System.Net.Mail.MailMessage())
            {
                mail.BodyEncoding = Encoding.UTF8;
                mail.IsBodyHtml = true;
                mail.Subject = Subject;
                mail.Body = HtmlBodies[0];

                // wird offenbar nicht genutzt
                //if (replyTo != null)
                //    mail.ReplyToList.Add(replyTo);

                mail.From = From;

                foreach (var item in To)
                    mail.To.Add((System.Net.Mail.MailAddress)item);

                foreach (var item in Cc)
                    mail.CC.Add((System.Net.Mail.MailAddress)item);

                foreach (var item in Bcc)
                    mail.Bcc.Add((System.Net.Mail.MailAddress)item);

                if (!System.Diagnostics.Debugger.IsAttached)
                    // sich selbst als BCC, damit es im Postfach landet
                    mail.Bcc.Add(From);

                foreach (var item in Attachments)
                    mail.Attachments.Add(item);

                _Log.Info($"Sending mail from {From} to {string.Join(", ", mail.To)}");

                _Log.Info($"{nameof(mail.Subject)}: {mail.Subject}");

                _Log.Debug(mail.Body);

                await client.SendAsync(mail);
            }
        }
    }

    public async Task SendViaMailKitAsync(SmtpAccount account)
    {
        using (var client = new SmtpClient())
        {
            //           await client.ConnectAsync(account.Host, account.Port, account.TLS);
            await client.ConnectAsync(account.Host, account.Port, MailKit.Security.SecureSocketOptions.Auto);

            await client.AuthenticateAsync(new NetworkCredential(account.Login, account.Password));

            var mail = new MimeMessage();

            mail.Subject = Subject;

            var bodyBuilder = new BodyBuilder();

            bodyBuilder.HtmlBody = HtmlBodies[0];

            // CHECK content ID?
            foreach (var item in Attachments)
            {
                var attachment =
                    bodyBuilder.Attachments.Add(item.Filename, item.FileData, ContentType.Parse(item.ContentType));
                attachment.ContentId = item.ContentId;
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

            //                if (!System.Diagnostics.Debugger.IsAttached)
            // sich selbst als BCC, damit es im Postfach landet
            mail.Bcc.Add(From.ToMailboxAddress());

            _Log.Info(
                $"Sending mail from {mail.From} to {string.Join(", ", mail.To)}, cc {string.Join(", ", mail.Cc)}, bcc {string.Join(", ", mail.Bcc)}");

            _Log.Info($"{nameof(mail.Subject)}: {mail.Subject}");

            _Log.Debug(mail.Body);

            var formatOptions = FormatOptions.Default.Clone();

            await client.SendAsync(formatOptions, mail);

            await client.DisconnectAsync(true);
        }
    }
}