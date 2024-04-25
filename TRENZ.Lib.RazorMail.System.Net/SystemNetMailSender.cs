using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using NLog;

using TRENZ.Lib.RazorMail.Extensions;
using TRENZ.Lib.RazorMail.Models;
using TRENZ.Lib.RazorMail.Services;
using TRENZ.Lib.RazorMail.SystemNetExtensions;

namespace TRENZ.Lib.RazorMail;

public class SystemNetMailSender : MailSender
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    public SystemNetMailSender(MailAddress from, IEnumerable<MailAddress> to, IEnumerable<MailAddress> cc, IEnumerable<MailAddress> bcc, IEnumerable<MailAddress> replyTo, RenderedMail renderedMail)
        : base(from, to, cc, bcc, replyTo, renderedMail)
    {
    }

    public override async Task SendAsync(SmtpAccount account)
    {
        using var client = new System.Net.Mail.SmtpClient(account.Host);
        client.Port = account.Port;
        client.EnableSsl = account.TLS;
        client.Credentials = new NetworkCredential(account.Login, account.Password);

        using var mail = new System.Net.Mail.MailMessage();
        mail.BodyEncoding = Encoding.UTF8;
        mail.IsBodyHtml = true;
        mail.Subject = Subject;
        mail.Body = HtmlBodies[0];

        // TODO later version
        //if (replyTo != null)
        //    mail.ReplyToList.Add(replyTo);

        mail.From = From.ToMailAddress();

        foreach (var item in To)
            mail.To.Add(item.ToMailAddress());

        foreach (var item in Cc)
            mail.CC.Add(item.ToMailAddress());

        foreach (var item in Bcc)
            mail.Bcc.Add(item.ToMailAddress());

        foreach (var item in ReplyTo)
            mail.ReplyToList.Add(item.ToMailAddress());

        foreach (var item in Attachments)
            mail.Attachments.Add(item.ToAttachment());

        Log.Info($"Sending mail from {From} to {string.Join(", ", mail.To)}");

        Log.Info($"{nameof(mail.Subject)}: {mail.Subject}");

        Log.Debug(mail.Body);

        await client.SendAsync(mail);
    }
}
