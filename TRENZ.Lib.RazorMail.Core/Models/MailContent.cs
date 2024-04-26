using System.Collections.Generic;

namespace TRENZ.Lib.RazorMail.Models;

/// <summary>
/// This class gets returned from the isolated template after it has been
/// rendered, and separates the various portions of the e-mail.
/// </summary>
public class MailContent(string? subject, string htmlBody, IDictionary<string, MailAttachment> attachments)
{
    /// <summary>
    /// The subject of the mail.
    /// </summary>
    public string? Subject { get; private set; } = subject;

    /// <summary>
    /// The HTML body of the mail.
    /// </summary>
    public string HtmlBody { get; private set; } = htmlBody;

    /// <summary>
    /// Attachments for the mail.
    /// </summary>
    public IDictionary<string, MailAttachment> Attachments { get; private set; } = attachments;
}
