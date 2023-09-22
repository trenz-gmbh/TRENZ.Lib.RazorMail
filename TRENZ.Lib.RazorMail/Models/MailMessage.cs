using System.Collections.Generic;

namespace TRENZ.Lib.RazorMail.Models;

public class MailMessage<TTag>
{
    public RenderedMail RenderedMail { get; }
    public SmtpAccount SmtpAccount { get; }

    public MailAddress From { get; private set; }
    public List<MailAddress> To { get; }
    public List<MailAddress> Cc { get; } = new List<MailAddress>();
    public List<MailAddress> Bcc { get; } = new List<MailAddress>();

    public List<string> HtmlBodies { get; private set; }
    public List<MailAttachment> Attachments { get; private set; }

    public TTag Tag { get; }

    /// <summary>
    /// The e-mail subject.
    /// 
    /// However, if the template contains its own non-empty subject, it
    /// takes precedence.
    /// </summary>
    public string Subject { get; set; } = "";

    public MailMessage(MailAddress from, List<MailAddress> to, RenderedMail renderedMail, SmtpAccount smtpAccount,
        TTag tag)
    {
        From = from;
        To = to;
        RenderedMail = renderedMail;
        SmtpAccount = smtpAccount;

        HtmlBodies = new List<string> { renderedMail.HtmlBody };

        if (!string.IsNullOrWhiteSpace(renderedMail.Subject))
            Subject = renderedMail.Subject;

        Attachments = new List<MailAttachment>();
        if (renderedMail.Attachments != null)
            Attachments.AddRange(renderedMail.Attachments.Values);

        Tag = tag;
    }
}