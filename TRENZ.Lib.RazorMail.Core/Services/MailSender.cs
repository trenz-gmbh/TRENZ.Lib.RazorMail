using System.Collections.Generic;
using System.Threading.Tasks;

using NLog;

using TRENZ.Lib.RazorMail.Models;

namespace TRENZ.Lib.RazorMail.Services;

public abstract class MailSender
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    // TODO: this isn't very useful yet; need support for includes/hierachy
    public List<string> HtmlBodies { get; private set; }
    public List<MailAttachment> Attachments { get; private set; }

    private MailSender(MailAddress from)
    {
        HtmlBodies = new List<string>();
        Attachments = new List<MailAttachment>();
        From = from;
    }

    protected MailSender(MailAddress from, List<MailAddress> to, RenderedMail renderedMail)
        : this(from)
    {
        To.AddRange(to);

        HtmlBodies.Add(renderedMail.HtmlBody);

        if (!string.IsNullOrWhiteSpace(renderedMail.Subject))
            Subject = renderedMail.Subject;

        if (renderedMail.Attachments != null)
            Attachments.AddRange(renderedMail.Attachments.Values);
    }

    public MailAddress From { get; }
    public List<MailAddress> To { get; } = new();
    public List<MailAddress> Cc { get; } = new();
    public List<MailAddress> Bcc { get; } = new();

    /// <summary>
    /// The e-mail subject.
    /// 
    /// However, if the template contains its own non-empty subject, it
    /// takes precedence.
    /// </summary>
    public string Subject { get; set; } = "";

    public abstract Task SendAsync(SmtpAccount account);
}