using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using TRENZ.Lib.RazorMail.Models;

namespace TRENZ.Lib.RazorMail.Services;

public abstract class MailSender
{
    protected readonly ILogger<MailSender> Logger;

    // TODO: this isn't very useful yet; need support for includes/hierachy
    public List<string> HtmlBodies { get; private set; }
    public List<MailAttachment> Attachments { get; private set; }

    private MailSender(MailAddress from, ILogger<MailSender>? logger = null)
    {
        HtmlBodies = new List<string>();
        Attachments = new List<MailAttachment>();
        From = from;

        Logger = logger ?? NullLogger<MailSender>.Instance;
    }

    protected MailSender(
        MailAddress from,
        IEnumerable<MailAddress> to,
        IEnumerable<MailAddress> cc,
        IEnumerable<MailAddress> bcc,
        IEnumerable<MailAddress> replyTo,
        RenderedMail renderedMail,
        ILogger<MailSender>? logger = null
    )
        : this(from, logger)
    {
        To.AddRange(to);
        Cc.AddRange(cc);
        Bcc.AddRange(bcc);
        ReplyTo.AddRange(replyTo);

        HtmlBodies.Add(renderedMail.HtmlBody);

        if (!string.IsNullOrWhiteSpace(renderedMail.Subject))
            Subject = renderedMail.Subject;

        if (renderedMail.Attachments != null)
            Attachments.AddRange(renderedMail.Attachments.Values);
    }

    public MailAddress From { get; }
    public List<MailAddress> To { get; } = new();
    public List<MailAddress> ReplyTo { get; } = new();
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
